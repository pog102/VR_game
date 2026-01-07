import socket
import threading
import json
import random
import subprocess
import re

HOST = "0.0.0.0"
PORT = 7777


MaxNumOfClients = 5

server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((HOST, PORT))
server.listen()
local_ip = socket.gethostbyname(socket.gethostname())


with open("questions.json", "r", encoding="utf-8") as f:
    question_data = json.load(f)
random.shuffle(question_data)

def get_wifi_name():
    try:
        output = subprocess.check_output(
            ["iw", "dev"],
            stderr=subprocess.DEVNULL
        ).decode()

        match = re.search(r"ssid (.+)", output)
        return match.group(1) if match else None
    except subprocess.CalledProcessError:
        return None

print(f"Prisijunk Per: {get_wifi_name()}")
print(f"Local IP: {local_ip}")
print(f"PORT: {PORT}")

clients ={}


def SendToAllClients(message):
    message = json.dumps(message).encode()
    for conn in list(clients.keys()):  # Use list() to avoid runtime dict change issues
        conn.sendall(message+b"\n")

def Question():
    # message = {"type": "question", "variable": "What is 2 + 2?"}
    # message={
    #     "question": "Kiek bus 2 + 2?",
    #     "answer": 0,
    #     "choices": [
    #     "3",
    #     "4",
    #     "5",
    #     "6"
    #   ],
    # }   
    SendToAllClients(question_data[-1])

        
submited_answers=0

def handle_client(conn, addr):
    global submited_answers
    while True:
        try:
            data = conn.recv(1024)
            if not data:
                break
            message = data.decode()
            try:

                msg_json = json.loads(message)
                var=msg_json.get("variable")

                match msg_json.get("type"):
                    case "name":
                        clients[conn] = {"name":var, "score":0}
                        msg = {"total": (len(clients))}
                        SendToAllClients(msg)
                        print(f"{clients[conn]}")
                    case "answer":
                        clients[conn]["score"] += 1 if var == "true" else 0
                        # print(f"Answer id {msg_json.get('variable')}")
                    # conn.sendall(b"Name received!")
            except json.JSONDecodeError:
                # num=1+num
                print("\033[91mNezinau ka cia man atsiuntei...\033[0m :\n", json.dumps(json.loads(message),indent=2), "\n")

            if (len(clients) >= MaxNumOfClients):
                Question()
        except ConnectionResetError:
            break

    print("Client disconnected:", clients[conn]["name"]," from ", addr)
    clients.pop(conn, None)
    msg = {"total": (len(clients))}
    SendToAllClients(msg)
    conn.close()

while True:
    conn, addr = server.accept()
    threading.Thread(target=handle_client, args=(conn, addr), daemon=True).start()

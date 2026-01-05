import socket
import threading
import json
HOST = "0.0.0.0"
PORT = 6969

server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((HOST, PORT))
server.listen()

local_ip = socket.gethostbyname(socket.gethostname())
print(f"Local IP: {local_ip}")
print(f"PORT: {PORT}")
num=0
clients ={}
def handle_client(conn, addr):
    print(f"Client connected: {addr}")
    global num 
    while True:
        try:
            data = conn.recv(1024)
            if not data:
                break

            message = data.decode()
            try:
                msg_json = json.loads(message)
                if msg_json.get("type") == "name":
                    clients[conn] = msg_json.get("variable")
                    print(f"Player joined: {clients[conn]}")
                    conn.sendall(b"Name received!")
            except json.JSONDecodeError:
                num=1+num
                send_msg = f"New Raspberry Pi message {num}"
                conn.sendall(send_msg.encode()) 

        except ConnectionResetError:
            break

    print("Client disconnected:", addr," ", clients[conn])
    clients.pop(conn, None)
    conn.close()

while True:
    conn, addr = server.accept()
    threading.Thread(target=handle_client, args=(conn, addr), daemon=True).start()

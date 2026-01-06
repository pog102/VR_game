import socket
import json
import random
import sys

if len(sys.argv) < 2:
    print("Usage: python persistent_client.py <SERVER_IP>")
    sys.exit(1)

SERVER_IP = sys.argv[1]
SERVER_PORT = 7777

NAMES = [
    "Alice", "Bob", "Charlie", "Diana", "Eve", "Frank", "Grace", "Hugo", "Ivy", "Jack",
    "Kara", "Liam", "Mia", "Noah", "Olivia", "Paul", "Quinn", "Ruby", "Sam", "Tina",
    "Uma", "Victor", "Wendy", "Xander", "Yara", "Zane", "Aaron", "Bella", "Caleb", "Daisy",
    "Ethan", "Fiona", "Gavin", "Hannah", "Isaac", "Jade", "Kyle", "Luna", "Mason", "Nora",
    "Owen", "Piper", "Quentin", "Riley", "Sophia", "Tyler", "Ulysses", "Violet", "Wyatt", "Zoe"
]

def random_name():
    return random.choice(NAMES)

def main():
    name = random_name()
    msg = {"type": "name", "variable": name}

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.connect((SERVER_IP, SERVER_PORT))
        s.sendall(json.dumps(msg).encode() + b"\n")
        print(f"Sent name: {name}")

        # Listen for server messages
        while True:
            try:
                data = s.recv(1024)
                if not data:
                    print("Server closed the connection")
                    break
                for line in data.split(b"\n"):
                    if line.strip():
                        try:
                            msg = json.loads(line.decode())
                            print(f"Received from server: {json.dumps(msg)}")
                        except json.JSONDecodeError:
                            print("Invalid data from server:", line)
            except ConnectionResetError:
                print("Server disconnected")
                break

if __name__ == "__main__":
    main()


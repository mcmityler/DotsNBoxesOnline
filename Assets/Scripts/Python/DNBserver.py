import socket
import _thread
import threading
import time
from enum import IntEnum
import json
from datetime import datetime


clients_lock = threading.Lock()
clients = {}


class SocketMessageType(IntEnum):
    NOTHING = 120 #nothing
    CONNECTDNB = 121
    UPDATEDNB = 122
    NEWDNBCLIENT = 123
    HOSTDNBGAME = 124


#listen for messages from server..
def handle_messages(sock: socket.socket):
    print("listening to messages on new thread")
    while True:
        
        data, addr = sock.recvfrom(1024)
        data = str(data.decode("utf-8"))
        data = json.loads(data)

        #print(f'Recieved message from {addr}: {data}')
        
        #payload = "guess recieved"
        #payload = bytes(payload.encode("utf-8"))
        clients_lock.acquire()
        if(addr in clients): #check if address is already in client list, if so then do something with that client
                if(data['header'] == SocketMessageType.CONNECTDNB):
                    clients[addr]['lobbyID'] = data['lobbyID']
                    print(clients[addr]['lobbyID'])
                if(data['header'] == SocketMessageType.HOSTDNBGAME):
                    clients[addr]['lobbyID'] = data['lobbyID']
                    print(clients[addr]['lobbyID'])
        else: # if you arent part of the contact list then do the connection 
            if(data['header'] == SocketMessageType.CONNECTDNB): # if they are connecting..
                clients[addr] = {} #create new obj
                #clients[addr]['lastBeat'] = datetime.now()
                clients[addr]['lobbyID'] = data['lobbyID']
                #clients[addr]['hand'] = "null"
                message = {"header": 123,"players":[{"id":str(addr),"lobbyID":str(data['lobbyID'])},]} # tell your own client connected that you connected to it.
                m = json.dumps(message)
                sock.sendto(bytes(m,'utf8'), addr)
                print(clients[addr]['lobbyID'])

        clients_lock.release()
        


def main():
    PORT = 12345
    print("Starting server.. on PORT: " + str(PORT))
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.bind(('',PORT))

    #start new thread for listening to messages
    _thread.start_new_thread(handle_messages, (s,))

    while True:
        time.sleep(1)


if __name__ == '__main__':
    main()



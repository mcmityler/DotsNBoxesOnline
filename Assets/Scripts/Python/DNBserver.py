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
    NOTHING = 120  # nothing
    CONNECTDNB = 121
    UPDATEDNB = 122
    NEWDNBCLIENT = 123
    HOSTDNBGAME = 124
    JOINDNBGAME = 125  # sent from client to server to check if lobby ID exists
    CHECKLOBBY = 126  # sent from the server to the client to tell if lobby is existent
    STARTGAME = 127 #sent from server to client when the lobby is full on players


# listen for messages from server..
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
        if(addr in clients):  # check if address is already in client list, if so then do something with that client
            if(data['header'] == SocketMessageType.CONNECTDNB):
                clients[addr]['lobbyID'] = data['lobbyID']
                print(clients[addr]['lobbyID'])
            if(data['header'] == SocketMessageType.HOSTDNBGAME):
                clients[addr]['lobbyID'] = data['lobbyID']
                print(clients[addr]['lobbyID'])
            if(data['header'] == SocketMessageType.JOINDNBGAME):
                CheckJoin(data, addr, sock)

                print(clients[addr]['lobbyID'])
        else:  # if you arent part of the contact list then do the connection
            # if they are connecting..
            if(data['header'] == SocketMessageType.CONNECTDNB):
                clients[addr] = {}  # create new obj
                #clients[addr]['lastBeat'] = datetime.now()
                clients[addr]['lobbyID'] = data['lobbyID']
                #clients[addr]['hand'] = "null"
                # tell your own client connected that you connected to it.
                message = {"header": 123, "players": [
                    {"id": str(addr), "lobbyID": str(data['lobbyID'])}, ]}
                m = json.dumps(message)
                sock.sendto(bytes(m, 'utf8'), addr)
                print(clients[addr]['lobbyID'])

        clients_lock.release()


def CheckJoin(_data, _addr, _sock):
    _existingLobbyID = 0
    _playerCount = 2
    for c in clients:
        if(_data['lobbyID'] == clients[c]['lobbyID']):
            # count how many clients have that lobbyID
            _existingLobbyID += 1
            

    if(_existingLobbyID == 0):
        # tell client it doesnt exist
        message = {"header": 126, "lobbyExists": int(0)}
        m = json.dumps(message)
        _sock.sendto(bytes(m, 'utf8'), _addr)
        print("DOESNT EXIST")
    elif(_existingLobbyID == 1):
        # tell client lobby exists (NEED TO CHECK WHAT THE ROOM SIZE IS)
        clients[_addr]['lobbyID'] = _data['lobbyID']
        _existingLobbyID += 1 #add this player to the player count
        message = {"header": 126, "lobbyExists": int(1)}
        m = json.dumps(message)
        _sock.sendto(bytes(m, 'utf8'), _addr)
        print("DOES EXIST")
    if(_existingLobbyID == _playerCount):
        for c in clients: #tell clients in lobby game is ready to start
            if(_data['lobbyID'] == clients[c]['lobbyID']):
                message = {"header": 127, "startGame": int(1)}
                m = json.dumps(message)
                _sock.sendto(bytes(m,'utf8'), (c[0],c[1]))


def main():
    PORT = 12345
    print("Starting server.. on PORT: " + str(PORT))
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    s.bind(('', PORT))

    # start new thread for listening to messages
    _thread.start_new_thread(handle_messages, (s,))

    while True:
        time.sleep(1)


if __name__ == '__main__':
    main()

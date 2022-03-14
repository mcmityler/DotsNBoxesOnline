from re import A
import socket
import _thread
import threading
import time
from enum import IntEnum
import json
from datetime import datetime
import random


clients_lock = threading.Lock() #lock for accessing clients so you cant overwrite
clients = {} #client list of people connect and their info


class SocketMessageType(IntEnum):
    NOTHING = 120  # nothing
    CONNECTDNB = 121 #client to server telling it to join // or resetting their lobby Key
    UPDATEDNB = 122 # sent from server to client to update 
    NEWDNBCLIENT = 123 #server to client to tell it your own address 
    HOSTDNBGAME = 124 #client to server when you want to start a new game lobby
    JOINDNBGAME = 125  # sent from client to server to check if lobby Key exists
    CHECKLOBBY = 126  # sent from the server to the client to tell if lobby is existent
    STARTGAME = 127 #sent from server to client when the lobby is full on players
    SENDLOBBYKEY = 128 #from the server to client to tell what the lobbykey is (so there isnt any duplicates)
    SENDBUTTON = 130 #SENT FROM CLIENT TO SERVER tells server to send other player button pressed
    GETBUTTON =131 #SENT FROM SERVER TO CLIENT tells client what the other player pressed
    PLAYERQUIT = 132 #SENT FROM CLINET TO SERVER && SERVER TO CLIENT tells server to tell all other clients to DC


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
                clients[addr]['lobbyKey'] = data['lobbyKey']
                print(clients[addr]['lobbyKey'])
            if(data['header'] == SocketMessageType.HOSTDNBGAME):
                CreateNewLobby(addr, sock)
                clients[addr]['SizeofBoard'] = data['SizeofBoard']
                clients[addr]['playerLimit'] = data['playerLimit'] #change size of the lobby based off what the host wants.
                print(clients[addr]['lobbyKey'])
            if(data['header'] == SocketMessageType.JOINDNBGAME):
                CheckJoin(data, addr, sock)

                print(clients[addr]['lobbyKey'])
            if(data['header'] == SocketMessageType.SENDBUTTON):
                SendButtonToOtherPlayers(data,addr,sock)#send button to other clients
            if(data['header'] == SocketMessageType.PLAYERQUIT):
                for c in clients:
                    if(clients[addr]['lobbyKey'] == clients[c]['lobbyKey'] and addr != c): # send dc from server message to clients that are in lobby with someone who quit after game is over
                        clients[c]['lobbyKey'] = "null"
                        message = {"header": 132}
                        m = json.dumps(message)
                        sock.sendto(bytes(m, 'utf8'), c)

                clients[addr]['lobbyKey'] = "null"
                print(clients[addr]['lobbyKey'])

        else:  # if you arent part of the contact list then do the connection
            # if they are connecting..
            if(data['header'] == SocketMessageType.CONNECTDNB):
                clients[addr] = {}  # -----------------create new client --------------
                #clients[addr]['lastBeat'] = datetime.now()
                clients[addr]['lobbyKey'] = data['lobbyKey']
                clients[addr]['playerLimit'] = 2
                clients[addr]['SizeofBoard'] = 4

                #clients[addr]['hand'] = "null"
                # tell your own client connected that you connected to it.
                message = {"header": 123, "players": [
                    {"adress": str(addr), "lobbyKey": str(data['lobbyKey'])}, ]}
                m = json.dumps(message)
                sock.sendto(bytes(m, 'utf8'), addr)
                print(clients[addr]['lobbyKey'])

        clients_lock.release()

def CreateNewLobby(m_addr, m_sock): # create new lobby key that is Unique and not taken already
    m_keytaken = 0
    m_tempkey = ""
    m_characters = "abcdefghijkmnpqrstuvwxyz23456789"
    m_tempkey = "".join(random.choice(m_characters) for i in range(5))
    for c in clients:
        if(clients[m_addr]['lobbyKey'] == m_tempkey):
            m_keytaken = 1
    if(m_keytaken == 0):#if the key isnt taken ...
        clients[m_addr]['lobbyKey'] = m_tempkey
        #tell client what the key is.
        message = {"header": 128, "lobbyKey": str(m_tempkey)}
        m = json.dumps(message)
        m_sock.sendto(bytes(m, 'utf8'), m_addr)
    elif(m_keytaken == 1): #if the key is taken
        CreateNewLobby(m_addr,m_sock)



def CheckJoin(m_data, m_addr, m_sock):
    m_existingLobbyKey = 0
    m_playerCount = 2
    m_intToClient = 0
    m_boardSize = 4
    for c in clients:
        if(m_data['lobbyKey'] == clients[c]['lobbyKey']):
            # count how many clients have that lobbyKey
            m_existingLobbyKey += 1
            m_boardSize = clients[c]['SizeofBoard']
            m_playerCount = clients[c]['playerLimit']
            

    if(m_existingLobbyKey == 0): # if lobby doesnt exist tell client
        m_intToClient = 0 #message to client to tell its non existent
        print("DOESNT EXIST")
    elif(m_existingLobbyKey < m_playerCount): #if lobbyKey is correct and lobby isnt full then add player
        m_intToClient = 1 #message to server to tell it is real and has room
        clients[m_addr]['lobbyKey'] = m_data['lobbyKey'] #set this players lobby key
        clients[m_addr]['SizeofBoard'] = m_boardSize #set this players lobby key
        m_existingLobbyKey += 1 #add this player to the player count
        print("DOES EXIST")
    elif(m_existingLobbyKey == m_playerCount): #if the lobby is full, tell client.
        m_intToClient = 2#message to server to tell it is full
        print("FULL LOBBY")
    
    message = {"header": 126, "lobbyExists": int(m_intToClient), "SizeofBoard": int(m_boardSize), "YourPlayerNumber": int(m_existingLobbyKey)}# message sent to server whether its full,existent, or room
    print(message)
    m = json.dumps(message)
    m_sock.sendto(bytes(m, 'utf8'), m_addr)
    if(m_existingLobbyKey == m_playerCount): #if the lobby is full after adding 1 to existing players then start the game.
        m_randomPlayerOrder = MPRandomizePlayerTurns(m_playerCount)
        for c in clients: #tell clients in lobby game is ready to start
            if(m_data['lobbyKey'] == clients[c]['lobbyKey']):
                message = {"header": 127, "startGame": int(1), "RandomOrder": m_randomPlayerOrder}
                m = json.dumps(message)
                m_sock.sendto(bytes(m,'utf8'), (c[0],c[1]))

def SendButtonToOtherPlayers(m_data, m_addr, m_sock): #send other client buttons that players press
    for c in clients:
        if(clients[m_addr]['lobbyKey'] == clients[c]['lobbyKey'] and c != m_addr): #if you have the same lobby key and its not your address send it the button pressed
            message = {"header": 131, "buttonName": str(m_data['buttonName']), "isRowButton": int(m_data['isRowButton'])}
            m = json.dumps(message)
            m_sock.sendto(bytes(m,'utf8'), (c[0],c[1]))

def MPRandomizePlayerTurns(m_playerCount):
    m_randomPlayerOrder = []
    if(m_playerCount == 4):
        m_randomPlayerOrder = [1,2,3,4]
    elif(m_playerCount == 3):
        m_randomPlayerOrder = [1,2,3]
    elif(m_playerCount == 2):
        m_randomPlayerOrder = [1,2]
    
    random.shuffle(m_randomPlayerOrder)
    print(m_randomPlayerOrder)
    return m_randomPlayerOrder


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

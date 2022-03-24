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
    HOSTDNBGAME = 124 #client to server when you want to start a new game lobby
    JOINDNBGAME = 125  # sent from client to server to check if lobby Key exists
    CHECKLOBBY = 126  # sent from the server to the client to tell if lobby is existent
    STARTGAME = 127 #sent from server to client when the lobby is full on players
    SENDLOBBYKEY = 128 #from the server to client to tell what the lobbykey is (so there isnt any duplicates)
    SENDBUTTON = 130 #SENT FROM CLIENT TO SERVER tells server to send other player button pressed
    GETBUTTON =131 #SENT FROM SERVER TO CLIENT tells client what the other player pressed
    PLAYERQUIT = 132 #SENT FROM CLIENT TO SERVER && SERVER TO CLIENT tells server to tell all other clients to DC
    REPLAY = 133 #SENT FROM CLIENT TO SERVER to tell sever you want to play the game again with the same players.. Then back from server to client to tell it everyone said yes (if everyone wants to play again)
    HEARTBEAT = 135 #SENT FROM CLIENT TO THE SERVER TO TELL IT THAT THE CLIENT IS STILL CONNECTED
    TIMEDOUT = 136 #SENT FROM SERVER TO CLIENT TO TELL it that it has disconnect / timed out. 

def cleanClients(sock): #check if there are any clients to disconnect
    while True:
        #use list(clients.keys()) because it makes a temporary copy of the list to iterate through rather then using the live list
        for c in list(clients.keys()):
            if(datetime.now()-clients[c]['lastBeat']).total_seconds() > 35: #check how long between heartbeat before dropping client
                for q in list(clients.keys()): 
                    if(c != q and clients[c]['lobbyID'] != "null"): # make sure you arent checking your own address && if they are in a lobby
                        if(clients[c]['lobbyID'] == clients[q]['lobbyID']): #tell everyone in your lobby that you left
                            SendTimeoutMessage(sock , q) #tell everyone who is stll in the lobby that you left.. depending on game stage do different things client side.
                #drop the timedout client from my list.(if heartbeat has expired on server. it will likely have expired client side aswell)
                clients_lock.acquire()
                print("drop client" + str(clients[c]))
                del clients[c]
                clients_lock.release()
        
        time.sleep(1)

def SendTimeoutMessage(sock: socket.socket , playerInLobby):
    clients[playerInLobby]['replay'] = 0 #reset replay incase player timesout after game and you already clicked reset
    print("other player in lobby timed out... now do what I need to")
    timedoutPayload = {} #dictionary
    timedoutPayload['header'] = SocketMessageType.TIMEDOUT #fill in header
    p = json.dumps(timedoutPayload).encode('utf-8') #convert obj to json formatted string.
    sock.sendto(bytes(p), playerInLobby)





# -----------listen for messages from server..-----------------------
def handle_messages(sock: socket.socket):
    print("listening to messages on new thread")
    while True:

        data, addr = sock.recvfrom(1024)
        data = str(data.decode("utf-8"))
        data = json.loads(data)

        print(f'Recieved message from {addr}: {data}')

        #payload = "guess recieved"
        #payload = bytes(payload.encode("utf-8"))
        clients_lock.acquire()
        if(addr in clients):  # check if address is already in client list, if so then do something with that client
            if(data['header'] == SocketMessageType.CONNECTDNB):
                clients[addr]['lobbyKey'] = "null"
                print(clients[addr]['lobbyKey'])
            if(data['header'] == SocketMessageType.TIMEDOUT): #sent from client to server when the client is in the middle of a game and gets DCED
                clients[addr]['lobbyKey'] = "null"#reset lobby ID to null
                clients[c]['replay'] = 0 #reset wanting to play again incase someone clicked replay
            if (data['header'] == SocketMessageType.HEARTBEAT): #if header is heartbeat then update heartbeat time.
                    clients[addr]['lastBeat'] = datetime.now() #update heartbeat
                    message = {"header": 135}
                    m = json.dumps(message)
                    sock.sendto(bytes(m, 'utf8'), addr) #send a heartbeat back to the client.
            if(data['header'] == SocketMessageType.HOSTDNBGAME):
                clients[addr]['lastBeat'] = datetime.now()
                CreateNewLobby(addr, sock)
                clients[addr]['SizeofBoard'] = data['SizeofBoard']
                clients[addr]['playerLimit'] = data['playerLimit'] #change size of the lobby based off what the host wants.
                print(clients[addr]['lobbyKey'])
            if(data['header'] == SocketMessageType.JOINDNBGAME):
                clients[addr]['lastBeat'] = datetime.now()
                CheckJoin(data, addr, sock)

                print(clients[addr]['lobbyKey'])
            if(data['header'] == SocketMessageType.SENDBUTTON):
                SendButtonToOtherPlayers(data,addr,sock)#send button to other clients
            if(data['header'] == SocketMessageType.STARTGAME):
                SendStartGameMessage(addr,sock)#check if the lobby is full and if you should start
            if(data['header'] == SocketMessageType.PLAYERQUIT):
                for c in clients:
                    if(clients[addr]['lobbyKey'] == clients[c]['lobbyKey'] and addr != c): # send dc from server message to clients that are in lobby with someone who quit after game is over
                        clients[c]['lobbyKey'] = "null"
                        clients[c]['replay'] = 0 #reset wanting to play again incase someone clicked replay
                        message = {"header": 132}
                        m = json.dumps(message)
                        sock.sendto(bytes(m, 'utf8'), c)

                clients[addr]['lobbyKey'] = "null"
                print(clients[addr]['lobbyKey'])
            if(data['header'] == SocketMessageType.REPLAY):
                #what to do when you want to play again
                clients[addr]['replay'] = 1
                everyoneReady = 1
                #check if everyone in lobby wants to play again
                for c in clients:
                    if(clients[addr]['lobbyKey'] == clients[c]['lobbyKey'] and clients[c]['replay'] == 0):
                        everyoneReady = 0
                if(everyoneReady == 1):
                    m_randomPlayerOrder = MPRandomizePlayerTurns(clients[addr]['playerLimit'])
                    for c in clients:
                        if(clients[addr]['lobbyKey'] == clients[c]['lobbyKey']):
                            clients[c]['replay'] = 0 # reset wanting to play
                            message = {"header": 133, "RandomOrder": m_randomPlayerOrder} #send message telling game to restart
                            m = json.dumps(message)
                            sock.sendto(bytes(m, 'utf8'), c)



        else:  # if you arent part of the contact list then do the connection
            # if they are connecting..
            if(data['header'] == SocketMessageType.CONNECTDNB):
                clients[addr] = {}  # -----------------create new client --------------
                #clients[addr]['lastBeat'] = datetime.now()
                clients[addr]['lobbyKey'] = "null"
                clients[addr]['playerLimit'] = 2
                clients[addr]['SizeofBoard'] = 4
                clients[addr]['replay'] = 0
                clients[addr]['lastBeat'] = datetime.now()
                clients[addr]['checklist'] = []

                #clients[addr]['hand'] = "null"
                # tell your own client connected that you connected to it. (start heartbeat client side)
                message = {"header": 121 }
                m = json.dumps(message)
                sock.sendto(bytes(m, 'utf8'), addr)
                print(clients[addr]['lobbyKey'])
            if (data['header'] == SocketMessageType.HEARTBEAT): #if header is heartbeat then update heartbeat time.
                print(str(addr) + " has already been disconnected")
                #SendTimeoutMessage(sock, addr)

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
        clients[m_addr]['playerLimit'] = m_playerCount # set lobby player limit for replaybtn
        m_existingLobbyKey += 1 #add this player to the player count
        print("DOES EXIST")
    elif(m_existingLobbyKey == m_playerCount): #if the lobby is full, tell client.
        m_intToClient = 2#message to server to tell it is full
        print("FULL LOBBY")
    
    message = {"header": 126, "lobbyExists": int(m_intToClient), "SizeofBoard": int(m_boardSize), "YourPlayerNumber": int(m_existingLobbyKey)}# message sent to server whether its full,existent, or room
    print(message)
    m = json.dumps(message)
    m_sock.sendto(bytes(m, 'utf8'), m_addr)
    
def SendStartGameMessage( m_addr, m_sock):
    m_existingLobbyKey = 0
    m_playerCount = 2
    for c in clients:
        if( clients[m_addr]['lobbyKey'] == clients[c]['lobbyKey']):
            # count how many clients have that lobbyKey
            m_existingLobbyKey += 1
            m_playerCount = clients[c]['playerLimit']

    if(m_existingLobbyKey == m_playerCount): #if the lobby is full after adding 1 to existing players then start the game.
        m_randomPlayerOrder = MPRandomizePlayerTurns(m_playerCount)
        for c in clients: #tell clients in lobby game is ready to start
            if(clients[m_addr]['lobbyKey'] == clients[c]['lobbyKey']):
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
    _thread.start_new_thread(cleanClients, (s,))
    cleanClients

    while True:
        time.sleep(1)


if __name__ == '__main__':
    main()

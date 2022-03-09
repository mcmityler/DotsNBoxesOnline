using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using UnityEngine.UI;

public class SocketManager : MonoBehaviour
{

    public UdpClient udp; //new udp (send/recieve messages from aws) 
    public const string IP_ADRESS = "54.205.115.9"; //the ip the server is connected to..
    public const int PORT = 12345; //port we are using
    public ServerMessage latestServerMessage; //last message recieved from the server

    string _tempUser = ""; //what was inputed in username textbox
    string _tempPassword = "";//what was inputed in password textbox

    [SerializeField] private InputField _usernameInputText, _passwordInputText, _lobbyIDInput; //reference to user and pass input // and lobbyID input
    [SerializeField] private Text _incorrectText; //reference to incorrect text output to show when u&p are wrong or taken
    bool _correctUandP = false; //was the password and username correct/taken/wrong?
    private GameScript _gameScript; //reference to gamescript code.
    private MenuScript _menuScript;
    [SerializeField] private Text _keyHostText; //reference

    bool _multiplayerGameStarted = false;
    private enum GAMESTATE //Enum for game state / what point the game is currently at.
    { 
        STARTMENU,
        SETTINGS,
        PLAYING,
        GAMEOVER,
        RESTART,
        LOGINREGISTER,
        LOBBYMENU,
        HOSTSCREEN
    };
    private GAMESTATE _currentGamestate = GAMESTATE.STARTMENU; //what is the current gamestate
    private string _lobbyString = "null"; //current lobby string
    const string characters= "abcdefghijkmnpqrstuvwxyz23456789"; //chars for random lobby id
    void Awake(){
        _gameScript = this.GetComponent<GameScript>();///set reference to gamescript
        _menuScript = this.GetComponent<MenuScript>();///set reference to MenuScript
    }
    public void SetSOCKETGameState(string m_gamestate) //get gamestate and update socket managers gamestate from Gamescript.
    {
        _currentGamestate = (GAMESTATE)System.Enum.Parse( typeof(GAMESTATE), m_gamestate, true);
    }

    public void Update()
    {
        if(_currentGamestate == GAMESTATE.HOSTSCREEN){ //if you are in host/keylobby screen
            _keyHostText.text = "Lobby Key: " + _lobbyString;
        }
        if(_multiplayerGameStarted){
            _keyHostText.text = "Gamestarted!";
        }
    }
    public void LoginButton()
    {
        _tempPassword = _passwordInputText.text;
        _tempUser = _usernameInputText.text;
        if (_tempPassword == "" || _tempUser == "")
        { //if username or password is left blank display error message
            Debug.Log("must enter a username and password");
            _incorrectText.text = "Incorrect User/Password";
        }
        else
        {
            //SEND password and user to server and examine them.
            //Check if user name is in system

            //Check if password matches username in system
            _correctUandP = true;
            _incorrectText.text = ""; //Reset back to nothing once you enter a correct U & P
        }

        //if the user and password are correct login...
        if (_correctUandP)
        {
            _menuScript.LobbyMenuSceenButton();
        }


    }

    public void startUDP()
    {
        udp = new UdpClient(); //create new udp client
        try{
            udp.Connect(IP_ADRESS, PORT); //try to connect to server
        }catch{
            Debug.LogError("Didnt find IP address");
        }
        SendConnectMessage(); //send a connect message to server (also add player to a connect list).
        udp.BeginReceive(new AsyncCallback(OnRecieved), udp);        //wait for server messages...

    }
    void OnRecieved(IAsyncResult result){ //Waiting for a message from the server..
        //convert recived async to socket
        UdpClient socket = result.AsyncState as UdpClient;

        //new source obj
        IPEndPoint source = new IPEndPoint(IPAddress.Any, 0 );
        //get data that was passed // source passed by memory not value
        byte[] message = socket.EndReceive(result, ref source);

        //turn data recieve to string
        string returnData = Encoding.ASCII.GetString(message);

        //start looking for another message
        socket.BeginReceive(new AsyncCallback(OnRecieved), socket);
        
        //handle message you recieved.
        HandleMessagePayload(returnData);
    } 
    void SendConnectMessage(){ //tell server you have connected.. send connect message
        //Debug.Log ("sending connect message");
        var payload = new LobbyIDClientMessage{ //payload is what you are sending to server.
            header = socketMessagetype.CONNECTDNB, //header tells server what type of message it is.
            lobbyID = _lobbyString
        };
        var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
        udp.Send(data, data.Length); //send data to server you connected to in start func. 
    }

    void HandleMessagePayload(string data){ //recieved message from server now process it.
        Debug.Log("Got Message: " + data);

        var payload = JsonUtility.FromJson<BaseSocketMessage>(data); //convert data string to base socket class.
        Debug.Log("Got Message: " + payload.header); //got the header message
         //check what type of header it is then convert and do what that payload needs to do.
        switch(payload.header)
        {
            case socketMessagetype.UPDATEDNB:
                latestServerMessage= JsonUtility.FromJson<ServerMessage>(data); //convert data from base class to result class
                //ClientRecievedMessage(); //tell client it is connected to the server/got update message
                break;
            case socketMessagetype.NEWDNBCLIENT:
                ServerMessage newClientPayload = JsonUtility.FromJson<ServerMessage>(data); //convert data from base class to result class
                Debug.Log("Connected!");
                //gmScript.FillClientDetails(newClientPayload.players[0].id, newClientPayload.players[0].lobbyID);//tell client what its own ID is and what lobby it is in
                break;
            case socketMessagetype.CHECKLOBBY:
                LobbyCheckClientMessage checkLobbyPayload = JsonUtility.FromJson<LobbyCheckClientMessage>(data); //convert data from base class to result class
                Debug.Log("does lobby exist: " + checkLobbyPayload.lobbyExists);
                if(checkLobbyPayload.lobbyExists == 0){
                    _lobbyString = "null";
                }
                //gmScript.FillClientDetails(newClientPayload.players[0].id, newClientPayload.players[0].lobbyID);//tell client what its own ID is and what lobby it is in
                break;
            case socketMessagetype.STARTGAME:
                StartGameClientMessage startGamePayload = JsonUtility.FromJson<StartGameClientMessage>(data); //convert data from base class to result class
                if(startGamePayload.startGame == 1){ //start game
                    Debug.Log("StartGame!");
                    _multiplayerGameStarted = true;
                }
                //gmScript.FillClientDetails(newClientPayload.players[0].id, newClientPayload.players[0].lobbyID);//tell client what its own ID is and what lobby it is in
                break;
           /* case socketMessagetype.DISCONNECT:
                var disconnectPayload = JsonUtility.FromJson<DisconnectPayload>(data); //convert data from base class to result class
                gmScript.PlayerDisconnected(disconnectPayload.droppedID);
                
                break;*/
        }

    }
    void StartMultiplayerGame(){
        _keyHostText.text = "Gamestarted!";
    }
    public void BackToLobbyMenu(){ //called by the back button in the host key lobby.
        _menuScript.LobbyKeyScreenToggle(); //toggle visibility of the lobbykeyscreen.
        _currentGamestate = GAMESTATE.LOBBYMENU; //change gamestate back to lobbymenu
        _gameScript.SetGSGameState(_currentGamestate.ToString()); //update gamescript gamestate
        _lobbyString = "null"; //make your lobby ID back to null or nonexistent
        SendLobbyKeyMessage(true); // change your lobby ID on the server / true means send it to server not check if its real.
        Debug.Log(_lobbyString);
        _multiplayerGameStarted = false; //make sure you exit game started.
    }
    public void HostGameButton(){
        CreateLobbyKey(); //make new random lobby ID for hosted game.
        SendLobbyKeyMessage(true); //send message to server telling it your lobby ID / you are hosting a game
        _menuScript.LobbyKeyScreenToggle();
        _currentGamestate = GAMESTATE.HOSTSCREEN;
        _gameScript.SetGSGameState(_currentGamestate.ToString());
    }
    
    public void JoinGameButton(){
        _lobbyString = _lobbyIDInput.text;
        SendLobbyKeyMessage(false); //send message to server telling it what you entered for a ID guess and return a message if its correct.
    }

    void SendLobbyKeyMessage(bool m_hosting){
        
        if(m_hosting){ //if you are sending a host message
            var payload = new LobbyIDClientMessage{ //payload is what you are sending to server.
                header = socketMessagetype.HOSTDNBGAME, //header tells server what type of message it is.
                lobbyID = _lobbyString
            };
            var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
            udp.Send(data, data.Length); //send data to server you connected to in start func. 
        }
        else //if you are just trying to connect/join a game
        {
            var payload = new LobbyIDClientMessage{ //payload is what you are sending to server.
                header = socketMessagetype.JOINDNBGAME, //header tells server what type of message it is.
                lobbyID = _lobbyString
            };
            var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
            udp.Send(data, data.Length); //send data to server you connected to in start func. 
        }
      
        
    }
    void CreateLobbyKey(){ //Create a random string
        _lobbyString = "";
        for(int i=0; i<5; i++)
        {        
             _lobbyString += characters[UnityEngine.Random.Range(0, characters.Length)];
        }
        Debug.Log(_lobbyString);
    }




}
public enum socketMessagetype{
    NOTHING = 120, //nothing
    CONNECTDNB = 121, //SENT FROM CLIENT TO SERVER
    UPDATEDNB = 122, //SENT FROM SERVER TO CLIENT
    NEWDNBCLIENT = 123, //SENT FROM SERVER TO CLIENT
    HOSTDNBGAME = 124, //SENT FROM CLIENT TO SERVER tells server you are hosting a game + sends it your lobbyID
    JOINDNBGAME = 125,  //SENT FROM CLIENT TO SERVER tells server you are trying to join a game / send back if its correct
    CHECKLOBBY = 126, ///SENT FROM SERVER TO CLIENT tells client if the lobby was existent / had room to join
    STARTGAME = 127 //SENT FROM SERVER TO CLIENT  when the lobby is full on players

}

[System.Serializable] class BaseSocketMessage{
    public socketMessagetype header; //enum header. of what its doing
}
[System.Serializable] class LobbyIDClientMessage: BaseSocketMessage
{
   public string lobbyID; 
}

[System.Serializable] class LobbyCheckClientMessage: BaseSocketMessage
{
    public int lobbyExists; 
}
[System.Serializable] class StartGameClientMessage: BaseSocketMessage
{
    public int startGame; 
}
[System.Serializable] class DisconnectPayload: BaseSocketMessage
{
   public string droppedID;
}


[System.Serializable] class HeartBeatMessage: BaseSocketMessage
{
   
}

[System.Serializable] class HandClientMessage: BaseSocketMessage
{
    public string hand; 
}

[System.Serializable] public class Player{
    public string id;   
    public string lobbyID; 
    public string hand; 
}


[System.Serializable] public class ServerMessage{
    public socketMessagetype header; //enum header. of what its doing
    public Player[] players;
}
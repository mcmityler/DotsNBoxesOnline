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

    [SerializeField] private InputField _usernameInputText, _passwordInputText; //reference to user and pass input
    [SerializeField] private Text _incorrectText; //reference to incorrect text output to show when u&p are wrong or taken
    bool _correctUandP = false; //was the password and username correct/taken/wrong?
    private GameScript _gameScript; //reference to gamescript code.
    private MenuScript _menuScript;
    private enum GAMESTATE //Enum for game state / what point the game is currently at.
    { 
        STARTMENU,
        SETTINGS,
        PLAYING,
        GAMEOVER,
        RESTART,
        LOGINREGISTER
    };
    private GAMESTATE _currentGamestate = GAMESTATE.STARTMENU; //what is the current gamestate
    private string _lobbyString = "null"; //current lobby string
    const string characters= "abcdefghijkmnpqrstuvwxyz23456789"; //chars for random lobby id
    void Awake(){
        _gameScript = this.GetComponent<GameScript>();///set reference to gamescript
        _menuScript = this.GetComponent<MenuScript>();///set reference to MenuScript
    }
    public void SetGameState(string m_gamestate) //get gamestate and update socket managers gamestate from Gamescript.
    {
        switch (m_gamestate)
        {
            case "STARTMENU":
                _currentGamestate = GAMESTATE.STARTMENU;
                break;
            case "SETTINGS":
                _currentGamestate = GAMESTATE.SETTINGS;
                break;
            case "PLAYING":
                _currentGamestate = GAMESTATE.PLAYING;
                break;
            case "GAMEOVER":
                _currentGamestate = GAMESTATE.GAMEOVER;
                break;
            case "RESTART":
                _currentGamestate = GAMESTATE.RESTART;
                break;
            case "LOGINREGISTER":
                _currentGamestate = GAMESTATE.LOGINREGISTER;
                break;
        }
    }

    public void Update()
    {
        if(_currentGamestate == GAMESTATE.LOGINREGISTER){ //if you are in the login / register screen 
            
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
        var payload = new ConnectClientMessage{ //payload is what you are sending to server.
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
           /* case socketMessagetype.DISCONNECT:
                var disconnectPayload = JsonUtility.FromJson<DisconnectPayload>(data); //convert data from base class to result class
                gmScript.PlayerDisconnected(disconnectPayload.droppedID);
                
                break;*/
        }

    }

    public void HostGameButton(){
        CreateLobbyKey(); //make new random lobby ID for hosted game.
        SendHostMessage(); //send message to server telling it your lobby ID / you are hosting a game
    }
    void SendHostMessage(){
        var payload = new ConnectClientMessage{ //payload is what you are sending to server.
            header = socketMessagetype.HOSTDNBGAME, //header tells server what type of message it is.
            lobbyID = _lobbyString
        };
        var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
        udp.Send(data, data.Length); //send data to server you connected to in start func. 
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
    HOSTDNBGAME = 124 //SENT FROM CLIENT TO SERVER tells server you are hosting a game + sends it your lobbyID

}

[System.Serializable] class BaseSocketMessage{
    public socketMessagetype header; //enum header. of what its doing
}
[System.Serializable] class ConnectClientMessage: BaseSocketMessage
{
   public string lobbyID; 
}

[System.Serializable] class HostServerMessage: BaseSocketMessage
{
    public string lobbyID; 
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
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
    public ServerMessage latestServerMessage; //last message recieved from the server (Update Function)
    string _tempUserInput = ""; //what was inputed in username textbox
    string _tempPasswordInput = "";//what was inputed in password textbox
    [SerializeField] private InputField _usernameInputText, _passwordInputText, _lobbyKeyInput; //user and pass input // lobbyKey input
    [SerializeField] private Text _errorUserPassText; //incorrect text output to show when u&p are wrong or taken
    [SerializeField] private Text _errorLobbyKeyText; //incorrect text output when you enter the wrong lobby key
    private string _errorKeyMessage = ""; //lobby key text
    bool _correctUandP = false; //was the password and username correct/taken/wrong?
    private GameScript _gameScript; //game script reference
    private MenuScript _menuScript; //menu script reference
    [SerializeField] private Text _keyHostText; //Text that displays lobby key when hosting
    bool _multiplayerLobbyReady = false; //bool to start multiplayer game when lobby is ready
    bool _multiplayerGameStarted = false; //has the multiplayer game been started.
    private bool _recieveButton = false;//tell client it recieved a button (in update message do something)
    private int _tempIsRowButton = 0;//temp is row bool recieved
    private string _tempButtonName;//save the button name recieved
    private bool _joinedGame = false;
    private enum GAMESTATE //Enum for game state / what point the game is currently at.
    { 
        STARTMENU,
        SETTINGS,
        PLAYING,
        GAMEOVER,
        RESTART,
        LOGINREGISTER,
        LOBBYMENU,
        HOSTSCREEN,
        JOINSCREEN,
        PLAYINGMULTIPLAYER
    };
    private GAMESTATE _currentGamestate = GAMESTATE.STARTMENU; //what is the current gamestate
    private string _lobbyString = "null"; //current lobby string
    const string characters= "abcdefghijkmnpqrstuvwxyz23456789"; //chars for random lobby key
    void Awake(){
        _gameScript = this.GetComponent<GameScript>();///set reference to gamescript
        _menuScript = this.GetComponent<MenuScript>();///set reference to MenuScript
    }
    public void SetSOCKETGameState(string m_gamestate) //get gamestate and update socket managers gamestate from Gamescript.
    {
        _currentGamestate = (GAMESTATE)System.Enum.Parse( typeof(GAMESTATE), m_gamestate, true); //convert string to enum (true means ignore case)
    }
    void OnDestroy(){
        //When you close the client destroy UDP that if running
        if(udp != null){
            udp.Dispose();
        }
    }
    public void DestroyUDP() // call this function if you ever want to destroy UDP for client
    {
        if(udp != null){
            udp.Dispose();
        }
        
    }

    public void Update()
    {
        if(_currentGamestate == GAMESTATE.HOSTSCREEN || _currentGamestate == GAMESTATE.JOINSCREEN){ //if you are in host/keylobby screen
            _keyHostText.text = "Lobby Key: " + _lobbyString;
        }
        if(_currentGamestate == GAMESTATE.LOBBYMENU){ //if you are in the lobby menu
            _errorLobbyKeyText.text = _errorKeyMessage;
        }
        if(_joinedGame){
            _joinedGame = false;
            _menuScript.LobbyKeyScreenToggle();
        }
        if(_multiplayerLobbyReady){ //Enough players in lobby, get it ready to play.
            _multiplayerLobbyReady = false;
            _menuScript.StartMPGame(); //hide menus so you can see game board
            _gameScript.StartMultiplayerGameBoard(); //display game board and hide board settings
            _multiplayerGameStarted = true; //game has started
        }
        if(_recieveButton){ //what to do when you get a button.
            _recieveButton = false;
            _gameScript.MPButtonClicked(_tempButtonName, _tempIsRowButton);

        }
    }
    public void LoginButton() //login button on Lobby menu screen
    {
        _tempPasswordInput = _passwordInputText.text; //get password from password input
        _tempUserInput = _usernameInputText.text; //get username from username input
        if (_tempPasswordInput == "" || _tempUserInput == "")//if username or password is left blank display error message
        { 
            Debug.Log("must enter a username and password");
            _errorUserPassText.text = "Cannot leave empty";
        }
        else
        {
            //SEND password and user to server and examine them.
            //Check if user name is in system

            //Check if password matches username in system
            _correctUandP = true;
            _errorUserPassText.text = ""; //Reset back to nothing once you enter a correct U & P
        }

        //if the user and password are correct login...
        if (_correctUandP)
        {
            _menuScript.LobbyMenuSceenButton();
        }
        
        


    }

    public void startUDP() //start UDP and connect to server.
    {
        udp = new UdpClient(); //create new udp client
        try{
            udp.Connect(IP_ADRESS, PORT); //try to connect to server
        }catch{
            Debug.LogError("Didnt find IP address");
        }
        SendConnectMessage(); //send a connect message to server (also add player to a connect list).
        udp.BeginReceive(new AsyncCallback(OnRecieved), udp);    //wait for server messages...

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
            case socketMessagetype.SENDLOBBYKEY:
                LobbyKeyClientMessage newLobbyKeyPayload = JsonUtility.FromJson<LobbyKeyClientMessage>(data);
                _lobbyString = newLobbyKeyPayload.lobbyKey;
                break;
            case socketMessagetype.CHECKLOBBY:
                LobbyCheckClientMessage checkLobbyPayload = JsonUtility.FromJson<LobbyCheckClientMessage>(data); //convert data from base class to result class
               
                if(checkLobbyPayload.lobbyExists == 0){ //Message from server telling it is non existent (0)
                     Debug.Log("does lobby exist (n): " + checkLobbyPayload.lobbyExists);

                     _errorKeyMessage = "Lobby Key " + _lobbyString + " Non-Existent";
                    _lobbyString = "null";
                    
                }
                else if(checkLobbyPayload.lobbyExists == 1){ //Message from server telling it is non existent (0)
                     Debug.Log("does lobby exist (y): " + checkLobbyPayload.lobbyExists);
                    _gameScript.SetSizeofBoard(checkLobbyPayload.SizeofBoard); //set board size based off what the hosts board size is.
                    _gameScript.SetMyPlayerNumber(checkLobbyPayload.YourPlayerNumber); //set what player number I am in the lobby
                    if(_gameScript.GetPlayerSize() != checkLobbyPayload.YourPlayerNumber){ //definitely have a problem with this when you are the last player
                        _currentGamestate = GAMESTATE.JOINSCREEN;
                        _gameScript.SetGSGameState(_currentGamestate.ToString());
                        _joinedGame = true;
                    }
                    
                }
                    
                else if (checkLobbyPayload.lobbyExists == 2){  //Message from server telling it is full (2)
                    Debug.Log("Lobby is full: " + checkLobbyPayload.lobbyExists);

                    _errorKeyMessage = "Lobby " + _lobbyString + " is full";
                    _lobbyString = "null";
                }
                //gmScript.FillClientDetails(newClientPayload.players[0].id, newClientPayload.players[0].lobbyID);//tell client what its own ID is and what lobby it is in
                break;
            case socketMessagetype.STARTGAME:
                StartGameClientMessage startGamePayload = JsonUtility.FromJson<StartGameClientMessage>(data); //convert data from base class to result class
                if(startGamePayload.startGame == 1){ //start game
                    Debug.Log("StartGame!");
                    _multiplayerLobbyReady = true;
                    if(_currentGamestate == GAMESTATE.HOSTSCREEN || _currentGamestate == GAMESTATE.JOINSCREEN){ 
                        _menuScript.LobbyKeyScreenToggle();
                    }
                    _currentGamestate = GAMESTATE.PLAYINGMULTIPLAYER;
                    _gameScript.SetGSGameState(_currentGamestate.ToString());
                    _gameScript.MPSetTurnOrder(startGamePayload.RandomOrder); //set random turn order
                    Debug.Log(_currentGamestate);
                    _errorKeyMessage = "";
                }
                //gmScript.FillClientDetails(newClientPayload.players[0].id, newClientPayload.players[0].lobbyID);//tell client what its own ID is and what lobby it is in
                break;
            case socketMessagetype.GETBUTTON:
                ButtonMessage buttonPayload = JsonUtility.FromJson<ButtonMessage>(data); //convert data from ButtonMessage class to result class
                _recieveButton = true; //tell client it recieved a button (in update message do something)
                _tempButtonName = buttonPayload.buttonName; //save the button name recieved
                _tempIsRowButton = buttonPayload.isRowButton; //save the temp is row bool recieved
                break;
           /* case socketMessagetype.DISCONNECT:
                var disconnectPayload = JsonUtility.FromJson<DisconnectPayload>(data); //convert data from base class to result class
                gmScript.PlayerDisconnected(disconnectPayload.droppedID);
                
                break;*/
        }

    }
    
    public void BackToLobbyMenu(){ //called by the back button in the host key lobby.
        _menuScript.LobbyKeyScreenToggle(); //toggle visibility of the lobbykeyscreen.
        _currentGamestate = GAMESTATE.LOBBYMENU; //change gamestate back to lobbymenu
        _gameScript.SetGSGameState(_currentGamestate.ToString()); //update gamescript gamestate
        _lobbyString = "null"; //make your lobby Key back to null or nonexistent
        SendConnectMessage(); // change your lobby Key on the server / true means send it to server not check if its real.
        _multiplayerGameStarted = false; //make sure you exit game started.
    }
    public void QuitToLobbyMenuButton() //called by back to menu when playing a multiplayergame at the end.
    {
        //set lobby to null

        //tellother players I quit / to go back to lobby
    }
    public void HostGameButton(){
        _gameScript.SetSizeofBoard(-1); //tell client board size from slider, so that when you host it sends the server the right size
        SendLobbyKeyMessage(true); //send message to server telling you are hosting a game and to send back a unique lobby key
        _menuScript.LobbyKeyScreenToggle();
        _currentGamestate = GAMESTATE.HOSTSCREEN;
        _gameScript.SetGSGameState(_currentGamestate.ToString());
        _gameScript.SetMyPlayerNumber(1); //set player number inlobby (since you are hosing you are first)
    }
    
    public void JoinGameButton(){
        _lobbyString = _lobbyKeyInput.text;
        if(_lobbyString != "null"){ //make sure they didnt type null (the default lobbykey)
            SendLobbyKeyMessage(false); //send message to server telling it what you entered for a Key guess and return a message if its correct.
        }else{
            _errorKeyMessage = "Lobby Key " + _lobbyString + " Non-Existent";
        }
    }
    void SendConnectMessage(){ //tell server you have connected.. send connect message
        var payload = new LobbyKeyClientMessage{ //payload is what you are sending to server.
            header = socketMessagetype.CONNECTDNB, //header tells server what type of message it is.
            lobbyKey = _lobbyString
        };
        var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
        udp.Send(data, data.Length); //send data to server you connected to in start func. 
    }
    void SendLobbyKeyMessage(bool m_hosting){
        var m_header = socketMessagetype.NOTHING;
        if(m_hosting){ //if you are sending a host message (which will give the client back a lobbyID)
            m_header = socketMessagetype.HOSTDNBGAME;
        }
        else //if you are just trying to connect/join a game
        {
            m_header = socketMessagetype.JOINDNBGAME;
        }
        var payload = new LobbyKeyClientMessage{ //payload is what you are sending to server.
                header = m_header, //header tells server what type of message it is.
                lobbyKey = _lobbyString,
                playerLimit = _gameScript.GetPlayerSize(),
                SizeofBoard = _gameScript.GetBoardSize()
        };
        Debug.Log(payload.SizeofBoard);
        var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
        udp.Send(data, data.Length); //send data to server you connected to in start func. 
      
        
    }
    public void SendButtonMessage(string m_bName, int m_row){
        var payload = new ButtonMessage{ //payload is what you are sending to server.
            header = socketMessagetype.SENDBUTTON, //header tells server what type of message it is.
            buttonName = m_bName,
            isRowButton = m_row //send row as int because python doesnt have bools
        };
        var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
        udp.Send(data, data.Length); //send data to server you connected to in start func. 
    }


}
public enum socketMessagetype{
    NOTHING = 120, //nothing
    CONNECTDNB = 121, //SENT FROM CLIENT TO SERVER
    UPDATEDNB = 122, //SENT FROM SERVER TO CLIENT
    NEWDNBCLIENT = 123, //SENT FROM SERVER TO CLIENT
    HOSTDNBGAME = 124, //SENT FROM CLIENT TO SERVER tells server you are hosting a game + sends it your lobbyKey
    JOINDNBGAME = 125,  //SENT FROM CLIENT TO SERVER tells server you are trying to join a game / send back if its correct
    CHECKLOBBY = 126, ///SENT FROM SERVER TO CLIENT tells client if the lobby was existent / had room to join
    STARTGAME = 127, //SENT FROM SERVER TO CLIENT  when the lobby is full on players
    SENDLOBBYKEY = 128, //SENT FROM SERVER TO CLIENT to tell what the lobbykey is (so there isnt any duplicates)
    SENDBUTTON = 130, //SENT FROM CLIENT TO SERVER tells server to send other player button pressed
    GETBUTTON =131 //SENT FROM SERVER TO CLIENT tells client what the other player pressed

}

[System.Serializable] class BaseSocketMessage{
    public socketMessagetype header; //enum header. of what its doing
}
[System.Serializable] class LobbyKeyClientMessage: BaseSocketMessage
{
   public string lobbyKey; 
   public int playerLimit = 0;
   public int SizeofBoard = 4;
}

[System.Serializable] class LobbyCheckClientMessage: BaseSocketMessage
{
    public int lobbyExists; 
    public int SizeofBoard;
    public int YourPlayerNumber;
}
[System.Serializable] class StartGameClientMessage: BaseSocketMessage
{
    public int startGame; 
    public int[] RandomOrder;
}
[System.Serializable] class DisconnectPayload: BaseSocketMessage
{
   public string droppedKey;
}


[System.Serializable] class HeartBeatMessage: BaseSocketMessage
{
   
}

[System.Serializable] class ButtonMessage: BaseSocketMessage
{
    public string buttonName;
    public int isRowButton; 
}

[System.Serializable] public class Player{
    public string lobbyKey; 
}


[System.Serializable] public class ServerMessage{
    public socketMessagetype header; //enum header. of what its doing
    public Player[] players;
}
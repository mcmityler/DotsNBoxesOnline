using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
/*

THIS SCRIPT HAS EVERYTHING TO DO WITH SENDING AND RECIEVING SERVER MESSAGES
by Tyler McMillan
*/
public class SocketManager : MonoBehaviour
{
    const float VERSION = 0.25f; //what version you are playing on (sends to server to check that you are on the right version)
    public UdpClient udp; //new udp (send/recieve messages from aws) 
    public const string IP_ADRESS = "54.205.115.9"; //the ip the server is connected to..
    public const int PORT = 12345; //port we are using
    string _tempUserInput = ""; //what was inputed in username textbox
    string _tempPasswordInput = "";//what was inputed in password textbox
    [SerializeField] private TMP_InputField _usernameInputText, _passwordInputText, _lobbyKeyInput; //user and pass input // lobbyKey input
    [SerializeField] private Text _errorUserPassText; //incorrect text output to show when u&p are wrong or taken
    [SerializeField] private Text _errorLobbyKeyText; //incorrect text output when you enter the wrong lobby key
    private string _errorKeyMessage = ""; //lobby key text
    bool _correctUandP, _incorrectUandP, _clientOutofDate = false; //was the password and username correct/taken/wrong? or is the client out of date

    private GameScript _gameScript; //game script reference
    private MenuScript _menuScript; //menu script reference
    [SerializeField] private Text _keyHostText; //Text that displays lobby key when hosting
    bool _multiplayerLobbyReady = false; //bool to start multiplayer game when lobby is ready
    bool _multiplayerGameStarted = false; //has the multiplayer game been started.
    private bool _recieveButton = false;//tell client it recieved a button (in update message do something)
    private int _tempIsRowButton = 0;//temp is row bool recieved
    private string _tempButtonName;//save the button name recieved
    private bool _joinedGame, _playerQuitAfterGame, _everyoneReplay = false; //bools to hold, that you joined a game (send to join screen).// player quit game after its been played once(send back to main menu) //everyone clicked restart game for updateloop
    [SerializeField] private Text playerQuitText; //text to tell puser a player quit and thats why they dced
    private int[] _tempTurnOrder;
    private bool heartbeating, _recievedHeartbeat = false; //has the heartbeat started? // did this client recieve a heartbeat back from the server
    private bool _otherPlayerTimedout = false; //bool for update loop when someone (not you) dcs from lobby
    [SerializeField] private GameObject _MPFadeTurnTextbox;
    [SerializeField] private Animator _fadeTextAnimator;
    private int _myWins = -1;
    private int _myMatches = -1;
    private int _winnerID = -1;
    [SerializeField] private AccountStatScript _accountStatScript;

    [SerializeField] private ScoreScript _scoreScript;
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
        PLAYINGMULTIPLAYER,
        WAITINGRESTART,
        HELPSCREEN,
        CREDITSCREEN,
        MYACCOUNT,
        COLOURSCREEN
    };
    private GAMESTATE _currentGamestate = GAMESTATE.STARTMENU; //what is the current gamestate
    private string _lobbyString = "null"; //current lobby string
    const string characters = "abcdefghijkmnpqrstuvwxyz23456789"; //chars for random lobby key

    private List<string> _checklist = new List<string>(); // list that holds tasks I want to send to the server
    private DateTime heartbeatTimer = DateTime.Now; //heartbeat timer for client side before it timesout
    private bool _checklistEmpty = true; //is the checklist empty / should you send a heartbeat since its empty
    private bool _startHeartbeat = false; //start heartbeat in update loop when server gets back connect message.
    private bool _accounttaken, _accountmade = false;
    [SerializeField] private GameObject connectionStatusText, lastPingText;
    [SerializeField] private Text _errorTimedOutClientSideText;
    private float lastPingCounter = -1;
    private string _myUserID = "null";
    [SerializeField] private Text _usernameText;
    [SerializeField] private GameObject _accountDetailsObj;

    [SerializeField] private string _myColour;
    private bool _changeColour = false;

    private GAMESTATE _tempGameState = GAMESTATE.GAMEOVER;


    void Awake()
    {
        _gameScript = this.GetComponent<GameScript>();///set reference to gamescript
        _menuScript = this.GetComponent<MenuScript>();///set reference to MenuScript
    }
    public void SetSOCKETGameState(string m_gamestate) //get gamestate and update socket managers gamestate from Gamescript.
    {
        _currentGamestate = (GAMESTATE)System.Enum.Parse(typeof(GAMESTATE), m_gamestate, true); //convert string to enum (true means ignore case)
    }
    public void OnDestroy()
    {
        // call this function if you ever want to destroy UDP for client
        if (udp != null)
        {
            udp.Dispose();
            heartbeating = false;//stop heartbeat
            lastPingCounter = -1;
            _myUserID = "null";
            //Should send player to main menu.. since this means that you dced
        }
    }
    // -----------------------------------------------------------------------------------------------------------------------------
    // ------------------------------------------------UPDATE---FUNCTION------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------------------
    public void Update()
    {
        Debug.Log("My matches: " + _myMatches + " My Wins: " + _myWins);
        /*if(Input.GetKeyDown(KeyCode.W)){
            FadeTurn();
        }
        if(Input.GetKeyDown(KeyCode.S)){
            ClearFadeTurn();
        }*/
        if (_currentGamestate != GAMESTATE.STARTMENU)
        { //if you arent on the start menu erase client side timeout error text
            _errorTimedOutClientSideText.text = "";
        }
        if (_currentGamestate == GAMESTATE.HOSTSCREEN || _currentGamestate == GAMESTATE.JOINSCREEN)
        { //if you are in host/keylobby screen
            _keyHostText.text = "Lobby Key: " + _lobbyString;
        }
        if (_currentGamestate == GAMESTATE.LOBBYMENU)
        { //if you are in the lobby menu
            _errorLobbyKeyText.text = _errorKeyMessage;

        }
        if (_joinedGame)
        {
            _joinedGame = false;
            _menuScript.LobbyKeyScreenToggle();
            SendServerPayload("STARTGAME", true); //check if the lobby is full everytime after a player joins
        }
        if (_multiplayerLobbyReady)
        { //Enough players in lobby, get it ready to play.
            _multiplayerLobbyReady = false;
            _gameScript.MPSetLobbySize(_randomTurnOrder);
            _gameScript.MPSetColour(_colourList, _myColour);
            _gameScript.UpdateMPUsernames(_userList);
            _gameScript.MPSetTurnOrder(_randomTurnOrder); //set random turn order

            _menuScript.StartMPGame(); //hide menus so you can see game board
            _gameScript.StartMultiplayerGameBoard(); //display game board and hide board settings
            _multiplayerGameStarted = true; //game has started
            if (_currentGamestate == GAMESTATE.HOSTSCREEN || _currentGamestate == GAMESTATE.JOINSCREEN)
            {
                _menuScript.LobbyKeyScreenToggle();
                Debug.Log("toggle screen");
            }
            _currentGamestate = GAMESTATE.PLAYINGMULTIPLAYER;
            _gameScript.SetGSGameState(_currentGamestate.ToString());

            Debug.Log(_currentGamestate);
            _errorKeyMessage = "";
        }
        if (_recieveButton)
        { //what to do when you get a button.
            _recieveButton = false;
            _gameScript.MPButtonClicked(_tempButtonName);


        }
        if (_playerQuitAfterGame)
        { //what to do if a player quits your lobby at the end of a game.
            _playerQuitAfterGame = false;
            //Update Account Stats.
            int m_myturnNumber = _gameScript.GetMyPlayerNumber();
            _winnerID = _gameScript.GetEarlyWinner();
            Debug.Log("my turn number " + m_myturnNumber + " the game winner: " + _winnerID);
            if(m_myturnNumber == _winnerID){
                _myWins += 1;
            }
            _myMatches += 1;
            _accountStatScript.UpdateAccountStats(_myUserID, _myWins, _myMatches);
            playerQuitText.gameObject.SetActive(true);
            QuittoLobbyMenuFunc();


        }
        if (_everyoneReplay)
        {
            _everyoneReplay = false;
            _gameScript.RestartButton(false);
            _currentGamestate = GAMESTATE.PLAYINGMULTIPLAYER;
            _gameScript.SetGSGameState(_currentGamestate.ToString());
            _gameScript.MPSetTurnOrder(_tempTurnOrder); //set random turn order
            _menuScript.StartMPGame(); //hide menus so you can see game board
            _gameScript.StartMultiplayerGameBoard(); //display game board and hide board settings
        }
        if (_otherPlayerTimedout)
        {
            _otherPlayerTimedout = false;
            if (_currentGamestate == GAMESTATE.PLAYINGMULTIPLAYER || _currentGamestate == GAMESTATE.WAITINGRESTART) //if you are in an actual game.. dc and reset your lobby ID / go back to lobby menu
            {
                Debug.Log("a player timedout from the game, quit game back to lobby menu");
                SendServerPayload("TIMEDOUT", true);
                QuittoLobbyMenuFunc();//set to show lobby menu..



            }
            if (_currentGamestate == GAMESTATE.JOINSCREEN || _currentGamestate == GAMESTATE.HOSTSCREEN) //if you are host or joinscreen update player count in lobby (text)
            {
                Debug.Log("a player timedout from the lobby");
                //update player count
            }
        }
        if (_recievedHeartbeat)
        {
            _recievedHeartbeat = false;
            heartbeatTimer = DateTime.Now;
            lastPingCounter = 0;
        }
        if (heartbeating)
        {
            lastPingCounter += Time.deltaTime;

        }
        if ((DateTime.Now - heartbeatTimer).TotalSeconds > 15 && heartbeating)
        {
            _lobbyString = "null"; //set lobby to null
            _currentGamestate = GAMESTATE.STARTMENU; //go back to lobbymenu gamestate
            _gameScript.SetGSGameState(_currentGamestate.ToString());
            //go back to lobby menu screen
            _menuScript.TimedoutMPGame(); //show main menu
            _gameScript.StopMultiplayerGameBoard(); //display game board and show board settings
            OnDestroy();
            _errorTimedOutClientSideText.text = "Client timed out, Unable to connect to server\nPlease try again soon...";
            Debug.Log("timed out client side");
        }
        if (_startHeartbeat)
        {
            _startHeartbeat = false;
            if (!heartbeating)
            { //If the heart isnt beating start it
                Debug.Log("start beating");
                heartbeating = true; //make sure it doesnt start more then once
                heartbeatTimer = DateTime.Now;
                InvokeRepeating("HeartBeatMessageToServer", 0, 0.5f);  //send a repeating message to server every second to tell server that client is still connected.
            }
        }
        if (lastPingCounter == -1 && _currentGamestate == GAMESTATE.LOGINREGISTER)
        {
            SendServerPayload("CONNECTDNB", true);
        }
        LoginRegisterUpdateLoop();
        UpdateConnectStatus();
        if (_changeColour)
        {
            _changeColour = false;
            foreach (GameObject colourButton in GameObject.FindGameObjectsWithTag("ColourButton"))
            {
                colourButton.GetComponent<Image>().sprite = _unselectedColour;
            }
            
            GameObject.Find(_myColour + "ColourButton").GetComponent<Image>().sprite = _selectedColour;
            _gameScript.UsernameTextColour(_usernameText, _myColour);

        }
    }
    void LateUpdate(){
         if(_tempGameState != _currentGamestate){ //check if the gamestate changed, and then update neon buttons if they need to be updated.
            _tempGameState = _currentGamestate;
            _gameScript.ButtonNeonColour(_myColour); //Update Neon button colours.. I do this because if a button is not active but needs to change colour it wont change until you switch panels.
        }
    }
    void LoginRegisterUpdateLoop()
    {
        //if the user and password arent taken tell user you successfully created the account..
        if (_accounttaken)
        { //what to do if the username is taken when you are trying to create a new account
            _accounttaken = false;
            string _tempString = "Username '" + _usernameInputText.text + "' is taken.";
            _errorUserPassText.text = _tempString;
            FindObjectOfType<AudioManager>().Play("failSound");
        }
        if (_accountmade)
        { //what to do if the username is taken when you are trying to create a new account
            _accountmade = false;
            string _tempString = "Account creation successful!!";
            _errorUserPassText.text = _tempString;
            FindObjectOfType<AudioManager>().Play("successSound");
        }
        if (_correctUandP)
        {
            _correctUandP = false;
            if (_myUserID != "null")
            {
                _passwordInputText.text = ""; // set password input box to nothing
                _usernameInputText.text = ""; //set username input box to nothing
                _menuScript.LobbyMenuSceenButton();
                FindObjectOfType<AudioManager>().Play("successSound");
                _accountStatScript.UpdateAccountStats(_myUserID, _myWins, _myMatches);
                _usernameText.text = _myUserID;
                _gameScript.UsernameTextColour(_usernameText, _myColour);
                _accountDetailsObj.SetActive(true);

            }
            else
            {
                _errorUserPassText.text = "Already logged in, wait 15 seconds and try again";
                FindObjectOfType<AudioManager>().Play("failSound");
            }
        }
        if (_incorrectUandP)
        {
            _incorrectUandP = false;
            _errorUserPassText.text = "Username or Password was entered incorrectly";
            FindObjectOfType<AudioManager>().Play("failSound");
        }
        if (_myUserID == "null")
        {
            _accountDetailsObj.SetActive(false);
        }
        if(_clientOutofDate == true){
            _clientOutofDate = false;
            _errorUserPassText.text = "Client Version is out of date";
            FindObjectOfType<AudioManager>().Play("failSound");
        }
    }

    public void startUDP() //start UDP and connect to server.
    {
        udp = new UdpClient(); //create new udp client
        try
        {
            udp.Connect(IP_ADRESS, PORT); //try to connect to server
        }
        catch
        {
            Debug.LogError("Didnt find IP address");
        }
        SendServerPayload("CONNECTDNB", true); //send a connect message to server (also add player to a connect list).
        udp.BeginReceive(new AsyncCallback(OnRecieved), udp);    //wait for server messages...


    }
    void OnRecieved(IAsyncResult result)
    { //Waiting for a message from the server..
        //convert recived async to socket
        UdpClient socket = result.AsyncState as UdpClient;

        //new source obj
        IPEndPoint source = new IPEndPoint(IPAddress.Any, 0);
        //get data that was passed // source passed by memory not value
        byte[] message = socket.EndReceive(result, ref source);

        //turn data recieve to string
        string returnData = Encoding.ASCII.GetString(message);

        //start looking for another message
        socket.BeginReceive(new AsyncCallback(OnRecieved), socket);

        //handle message you recieved.
        HandleMessagePayload(returnData);
    }
    int[] _randomTurnOrder;
    string[] _userList, _colourList;
    void HandleMessagePayload(string data)
    { //recieved message from server now process it.
        Debug.Log("Got Message: " + data);

        var payload = JsonUtility.FromJson<BaseSocketMessage>(data); //convert data string to base socket class.
        Debug.Log("Got Message: " + payload.header); //got the header message
                                                     //check what type of header it is then convert and do what that payload needs to do.
        switch (payload.header)
        {
            case socketMessagetype.HEARTBEAT:
                _recievedHeartbeat = true;
                //when you recieve a heartbeat from the server tell client to update its timer / counter to tell it its connected (ping)
                //
                //                            *************************************************
                //                            *************************************************
                //
                break;
            case socketMessagetype.CONNECTDNB: //Did the client connect to the server (start heartbeat message)
                Debug.Log("Connected!");
                _startHeartbeat = true;

                //gmScript.FillClientDetails(newClientPayload.players[0].id, newClientPayload.players[0].lobbyID);//tell client what its own ID is and what lobby it is in
                break;
            case socketMessagetype.SENDLOBBYKEY: //received back if you send server  host message
                LobbyKeyClientMessage newLobbyKeyPayload = JsonUtility.FromJson<LobbyKeyClientMessage>(data); //message from server with lobby ID for your hosted lobby
                _lobbyString = newLobbyKeyPayload.lobbyKey; //set lobby ID on client
                Debug.Log("Hosting!");
                _recievedHeartbeat = true; //recieved heartbeat since you sent a host msg instead of heartbeat
                if (_checklist[0] == "HOSTDNBGAME") //if the first item in the list is hostdnb game remove it from the list.
                {
                    _checklist.RemoveAt(0);
                }
                break;
            case socketMessagetype.CHECKLOBBY:
                LobbyCheckClientMessage checkLobbyPayload = JsonUtility.FromJson<LobbyCheckClientMessage>(data); //convert data from base class to result class

                if (checkLobbyPayload.lobbyExists == 0)
                { //Message from server telling it is non existent (0)
                    Debug.Log("does lobby exist (n): " + checkLobbyPayload.lobbyExists);

                    _errorKeyMessage = "Lobby Key " + _lobbyString + " Non-Existent";
                    _lobbyString = "null";

                }
                else if (checkLobbyPayload.lobbyExists == 1)//Message from server telling it has space and exists
                {
                    Debug.Log("does lobby exist (y): " + checkLobbyPayload.lobbyExists);
                    _gameScript.SetSizeofBoard(checkLobbyPayload.SizeofBoard); //set board size based off what the hosts board size is.
                    _gameScript.SetMyPlayerNumber(checkLobbyPayload.YourPlayerNumber); //set what player number I am in the lobby
                    //if(_gameScript.GetPlayerSize() != checkLobbyPayload.YourPlayerNumber){ //if you arent the last player send player to waiting screen
                    _currentGamestate = GAMESTATE.JOINSCREEN;
                    _gameScript.SetGSGameState(_currentGamestate.ToString());
                    _joinedGame = true;
                    _lobbyKeyInput.text = "";
                    //}

                }

                else if (checkLobbyPayload.lobbyExists == 2)
                {  //Message from server telling it is full (2)
                    Debug.Log("Lobby is full: " + checkLobbyPayload.lobbyExists);

                    _errorKeyMessage = "Lobby " + _lobbyString + " is full";
                    _lobbyString = "null";
                }
                _recievedHeartbeat = true; //tell client it got a message from server
                if (_checklist[0] == "JOINDNBGAME") //if the first to do list item was join game remove it from the list.
                {
                    _checklist.RemoveAt(0);
                }
                //gmScript.FillClientDetails(newClientPayload.players[0].id, newClientPayload.players[0].lobbyID);//tell client what its own ID is and what lobby it is in
                break;
            case socketMessagetype.STARTGAME:
                StartGameServerMessage startGamePayload = JsonUtility.FromJson<StartGameServerMessage>(data); //convert data from base class to result class
                if (startGamePayload.startGame == 1)
                { //start game

                    //Players usernames are passed through as a userlist from server when the game starts.
                    Debug.Log(startGamePayload.UserList.ToString());

                    //From here i need to use the user list to populate the user names in the gamescript
                    _randomTurnOrder = startGamePayload.RandomOrder;
                    _userList = startGamePayload.UserList;
                    _colourList = startGamePayload.ColourList;
                    Debug.Log("StartGame!");
                    _multiplayerLobbyReady = true;


                }
                //gmScript.FillClientDetails(newClientPayload.players[0].id, newClientPayload.players[0].lobbyID);//tell client what its own ID is and what lobby it is in
                break;
            case socketMessagetype.GETBUTTON:
                ButtonMessage buttonPayload = JsonUtility.FromJson<ButtonMessage>(data); //convert data from ButtonMessage class to result class
                _recieveButton = true; //tell client it recieved a button (in update message do something)
                _tempButtonName = buttonPayload.buttonName; //save the button name recieved
                _tempIsRowButton = buttonPayload.isRowButton; //save the temp is row bool recieved
                break;
            case socketMessagetype.PLAYERQUIT:
                _playerQuitAfterGame = true;
                break;
            case socketMessagetype.REPLAY:
                RestartServerMessage restartGamePayload = JsonUtility.FromJson<RestartServerMessage>(data);
                _tempTurnOrder = restartGamePayload.RandomOrder;
                _everyoneReplay = true;
                break;
            case socketMessagetype.TIMEDOUT:
                //what to do if other player in lobby timed out...
                _otherPlayerTimedout = true;
                break;
            case socketMessagetype.USERTAKEN:
                //what to do if username you entered was taken on create account.
                _accounttaken = true;
                break;
            case socketMessagetype.USERAVAILABLE:
                //what to do if username you entered was available on create account.
                _accountmade = true;
                break;
            case socketMessagetype.LOGINACCOUNT:
                //what to do if username and password were right
                LoginCreateAccountMessage loginpayload = JsonUtility.FromJson<LoginCreateAccountMessage>(data);
                _myUserID = loginpayload.UserID;
                _myColour = loginpayload.MyColour;
                _myWins = loginpayload.Wins;
                _myMatches = loginpayload.Matches;
                _correctUandP = true;
                break;
            case socketMessagetype.LOGINFAILED:
                //what to do if username and password were right
                _incorrectUandP = true;
                break;
            case socketMessagetype.ALREADYLOGGEDIN:
                //what to do if username and password were right but already logged into
                _correctUandP = true;
                break;
            case socketMessagetype.OLDVERSION:
                //what to do the client is out of date
                Debug.Log("client is out of date");
                _clientOutofDate = true;
                
                break;
            case socketMessagetype.COLOURCHANGE:
                //What to do if you changed your colour
                ColourMessage colourPayload = JsonUtility.FromJson<ColourMessage>(data);
                _myColour = colourPayload.MyColour;
                _changeColour = true;
                break;
        }

    }
    void SendServerPayload(string m_messageType, bool m_baseMessage)
    {
        bool m_lobbykeyMessage = false;
        if (!m_baseMessage)
        {
            switch (m_messageType)
            {
                case "REPLAY":
                    var replayPayload = new RestartClientMessage
                    { //payload is what you are sending to server.
                        header = socketMessagetype.REPLAY, //header tells server what type of message it is.
                        replay = 1 //tells server yes (1 = true since python doesnt have booleans)
                    };
                    var replayData = Encoding.ASCII.GetBytes(JsonUtility.ToJson(replayPayload)); //convert payload to transmittable data.(json file)
                    if (udp != null)
                    {
                        udp.Send(replayData, replayData.Length); //send data to server you connected to in start func.
                    }
                    break;
                case "HOSTDNBGAME":
                    m_lobbykeyMessage = true;
                    break;
                case "JOINDNBGAME":
                    m_lobbykeyMessage = true;
                    break;
                case "PLAYERQUIT":
                    var payload = new GameoverMessage
                    { //payload is what you are sending to server.
                        header = socketMessagetype.PLAYERQUIT,
                        Winner = _winnerID
                    };
                    var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
                    if (udp != null)
                    {
                        Debug.Log(m_messageType);
                        udp.Send(data, data.Length); //send data to server you connected to in start func. 
                    }
                    break;

            }
        }
        else if (m_baseMessage) //if its a base server message conver string passes to header and then send base socket message to server.
        {
            socketMessagetype m_tempmessagetype = (socketMessagetype)System.Enum.Parse(typeof(socketMessagetype), m_messageType, true); //switch string into socket message type. / header
            var payload = new BaseSocketMessage
            { //payload is what you are sending to server.
                header = m_tempmessagetype
            };
            var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
            if (udp != null)
            {
                Debug.Log(m_messageType);
                udp.Send(data, data.Length); //send data to server you connected to in start func. 
            }
        }

        if (m_lobbykeyMessage)
        {
            socketMessagetype m_tempmessagetype = (socketMessagetype)System.Enum.Parse(typeof(socketMessagetype), m_messageType, true);
            var payload = new LobbyKeyClientMessage
            { //payload is what you are sending to server.
                header = m_tempmessagetype, //header tells server what type of message it is.
                lobbyKey = _lobbyString,
                playerLimit = _gameScript.GetLobbySize(), //tell server and other players how many players in the game if host.
                SizeofBoard = _gameScript.GetBoardSize()
            };
            Debug.Log(payload.SizeofBoard);
            var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
            if (udp != null)
            {
                udp.Send(data, data.Length); //send data to server you connected to in start func. 
            }
        }

    }

    private bool DoChecklist()
    {
        if (_checklist.Count == 0) //if the checklist has nothing in it
        {
            _checklistEmpty = true;
            return true; //if the list isnt populated.
        }
        else if (_checklist.Count >= 1) //if the checklist has atleast one item in it.
        {

            switch (_checklist[0])
            {
                case "HOSTDNBGAME":
                    SendServerPayload("HOSTDNBGAME", false);
                    break;
                case "JOINDNBGAME":
                    SendServerPayload("JOINDNBGAME", false);
                    break;
            }
        }
        return false; //if the checklist has one item

    }
    void HeartBeatMessageToServer()
    { //tell server client is connected every second
        if (!heartbeating)
        { //stop heart if it is dced from losing connection on client side
            CancelInvoke();
            Debug.Log("stop beating");
            return;
        }
        if (DoChecklist())//do first item on checklist if anything is on it. //if there were no other server messages to send, send heartbeat.
        {
            var payload = new BaseSocketMessage
            {
                header = socketMessagetype.HEARTBEAT,
            };
            var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload));
            if (udp != null)
            {
                udp.Send(data, data.Length);
            }

        }
        Debug.Log("heartbeating message");

    }
    public void ExitGameButton()
    {
        if (_currentGamestate == GAMESTATE.PLAYINGMULTIPLAYER)
        {
            _winnerID = _gameScript.GetEarlyWinner();
            
            
            SendServerPayload("PLAYERQUIT", false);
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
        else if (lastPingCounter == -1)
        {
            _errorUserPassText.text = "Not connected to server";
        }
        else
        {
            //SEND password and user to server and examine them.
            //Check if user name is in system

            var payload = new LoginCreateAccountMessage
            { //payload is what you are sending to server.
                header = socketMessagetype.LOGINACCOUNT, //header tells server what type of message it is.
                UserID = _tempUserInput,
                PasswordID = _tempPasswordInput,
                VersionID = VERSION
            };
            var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
            if (udp != null)
            {
                udp.Send(data, data.Length); //send data to server you connected to in start func. 
            }
            //Check if password matches username in system
            //_correctUandP = true;
            _errorUserPassText.text = ""; //Reset back to nothing once you enter a correct U & P
        }

        //if the user and password are correct login...


        playerQuitText.gameObject.SetActive(false); //hide player quit text 


    }
    public void CreateProfileButton() //create account button on Lobby menu screen
    {
        _tempPasswordInput = _passwordInputText.text; //get password from password input
        _tempUserInput = _usernameInputText.text; //get username from username input
        if (_tempPasswordInput == "" || _tempUserInput == "")//if username or password is left blank display error message
        {
            Debug.Log("must enter a username and password");
            _errorUserPassText.text = "Cannot leave empty";
        }
        else if (lastPingCounter == -1)
        {
            _errorUserPassText.text = "Not connected to server";
        }
        else
        {
            //SEND password and user to server and examine them.
            //Check if username is available
            var payload = new LoginCreateAccountMessage
            { //payload is what you are sending to server.
                header = socketMessagetype.CREATEACCOUNT, //header tells server what type of message it is.
                UserID = _tempUserInput,
                PasswordID = _tempPasswordInput
            };
            var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
            if (udp != null)
            {
                udp.Send(data, data.Length); //send data to server you connected to in start func. 
            }
            //_UandPNotTaken = true;
            _errorUserPassText.text = ""; //Reset back to nothing once you enter a correct U & P
        }



        playerQuitText.gameObject.SetActive(false); //hide player quit text 


    }
    public void SendGameoverMessage(int m_winner){
        var payload = new GameoverMessage
        { //payload is what you are sending to server.
            header = socketMessagetype.GAMEOVER, //header tells server what type of message it is.
            Winner = m_winner //send buttons name
        };
        var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
        if (udp != null)
        {
            udp.Send(data, data.Length); //send data to server you connected to in start func. 
        }
        _myWins += m_winner; //if you won, add 1, if you lost add 0
        _myMatches += 1;
        _accountStatScript.UpdateAccountStats(_myUserID, _myWins, _myMatches);
    }
    public void SendButtonMessage(string m_bName)
    {
        var payload = new ButtonMessage
        { //payload is what you are sending to server.
            header = socketMessagetype.SENDBUTTON, //header tells server what type of message it is.
            buttonName = m_bName //send buttons name
        };
        var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
        if (udp != null)
        {
            udp.Send(data, data.Length); //send data to server you connected to in start func. 
        }
    }
    public void RecivedButtonMessage()
    {
        SendServerPayload("BUTTONRECEIVED", true);
    }
    public void BackToLobbyMenu()
    { //called by the back button in the host key lobby.
        _menuScript.LobbyKeyScreenToggle(); //toggle visibility of the lobbykeyscreen.
        _currentGamestate = GAMESTATE.LOBBYMENU; //change gamestate back to lobbymenu
        _gameScript.SetGSGameState(_currentGamestate.ToString()); //update gamescript gamestate
        _lobbyString = "null"; //make your lobby Key back to null or nonexistent
        SendServerPayload("CONNECTDNB", true); // change your lobby Key on the server / true means send it to server not check if its real.
        _multiplayerGameStarted = false; //make sure you exit game started.
        playerQuitText.gameObject.SetActive(false); //hide player quit text
    }
    public void QuitToLobbyMenuButton() //called by back to menu when playing a multiplayergame at the end.
    {
        QuittoLobbyMenuFunc();
        //tellother players I quit / to go back to lobby
        SendServerPayload("PLAYERQUIT", true);

    }
    private void QuittoLobbyMenuFunc()
    {
        _lobbyString = "null"; //set lobby to null
        _currentGamestate = GAMESTATE.LOBBYMENU; //go back to lobbymenu gamestate
        _gameScript.SetGSGameState(_currentGamestate.ToString());
        //go back to lobby menu screen
        _menuScript.StopMPGame(); //show menus
        _gameScript.StopMultiplayerGameBoard(); //display game board and hide board settings
    }
    public void HostGameButton()
    {
        _gameScript.SetSizeofBoard(-1); //tell client board size from slider, so that when you host it sends the server the right size
        _checklist.Add("HOSTDNBGAME");
        //SendServerPayload("HOSTDNBGAME", false); //send message to server telling you are hosting a game and to send back a unique lobby key
        _menuScript.LobbyKeyScreenToggle();
        _currentGamestate = GAMESTATE.HOSTSCREEN;
        _gameScript.SetGSGameState(_currentGamestate.ToString());
        _gameScript.SetMyPlayerNumber(1); //set player number inlobby (since you are hosing you are first)
        playerQuitText.gameObject.SetActive(false); //hide player quit text
    }

    public void JoinGameButton()
    {
        _lobbyString = _lobbyKeyInput.text;
        if (_lobbyString != "null" && _lobbyString.Length >= 5)
        { //make sure they didnt type null (the default lobbykey)
            _checklist.Add("JOINDNBGAME");
        }
        else if (_lobbyString.Length < 5)
        {
            _errorKeyMessage = "Lobby Key " + _lobbyString + " Non-Existent";
        }
        playerQuitText.gameObject.SetActive(false); //hide player quit text
    }

    public void RestartMPButton()
    {
        _currentGamestate = GAMESTATE.WAITINGRESTART;
        _gameScript.SetGSGameState(_currentGamestate.ToString());
        _scoreScript.HideScoreBoard();
        SendServerPayload("REPLAY", false);
    }
    private void UpdateConnectStatus()
    {
        lastPingText.GetComponent<Text>().text = lastPingCounter.ToString() + " Seconds";
        if (lastPingCounter == -1 || lastPingCounter > 8 && lastPingCounter < 12)
        {
            //the heart hasnt connected to the server yet
            connectionStatusText.GetComponent<Text>().text = "Pending...";
            connectionStatusText.GetComponent<Text>().color = Color.yellow;
        }
        if (lastPingCounter < 8 && lastPingCounter > 0)
        {
            //the heart is connected and alive
            connectionStatusText.GetComponent<Text>().text = "Connected!";
            connectionStatusText.GetComponent<Text>().color = Color.green;
        }
        if (lastPingCounter >= 12)
        {
            //the heart message isnt coming back from the server for long enough
            connectionStatusText.GetComponent<Text>().text = "Disconnected!";
            connectionStatusText.GetComponent<Text>().color = Color.red;
        }
    }
    [SerializeField] private Sprite _unselectedColour, _selectedColour;
    public void SetColourButtonSelected()
    { 
        foreach (GameObject colourButton in GameObject.FindGameObjectsWithTag("ColourButton")) //reset to unselected, so if you are logging into a different account, it wont have your last colour selected 
        {
                colourButton.GetComponent<Image>().sprite = _unselectedColour;
        }
        GameObject.Find(_myColour + "ColourButton").GetComponent<Button>().image.sprite = _selectedColour;

    }
    public void SendColourMessage(string m_colourName)
    { //called by colour change buttons Multiplayer

        if (_myColour != m_colourName) //dont send server a message if you already have that colour selected.
        {
            var payload = new ColourMessage
            { //payload is what you are sending to server.
                header = socketMessagetype.COLOURCHANGE, //header tells server what type of message it is.
                MyColour = m_colourName
            };
            var data = Encoding.ASCII.GetBytes(JsonUtility.ToJson(payload)); //convert payload to transmittable data.(json file)
            if (udp != null)
            {
                udp.Send(data, data.Length); //send data to server you connected to in start func. 
            }
        }
    }
    
    public void FadeTurn(){
        _MPFadeTurnTextbox.GetComponent<TMP_Text>().text = _myUserID + "'s turn.";
        _MPFadeTurnTextbox.SetActive(true);
        _fadeTextAnimator.SetBool("Fade", true); 

    }
    public void ClearFadeTurn(){
        _fadeTextAnimator.SetBool("Fade", false);
    }

}
public enum socketMessagetype
{
    NOTHING = 120, //nothing
    CONNECTDNB = 121, //SENT FROM CLIENT TO SERVER
    UPDATEDNB = 122, //SENT FROM SERVER TO CLIENT
    NEWDNBCLIENT = 123, //SENT FROM SERVER TO CLIENT
    HOSTDNBGAME = 124, //SENT FROM CLIENT TO SERVER tells server you are hosting a game + sends it your lobbyKey
    JOINDNBGAME = 125,  //SENT FROM CLIENT TO SERVER tells server you are trying to join a game / send back if its correct
    CHECKLOBBY = 126, ///SENT FROM SERVER TO CLIENT tells client if the lobby was existent / had room to join
    STARTGAME = 127, //SENT FROM CLIENT TO SERVER to check if the lobby is ready to send start message to all players.. ALSO SENT FROM SERVER TO CLIENT but only if its ready to start.
    SENDLOBBYKEY = 128, //SENT FROM SERVER TO CLIENT to tell what the lobbykey is (so there isnt any duplicates)
    SENDBUTTON = 130, //SENT FROM CLIENT TO SERVER tells server to send other player button pressed
    GETBUTTON = 131, //SENT FROM SERVER TO CLIENT tells client what the other player pressed
    PLAYERQUIT = 132, //SENT FROM CLIENT TO SERVER && SERVER TO CLIENT tells server to tell all other clients to DC
    REPLAY = 133, //SENT FROM CLIENT TO SERVER to tell sever you want to play the game again with the same players.. Then back from server to client to tell it everyone said yes (if everyone wants to play again)
    HEARTBEAT = 135, //SENT FROM CLIENT TO THE SERVER TO TELL IT THAT THE CLIENT IS STILL CONNECTED
    TIMEDOUT = 136, //SENT FROM SERVER TO CLIENT TO TELL it that it has disconnect / timed out.
    BUTTONRECEIVED = 137,
    USERAVAILABLE = 138,//SENT FROM SERVER TO CLIENT to tell client that the username was successfully made.
    USERTAKEN = 139, //SENT FROM SERVER TO CLIENT to tell client that the username was taken.
    CREATEACCOUNT = 140,
    LOGINACCOUNT = 141, //SENT FROM SERVER TO CLIENT to tell client login successful // SENT FROM CLIENT TO SERVER to tell server client is trying to log in
    LOGINFAILED = 142, //SENT FROM SERVER TO CLIENT to tell client login failed
    ALREADYLOGGEDIN = 143, //SENT FROM SERVER TO CLIENT to tell client that that account is already logged in on.
    OLDVERSION = 144, //SENT FROM SERVER TO CLIENT WHEN THE CLIENTS VERSION IS OUT OF DATE
    COLOURCHANGE = 145, //SENT FROM SERVER TO CLIENT when the server gets colour change request // CLIENT TO SERVER when you want to change colours
    GAMEOVER = 150 //SENT FROM THE CLIENT TO SERVER WHEN THE GAME IS OVER
}

[System.Serializable]
class BaseSocketMessage
{
    public socketMessagetype header; //enum header. of what its doing
}
[System.Serializable]
class LobbyKeyClientMessage : BaseSocketMessage
{
    public string lobbyKey;
    public int playerLimit = 0; //send lobby limit to server
    public int SizeofBoard = 4; //send size to server
}

[System.Serializable]
class LobbyCheckClientMessage : BaseSocketMessage
{
    public int lobbyExists;
    public int SizeofBoard; //get size from server
    public int YourPlayerNumber; //get number froms erver
}
[System.Serializable]
class StartGameServerMessage : BaseSocketMessage
{
    public int startGame;
    public int[] RandomOrder;
    public string[] UserList;
    public string[] ColourList;

}
[System.Serializable]
class RestartClientMessage : BaseSocketMessage
{
    public int replay;
}
[System.Serializable]
class RestartServerMessage : BaseSocketMessage
{
    public int[] RandomOrder;
}
[System.Serializable]
class GameoverMessage : BaseSocketMessage
{
    public int Winner;
}
[System.Serializable]
class LoginCreateAccountMessage : BaseSocketMessage
{
    public string UserID;
    public string PasswordID;
    public float VersionID;
    public string MyColour;
    public int Wins;
    public int Matches;
    
}
[System.Serializable]
class ColourMessage : BaseSocketMessage
{
    public string MyColour;
}

[System.Serializable]
class ButtonMessage : BaseSocketMessage
{
    public string buttonName;
    public int isRowButton;
}

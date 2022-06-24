using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Linq;
using TMPro;
/*

THIS SCRIPT HAS EVERYTHING TO DO WITH THE ACTUAL GAME LOOP
by Tyler McMillan
*/
public class GameScript : MonoBehaviour
{

    //----------------------------VARIABLES-------------------------------------
    private SocketManager _socketManager; //ref to socket manager script 
    [Range(2.0F, 8.0F)][SerializeField] private int _boardSize = 4; //Board size in game, (ie 4x4, max it can go is 8x8 before it goes off screen) (range is what shows in the slider 2-8)
    public GameObject cornerButtonObj; //Prefab for buttons that surround the box
    public GameObject box; //Prefab of center of buttons aka box. Also hold what buttons surround it and need to be pressed to change its color.
    private GameObject _canvas; //Canvas obj (where all the UI is)
    [SerializeField] private GameObject _backgroundObj; //Background panel of the entire canvas (also where the gameboard is drawn)
    [SerializeField] private GameObject _boardSettingsObj, _localPlayerNamesObj; //Settings panel for picking board size and player amount //local player name inputs obj (toggles visibility)
    [SerializeField] private Slider _boardSizeSlider, _mpBoardSizeSlider; //slider on board setting panel to choose size of board // slider on multiplayer menu
    [SerializeField] private TMP_Text _boardSizeText, _mpBoardSizeText;//Text that displays slider count// text for slider on multiplayer menu
    [SerializeField] private TMP_Text _numberOfPlayerText, _mpLobbySizeText; //text that displays how many people are playing on settings screen // multiplayer lobby menu
    [SerializeField] private TMP_InputField[] _localPlayerNameInput; //player 1-4 name input for local games
    private string[] _playerNames = new string[] { "Player 1", "Player 2", "Player 3", "Player 4" }; //player 1-4 local names.
    [SerializeField] private Animator _localNameInputAnimator; //local name input animator (changes how many names on local input screen)
    private int _numberOfPlayers = 2; //how many players are allowed in your lobby
    private bool _localGame = true; //are you playing a local game 

    [SerializeField] private Animator _turnOrderAnimator; //turn order animator
    [SerializeField] private Animator _ingameScoreboardAnimator;
    private int _myPlayerNumberMP = 0; //turn number / what player you are in the lobby
    private int[] _whosTurn = new int[] { 0, 0, 0, 0 }; //holds randomized turn order
    private int _turnRotation = 0; //placeholder for what turn you are currently on
    [SerializeField] private TMP_Text[] _playerTurnOrderText; //text boxes that display whos turn it is in the turn order 
    private GameBoard _gameBoard = new GameBoard(); //gameboard is every button, box and if the buttons have been clicked yet.
    [SerializeField] private ScoreScript _scoreScript; //reference to score script
    private enum GAMESTATE
    { //Enum for game state / what point the game is currently at.
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

    private GAMESTATE _currentGamestate = GAMESTATE.STARTMENU; //current gamestate of game


    // -------------------------------------COLOUR VARIABLES----------------------------
    List<int> _redctr, _bluectr, _yellowctr, _greenctr, _purplectr, _orangectr, _lightbluectr; //different lists to count if a colour is taken, when setting other players colours in multiplayer
    private Color32[] _playerColors = new Color32[] { new Color32(0, 16, 255, 255), new Color32(255, 0, 10, 255), new Color32(37,181,27, 255), new Color32(173, 161, 0, 255) }; // players colors on the board. (in order first - fourth player)
    private Color32 _blue = new Color32(0, 16, 255, 255);
    private Color32 _red = new Color32(255, 0, 10, 255);
    private Color32 _green = new Color32(37,181,27, 255);
    private Color32 _yellow = new Color32(173, 161, 0, 255);
    private Color32 _purple = new Color32(225, 0, 255, 255);
    private Color32 _orange = new Color32(255, 129, 0, 255);
    private Color32 _lightblue = new Color32(63, 163, 163, 255);

    //----------------------------FUNCTIONS-------------------------------------
    void Awake()
    {
        _canvas = GameObject.FindGameObjectWithTag("Canvas"); //give canvas reference to the canvas obj
        _socketManager = this.GetComponent<SocketManager>();//set reference to socket script
        _socketManager.SetSOCKETGameState(_currentGamestate.ToString()); //set gamestate in socketmanager
        
       
    }
    void Update()
    {
        if (_currentGamestate == GAMESTATE.SETTINGS)//if you are on the board settings menu
        {
            _boardSize = (int)_boardSizeSlider.value; //make board the size that the slider is at.
            _boardSizeText.text = (_boardSizeSlider.value).ToString(); //make board size text on board settings panel update
            _localNameInputAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //tell input animtor to show the correct number of players
        }
        if (_currentGamestate == GAMESTATE.PLAYING || _currentGamestate == GAMESTATE.PLAYINGMULTIPLAYER)//if you are in the game play loop
        {
            _scoreScript.UpdateGameScore(_numberOfPlayers);
            _ingameScoreboardAnimator.SetInteger("PlayerAmount", _numberOfPlayers);
            _turnOrderAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //Make sure that turn animation is on the correct anim size for player amount
            _turnOrderAnimator.SetInteger("PlayerTurn", _turnRotation); //Change turn animation depending on turn rotation
        }
        if (_currentGamestate == GAMESTATE.GAMEOVER) //if you are on the gameover screen
        {
            _scoreScript.DisplayGameoverScore(_numberOfPlayers, _localGame);
            _scoreScript.CalculateWinner(_numberOfPlayers, _myPlayerNumberMP, _playerNames);
        }
        if (_currentGamestate == GAMESTATE.LOBBYMENU)
        {//if you are on the multiplayer lobby menu screen
            _mpBoardSizeText.text = (_mpBoardSizeSlider.value).ToString(); //display slider size
        }
    }
    public void SetLocalGame(bool m_local) //set if you playing a local or multiplayer game.
    {
        _localGame = m_local; //local or multiplayer?
        if (_localGame)//if its a local game
        {
            _currentGamestate = GAMESTATE.SETTINGS; //change gamestate
            _socketManager.SetSOCKETGameState(_currentGamestate.ToString()); //Change gamestate in socketmanager
        }
        else //if its a multiplayer game 
        {
            _currentGamestate = GAMESTATE.LOGINREGISTER;//change gamestate
            _socketManager.SetSOCKETGameState(_currentGamestate.ToString()); //Change gamestate in socketmanager
        }
    }
    public void SetGSGameState(string m_state) //change gamestate if you need to from socket manager
    {
        _currentGamestate = (GAMESTATE)System.Enum.Parse(typeof(GAMESTATE), m_state, true);//change string into a enum. (true means case doesnt matter)
    }
    public void RestartButton(bool m_resetNames) //restart button on game over screen.
    {
        foreach (GameObject _b in GameObject.FindGameObjectsWithTag("button"))//find all objects with the tag button to delete them from the scene
        {
            Destroy(_b);
        }
        foreach (GameObject _box in _gameBoard.boxes)///find all boxes and destroy them
        {
            Destroy(_box);
        }
        _gameBoard = new GameBoard(); //create new gameboard 
        for (int i = 0; i < 4; i++)
        {
            _scoreScript.RestartScore(i);
            _whosTurn[i] = 0; //reset turn order so you can re-randomize them.
            if (!_localGame && m_resetNames)
            {
                _playerNames[i] = "Player " + (i + 1).ToString();
            }
            _playerTurnOrderText[i].text = _playerNames[i]; //reset names of text boxes in the turn order so when it resets its back to players 1-4 in correct order
            _playerTurnOrderText[i].color = _playerColors[i]; //reset color of turn order text boxes ^^

        }

        _turnRotation = 0; // go back to the start of turn order
        while (_turnOrderAnimator.GetInteger("PlayerTurn") != 0) //cycle through turn order until its back to 0, so it shows players 1-4 in order.
        {
            int m_i = _turnOrderAnimator.GetInteger("PlayerTurn");
            if (m_i >= _numberOfPlayers)
            {
                _turnOrderAnimator.SetInteger("PlayerTurn", 0);
            }
            else
            {
                _turnOrderAnimator.SetInteger("PlayerTurn", m_i + 1);
            }

        }
        if (_localGame) //if its a local game
        {
            _currentGamestate = GAMESTATE.SETTINGS; //change gamestate 
            _socketManager.SetSOCKETGameState(_currentGamestate.ToString()); //Change gamestate in socketmanager
            _boardSettingsObj.SetActive(true);  //make board settings visable
        }
        _playerColors[0] = _blue;
        _playerColors[1] = _red;
        _playerColors[2] = _green;
        _playerColors[3] = _yellow;
    }
    public void AddPlayerButton() //Add button on board setting panel
    {
        if (_numberOfPlayers < 4)//add if num is less then max player
        {
            _numberOfPlayers++;
            _numberOfPlayerText.text = _numberOfPlayers.ToString(); //display how many are allowed in local lobby
            _mpLobbySizeText.text = _numberOfPlayers.ToString(); //display how many are allowed in MP lobby
            FindObjectOfType<AudioManager>().Play("addPlayerSound");
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("noPlayerSpaceSound");
        }
        if (_localGame) //if local
        {
            _localNameInputAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //Animate how many player name inputs are visable
            _ingameScoreboardAnimator.SetInteger("PlayerAmount", _numberOfPlayers);
            _turnOrderAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //Make sure that turn animation is on the correct anim size for player amount
        }

    }
    public void MinusPlayerButton()
    {
        if (_numberOfPlayers > 2) //subtract if more then min player
        {
            _numberOfPlayers--;
            _numberOfPlayerText.text = _numberOfPlayers.ToString(); //display how many are allowed in local lobby
            _mpLobbySizeText.text = _numberOfPlayers.ToString(); //display how many are allowed in MP lobby
            FindObjectOfType<AudioManager>().Play("minusPlayerSound");
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("noPlayerSpaceSound");
        }
        if (_localGame)
        {
            _localNameInputAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //Animate how many player name inputs are visable
            _ingameScoreboardAnimator.SetInteger("PlayerAmount", _numberOfPlayers);
            _turnOrderAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //Make sure that turn animation is on the correct anim size for player amount
        }
    }
    public void GameStartButton()//start game button from board setting panel
    {
        _boardSettingsObj.SetActive(false); //hide board setting panel
        _boardSize = (int)_boardSizeSlider.value; //make board the size that the slider is at.
        CreateGame(); //create the game board size
        _currentGamestate = GAMESTATE.PLAYING;//Change gamestate to playing game
        _socketManager.SetSOCKETGameState(_currentGamestate.ToString()); //Change gamestate in socketmanager
        if (_localGame) //if its a local game
        {
            LocalRandomTurn(); //randomize who turn order.
            for (int i = 0; i < _numberOfPlayers; i++)
            {
                //Display local names if they entered them
                if (_localPlayerNameInput[i].text != "")
                {  //check names arent left blank

                    _playerNames[i] = _localPlayerNameInput[i].text; //display name in text box
                    _scoreScript.UpdateUserNames(i, _playerNames[i]);//display names in current score screen.
                    Debug.Log(_playerNames[i] + " < name + " + i + " < player number");
                }
               
            }
            for (int i = 0; i < _numberOfPlayers; i++) //sepereate for loop because before it wouldnt always arrange correctly.
            {
                 ArrangeTurnOrder(i); //Arrange text and color of text boxes in bottom left
            }
        }

    }
    private void ArrangeTurnOrder(int m_turnOrder) //change names and color in turn order
    {
        _playerTurnOrderText[m_turnOrder].text = _playerNames[(_whosTurn[m_turnOrder]) - 1]; //Change text to correct name
        _playerTurnOrderText[m_turnOrder].color = _playerColors[(_whosTurn[m_turnOrder]) - 1]; //Change color of text box to players color
        Debug.Log("random turn oder" + (((_whosTurn[m_turnOrder]) - 1)));
    }
    public void UpdateMPUsernames(string[] m_userList)
    {

        int m_counter = 0;
        foreach (string s in m_userList)
        {
            _playerNames[m_counter] = m_userList[m_counter];
            _scoreScript.UpdateUserNames(m_counter, _playerNames[m_counter]);//display names in current score screen.
            //count after you do what you want 
            m_counter++;
        }
    }
    private void CreateGame() //create game board
    {
        for (int b = 0; b < _boardSize + 1; b++)
        {
            for (int i = 0; i < _boardSize; i++)
            {
                //create dictionary of ROW of buttons and set their placement, name, and other
                GameObject rButtonObj = Instantiate(cornerButtonObj, Vector3.zero, Quaternion.identity) as GameObject; //make new rButton from prefab
                Button rButton = rButtonObj.GetComponentInChildren<Button>();
                var rectTransformRow = rButtonObj.GetComponent<RectTransform>(); //get reference of buttons rectTransformRow
                rectTransformRow.SetParent(_backgroundObj.transform); //make rButton child of background panel obj
                rButton.GetComponent<RectTransform>().sizeDelta = new Vector2(82, 20); //set buttons size
                //set buttons position on background panel obj
                rectTransformRow.position = new Vector2(Screen.width / 2 + (_boardSize / 2 * 80) - i * 80, Screen.height / 2 + (_boardSize / 2 * 80) + 35 - b * 80);
                rButton.name = (b.ToString() + i.ToString() + "r"); //set name of rButton
                _gameBoard.rowButtons.Add(rButton.name, rButton); //add rButton to dictionary with rButton name
                _gameBoard.rowButtons[rButton.name].onClick.AddListener(() => ButtonClicked(rButton, true)); //add listener to rButton so it knows when its clicked and which is clicked. ************ PASSES TRUE SO IT KNOWS ITS A ROW
                _gameBoard.rowClicked.Add(rButton.name, false); //add if rButton is clicked to dictionary with rButton name


                //create dictionary of COLUMNS of buttons and set their placement, name, and other (reversed b & i)**********
                GameObject cButtonObj = Instantiate(cornerButtonObj, Vector3.zero, Quaternion.identity) as GameObject; //make new cButton from prefab
                Button cButton = cButtonObj.GetComponentInChildren<Button>();
                var rectTransformCol = cButtonObj.GetComponent<RectTransform>(); //get reference of buttons rectTransformCol
                rectTransformCol.SetParent(_backgroundObj.transform);//make cButton child of canvas
                cButton.GetComponent<RectTransform>().sizeDelta = new Vector2(82, 20); //set buttons size
                //set buttons position on canvas
                rectTransformCol.position = new Vector2(Screen.width / 2 + (_boardSize / 2 * 80) + 35 - b * 80, Screen.height / 2 + (_boardSize / 2 * 80) - i * 80);
                //set rotation of buttons.
                rectTransformCol.eulerAngles = new Vector3(rectTransformCol.transform.eulerAngles.x, rectTransformCol.transform.eulerAngles.y, 90);
                cButton.name = (i.ToString() + b.ToString() + "c");//set name of cButton
                _gameBoard.colButtons.Add(cButton.name, cButton); //add cButton to dictionary with cButton name
                _gameBoard.colButtons[cButton.name].onClick.AddListener(() => ButtonClicked(cButton, true));//add listener to cButton so it knows when its clicked and which is clicked. ************ PASSES FALSE SO IT KNOWS ITS A COL
                _gameBoard.colClicked.Add(cButton.name, false);//add if cButton is clicked to dictionary with cButton name
            }
        }
        //create list of BOXES and set their placement, name, and other
        for (int b = 0; b < _boardSize; b++)
        {
            for (int i = 0; i < _boardSize; i++)
            {
                if (_gameBoard.colButtons.ContainsKey(b.ToString() + (i + 1).ToString() + "c") && _gameBoard.rowButtons.ContainsKey((b + 1).ToString() + i.ToString() + "r"))
                { //check that the boxes are within the correct buttons. (contains key searches dictionary for if that key exists)
                    GameObject _box = Instantiate(box, Vector3.zero, Quaternion.identity) as GameObject;//make box obj from prefab
                    var _rectTransform = _box.GetComponent<RectTransform>(); //get reference of buttons rectTransform
                    _rectTransform.SetParent(_backgroundObj.transform);//make button child of canvas
                    _rectTransform.SetAsFirstSibling(); //make sure box is behind buttons
                   _box.GetComponent<RectTransform>().sizeDelta = new Vector2(75, 75);//set Boxes size
                    //set box position on canvas
                    _rectTransform.position = new Vector2(Screen.width / 2 + (_boardSize / 2 * 80) - i * 80 - 4, Screen.height / 2 + (_boardSize / 2 * 80) - b * 80 - 4); ;
                    //add buttons that are surrounding boxes to list within the box prefab.
                    _box.GetComponent<BoxScript>().boxLines(_gameBoard.rowButtons[b.ToString() + i.ToString() + "r"]);
                    _box.GetComponent<BoxScript>().boxLines(_gameBoard.rowButtons[(b + 1).ToString() + i.ToString() + "r"]);
                    _box.GetComponent<BoxScript>().boxLines(_gameBoard.colButtons[b.ToString() + i.ToString() + "c"]);
                    _box.GetComponent<BoxScript>().boxLines(_gameBoard.colButtons[b.ToString() + (i + 1).ToString() + "c"]);
                    _box.tag = "unchecked";//tag box that it hasnt been taken for points.
                    _box.name = (b.ToString() + i.ToString() + "b"); //name box given x,y coords on board.
                    _gameBoard.boxes.Add(_box); //add box to boxes list.
                }
            }
        }
    }

    static int GetNextInt32(RNGCryptoServiceProvider rnd) //Function that randomizes turn order for local games.
    {
        byte[] randomInt = new byte[200];
        rnd.GetBytes(randomInt);
        return System.Convert.ToInt32(randomInt[0]);
    }

    private void LocalRandomTurn() //randomize turn order
    {
        //Depending on number of players set array size
        int[] m_RandomTurnOrder = { };
        if (_numberOfPlayers == 4)
        {
            m_RandomTurnOrder = new int[] { 1, 2, 3, 4 };
        }
        else if (_numberOfPlayers == 3)
        {
            m_RandomTurnOrder = new int[] { 1, 2, 3 };
        }
        else if (_numberOfPlayers == 2)
        {
            m_RandomTurnOrder = new int[] { 1, 2 };
        }
        //Randomize the turn order using Fisher Yates
        RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
        m_RandomTurnOrder = m_RandomTurnOrder.OrderBy(x => GetNextInt32(rnd)).ToArray();
        for (int i = 0; i < _numberOfPlayers; i++)
        {
            _whosTurn[i] = m_RandomTurnOrder[i]; //set turn order 
        }
    }


    public void ButtonClicked(Button m_b, bool _youPressed) //function is called when row or column button is pressed, passes button pressed & bool for whether it is a row or col button.
    {
        if (_localGame || (_whosTurn[_turnRotation] == _myPlayerNumberMP && !_localGame) || (!_localGame && !_youPressed)) //only click button if its local, your turn, or you are recieving a button
        {
            bool m_alreadyClicked = false; //temp holds if it has been pressed in the past
            if (m_b.name.Contains('r') && _gameBoard.rowClicked[m_b.name] != true)//check it it is a ROW button && if it HASNT been pressed.
            {
                _gameBoard.rowClicked[m_b.name] = true;
            }
            else if (m_b.name.Contains('c') && _gameBoard.colClicked[m_b.name] != true)//check it it is a COL button && if  it HASNT been pressed.
            {
                _gameBoard.colClicked[m_b.name] = true;
            }
            else //tells script that this button has already been pressed in the past
            {
                m_alreadyClicked = true;
            }
            if (!m_alreadyClicked)//if button hasnt been clicked then change the buttons colour and check if the box needs to be filled in.
            {
                  if(GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>().IsSoundMuted() == false){ //should you play the sound? is the game muted?
            
                 m_b.GetComponentInParent<AudioSource>().Play();
                  }
                m_b.GetComponentInParent<Animator>().SetBool("Pressed", true);
                m_b.gameObject.SetActive(false);

                if (_whosTurn[_turnRotation] == 1)
                {//FIRST PLAYER
                    m_b.GetComponentInParent<Transform>().Find("Image").gameObject.GetComponent<Image>().color = _playerColors[0];
                    m_b.GetComponent<Image>().color = _playerColors[0];
                }
                else if (_whosTurn[_turnRotation] == 2)
                {//SECOND PLAYER
                    m_b.GetComponentInParent<Transform>().Find("Image").gameObject.GetComponent<Image>().color = _playerColors[1];
                    m_b.GetComponent<Image>().color = _playerColors[1];
                }
                else if (_whosTurn[_turnRotation] == 3)
                {//THIRD PLAYER
                    m_b.GetComponentInParent<Transform>().Find("Image").gameObject.GetComponent<Image>().color = _playerColors[2];
                    m_b.GetComponent<Image>().color = _playerColors[2];
                }
                else if (_whosTurn[_turnRotation] == 4)
                {//FOURTH PLAYER
                    m_b.GetComponentInParent<Transform>().Find("Image").gameObject.GetComponent<Image>().color = _playerColors[3];
                    m_b.GetComponent<Image>().color = _playerColors[3];
                }

                CheckBoxes(); //check if box needs to be filled in. (check if it is surrounded by 4 pressed buttons)

                if (!_localGame && _youPressed) //if multi AND you pressed button send to other players
                {
                    //send button to server
                    _socketManager.SendButtonMessage(m_b.name);
                }
                if (!_localGame && !_youPressed) //if multi and you got the button // tell server you got it to remove it from the checklist.
                {
                    _socketManager.RecivedButtonMessage();
                }
            }
        }
    }
    private void CheckBoxes()
    {

        bool m_boxesLeftCheck = false; //holds whether there are any unclaimed boxes left
        bool[] m_clicked = new bool[4]; //temp array to hold bools of whether its been clicked.
        bool m_pointGained = false; //holds whether you scored a point or not, basically to change turns, if point gained dont change turn.

        foreach (GameObject box in _gameBoard.boxes)//go through list of boxes in game
        {
            if (box.tag == "unchecked")//make sure box hasnt been claimed already
            {
                List<Button> temp = box.GetComponent<BoxScript>().buttonList; //temp list of buttons surrounding box
                int counter = 0; //counter to distinguish buttons between row and column
                foreach (Button but in temp)
                {
                    if (counter < 2) //first two buttons stored on box are always ROW buttons
                    {
                        //check and add whether button was m_clicked to temp list
                        if (_gameBoard.rowClicked[but.name])
                        {
                            m_clicked[counter] = true;
                        }
                        else
                        {
                            m_clicked[counter] = false;
                        }
                    }
                    else if (counter < 4)//second two buttons stored on box are always COL buttons
                    {
                        //check and add whether button was m_clicked to temp list
                        if (_gameBoard.colClicked[but.name])
                        {
                            m_clicked[counter] = true;
                        }
                        else
                        {
                            m_clicked[counter] = false;
                        }

                    }
                    else //button shouldnt exist
                    {
                        Debug.Log("button shouldnt exist");
                    }
                    counter++;

                }
                if (m_clicked[0] && m_clicked[1] && m_clicked[2] && m_clicked[3]) // if all buttons in temp m_clicked array were pressed do this.
                {
                    box.GetComponent<BoxScript>().ButtonSurrounded( _playerColors[_whosTurn[_turnRotation] - 1], _blue, _red, _green, _yellow, _purple, _orange, _lightblue); //pass to box script to change its colour and depending on whos turn it is.
                    m_pointGained = true; //point is gained
                    _scoreScript.AddScore(_whosTurn[_turnRotation] - 1);//add points to whoevers turns score.

                }

            }
        }
        //if point isnt gained change turn
        if (!m_pointGained)
        {
            _turnRotation++;
            if (_turnRotation >= _numberOfPlayers)
            {
                _turnRotation = 0;
            }
            Debug.Log(_turnRotation + " turn im on ! -- " + _myPlayerNumberMP + "my player number");
            if (_whosTurn[_turnRotation]  == (_myPlayerNumberMP))
            {
                Debug.Log("turn rotation: " + _turnRotation + " My player num: "+( _myPlayerNumberMP - 1));
                _socketManager.FadeTurn();
            }
            else
            {
                _socketManager.ClearFadeTurn();
            }
        }
        //check if there are any unchecked boxes left (if game is over)
        foreach (GameObject box in _gameBoard.boxes)
        {
            if (box.tag == "unchecked")
            {
                m_boxesLeftCheck = true;
                break;
            }
        }
        //if no more unchecked boxes left end game.
        if (!m_boxesLeftCheck && _currentGamestate != GAMESTATE.GAMEOVER)
        {
            _currentGamestate = GAMESTATE.GAMEOVER;
            _socketManager.SetSOCKETGameState(_currentGamestate.ToString()); //Change gamestate in socketmanager
            _scoreScript.MPGameoverMessage(_socketManager, _numberOfPlayers, _localGame, _myPlayerNumberMP, _playerNames);
        }
        else
        {
            m_boxesLeftCheck = false; //reset for next check
        }
    }


    public int GetEarlyWinner() //get early winner if a player leaves after 50% of a game, called through here to pass variables.
    {
        return _scoreScript.GetEarlyWinner(_boardSize, _numberOfPlayers, _myPlayerNumberMP, _playerNames);
    }
    public int GetBoardSize()//getter for size of board
    {
        return _boardSize;
    }
    public void SetSizeofBoard(int m_sizeofboard)//setter for size of board
    {
        if (_localGame)
        { //local game
            _boardSize = (int)_boardSizeSlider.value;
        }
        if (!_localGame)
        { //multiplayer
            _boardSize = m_sizeofboard; //change if value passed through server
            if (m_sizeofboard == -1) //if hosting game get value from slider.
            {
                _boardSize = (int)_mpBoardSizeSlider.value; //MP slider.
            }
        }

    }
    public Color32[] GetPlayerColours(){
        return _playerColors;
    }
    public int GetMyPlayerNumber() //Getter for player number in multiplayer.
    {
        return _myPlayerNumberMP;
    }
    public void SetMyPlayerNumber(int m_myPlayerNumber) //get my turn number from the server (aka what player you are in the lobby)
    {
        _myPlayerNumberMP = m_myPlayerNumber;
    }

    public int GetLobbySize() //getter lobby size.
    {
        return _numberOfPlayers;
    }
    public void SetLobbySize(int m_lobbySize) //getter lobby size.
    {
         _numberOfPlayers = m_lobbySize;
    }
    public void MPSetLobbySize(int[] m_turnOrder) //setter lobby size 
    {
        _numberOfPlayers = m_turnOrder.Length;

    }
    public void MPSetTurnOrder(int[] m_turnorder) //Setter for turn order in multiplayer
    {

        _numberOfPlayers = m_turnorder.Count();
        for (int i = 0; i < _numberOfPlayers; i++)
        {
            _whosTurn[i] = m_turnorder[i];
            //Set turn order names...
            ArrangeTurnOrder(i);
        }

    }
    
    public void MPSetColour(string[] m_colourlist, string m_mycolour)// Set Player colours so no one has the same colour
    {
        //reset counters
        _redctr = new List<int>();
        _bluectr = new List<int>();
        _yellowctr = new List<int>();
        _greenctr = new List<int>();
        _purplectr = new List<int>();
        _orangectr = new List<int>();
        _lightbluectr = new List<int>();

        int m_playerNum = _myPlayerNumberMP - 1; //my player number - 1 (so it matches 0,1,2,3)
        int m_playerctr = 0; //counter to count what player you are on

        //count & set each players colours to see if there are any duplicates
        foreach (string colour in m_colourlist)
        {
            switch (colour)
            {
                case "red":
                    _redctr.Add(m_playerctr);
                    _playerColors[m_playerctr] = _red;
                    break;
                case "blue":
                    _bluectr.Add(m_playerctr);
                    _playerColors[m_playerctr] = _blue;
                    break;
                case "yellow":
                    _yellowctr.Add(m_playerctr);
                    _playerColors[m_playerctr] = _yellow;
                    break;
                case "green":
                    _playerColors[m_playerctr] = _green;
                    _greenctr.Add(m_playerctr);
                    break;
                case "purple":
                    _playerColors[m_playerctr] = _purple;
                    _purplectr.Add(m_playerctr);
                    break;
                case "orange":
                    _playerColors[m_playerctr] = _orange;
                    _orangectr.Add(m_playerctr);
                    break;
                case "lightblue":
                    _playerColors[m_playerctr] = _lightblue;
                    _lightbluectr.Add(m_playerctr);
                    break;
            }
            m_playerctr++;
        }


        int m_colourCtr = 0; //holds count of how many of one colour

        if (_redctr.Count() >= 2) //if there is a duplicate colour
        {
            m_colourCtr = _redctr.Count();
            foreach (int x in _redctr)
            {
                if (m_colourCtr > 1 && x != m_playerNum) //change colour to different colour if its not my colour (dont want to switch my own colour to something i dont want)
                {
                    SwitchColour(x);
                    m_colourCtr--;

                }
            }
        }
        if (_bluectr.Count() >= 2)
        {
            m_colourCtr = _bluectr.Count();
            foreach (int x in _bluectr)
            {
                if (m_colourCtr > 1 && x != m_playerNum)
                { //if its not my number and its in the red ctr still.
                    SwitchColour(x);
                    m_colourCtr--;
                }
            }

        }
        if (_yellowctr.Count() >= 2)
        {
            m_colourCtr = _yellowctr.Count();
            foreach (int x in _yellowctr)
            {
                if (m_colourCtr > 1 && x != m_playerNum)//if its not my number and greater then 1 change to different colour 
                {
                    SwitchColour(x);
                    m_colourCtr--;

                }
            }
        }
        if (_greenctr.Count() >= 2)
        {
            m_colourCtr = _greenctr.Count();
            foreach (int x in _greenctr)
            {
                if (m_colourCtr > 1 && x != m_playerNum)
                { //if its not my number and its in the red ctr still.
                    SwitchColour(x);
                    m_colourCtr--;
                }
            }
        }
        if (_purplectr.Count() >= 2)
        {
            m_colourCtr = _purplectr.Count();
            foreach (int x in _purplectr)
            {
                if (m_colourCtr > 1 && x != m_playerNum)
                { //if its not my number and its in the red ctr still.
                    SwitchColour(x);
                    m_colourCtr--;
                }
            }
        }
        if (_orangectr.Count() >= 2)
        {
            m_colourCtr = _orangectr.Count();
            foreach (int x in _orangectr)
            {
                if (m_colourCtr > 1 && x != m_playerNum)
                { //if its not my number and its in the red ctr still.
                    SwitchColour(x);
                    m_colourCtr--;
                }
            }
        }
        if (_lightbluectr.Count() >= 2)
        {
            m_colourCtr = _lightbluectr.Count();
            foreach (int x in _lightbluectr)
            {
                if (m_colourCtr > 1 && x != m_playerNum)
                { //if its not my number and its in the red ctr still.
                    SwitchColour(x);
                    m_colourCtr--;
                }
            }
        }


    }
    public void UsernameTextColour(TMP_Text m_usernameText, string m_mycolour) //update username text colour 
    {
        switch (m_mycolour)
        {
            case "blue":
                m_usernameText.color = _blue;
                break;
            case "red":
                m_usernameText.color = _red;
                break;
            case "green":
                m_usernameText.color = _green;
                break;
            case "yellow":
                m_usernameText.color = _yellow;
                break;
            case "purple":
                m_usernameText.color = _purple;
                break;
            case "orange":
                m_usernameText.color = _orange;
                break;
            case "lightblue":
                m_usernameText.color = _lightblue;
                break;
        }
        ButtonNeonColour(m_mycolour);
    }
     public void ButtonNeonColour(string m_mycolour) //update username text colour 
    {
        switch (m_mycolour)
        {
            case "blue":
                foreach(GameObject neonButtonorText in GameObject.FindGameObjectsWithTag("Neon")){
                    neonButtonorText.GetComponent<NeonButtonScript>().ChangeColour(_blue);
                }
                break;
            case "red":
                foreach(GameObject neonButtonorText in GameObject.FindGameObjectsWithTag("Neon")){
                    neonButtonorText.GetComponent<NeonButtonScript>().ChangeColour(_red);
                }
                break;
            case "green":
                foreach(GameObject neonButtonorText in GameObject.FindGameObjectsWithTag("Neon")){
                    neonButtonorText.GetComponent<NeonButtonScript>().ChangeColour(_green);
                }
                break;
            case "yellow":
                foreach(GameObject neonButtonorText in GameObject.FindGameObjectsWithTag("Neon")){
                    neonButtonorText.GetComponent<NeonButtonScript>().ChangeColour(_yellow);
                }
                break;
            case "purple":
                foreach(GameObject neonButtonorText in GameObject.FindGameObjectsWithTag("Neon")){
                    neonButtonorText.GetComponent<NeonButtonScript>().ChangeColour(_purple);
                }
                break;
            case "orange":
                foreach(GameObject neonButtonorText in GameObject.FindGameObjectsWithTag("Neon")){
                    neonButtonorText.GetComponent<NeonButtonScript>().ChangeColour(_orange);
                }
                break;
            case "lightblue":
                foreach(GameObject neonButtonorText in GameObject.FindGameObjectsWithTag("Neon")){
                    neonButtonorText.GetComponent<NeonButtonScript>().ChangeColour(_lightblue);
                }
                break;
        }
    }
    public void SwitchColour(int m_playerToSwitch) //switch opponents colours if they are the same.
    {
        //Could randomize it ??

        if (_redctr.Count() == 0)
        { //if there are no red players, make it red
            _playerColors[m_playerToSwitch] = _red;
            _redctr.Add(m_playerToSwitch);

        }
        else if (_bluectr.Count() == 0)
        {//if there are no blue players, make it blue
            _playerColors[m_playerToSwitch] = _blue;
            _bluectr.Add(m_playerToSwitch);

        }
        else if (_greenctr.Count() == 0)
        {//if there are no green players, make it green
            _playerColors[m_playerToSwitch] = _green;
            _greenctr.Add(m_playerToSwitch);

        }
        else if (_yellowctr.Count() == 0)
        { //if there are no yellow players, make it red
            _playerColors[m_playerToSwitch] = _yellow;
            _yellowctr.Add(m_playerToSwitch);

        }
        else if (_purplectr.Count() == 0)
        { //if there are no yellow players, make it red
            _playerColors[m_playerToSwitch] = _purple;
            _purplectr.Add(m_playerToSwitch);

        }
        else if (_orangectr.Count() == 0)
        { //if there are no yellow players, make it red
            _playerColors[m_playerToSwitch] = _orange;
            _orangectr.Add(m_playerToSwitch);

        }
        else if (_lightbluectr.Count() == 0)
        { //if there are no yellow players, make it red
            _playerColors[m_playerToSwitch] = _lightblue;
            _lightbluectr.Add(m_playerToSwitch);

        }
    }
    public void StartMultiplayerGameBoard()//everything needed to start a multiplayer board
    {

        _boardSettingsObj.SetActive(false); //hide board settings to see board
        CreateGame(); //create board based off board size

    }
    public void StopMultiplayerGameBoard()//everything needed to start a multiplayer board
    {

        _boardSettingsObj.SetActive(true); //show board settings to see board
        //Destroy Game board.
        RestartButton(true);

    }
    public void MPButtonClicked(string m_bName) //What to do when you get a button click from another player
    {
        ButtonClicked(GameObject.Find(m_bName).GetComponent<Button>(), false); //false to say you didnt press it you got it from the server. 
    }
    


    // -------------------------CLASSES ---------------------------------------
    [System.Serializable]
    class GameBoard
    {
        public Dictionary<string, Button> colButtons = new Dictionary<string, Button>(); //dictionary of buttons in column named by their position on a graph, ie 00, 01, 10 (first number is x value, 2nd num is y value ie xy or (0,0) would be 00)
        public Dictionary<string, Button> rowButtons = new Dictionary<string, Button>();//dictionary of buttons in row named by their position on a graph like above
        public Dictionary<string, bool> rowClicked = new Dictionary<string, bool>();//dictionary of bools in row named by their position on a graph, tells whether buttons been pressed or not
        public Dictionary<string, bool> colClicked = new Dictionary<string, bool>();//dictionary of bools in column named by their position on a graph, tells whether buttons been pressed or not
        public List<GameObject> boxes = new List<GameObject>(); //list of boxed (the center of whats between 4 buttons and what changes colour)
    }


}

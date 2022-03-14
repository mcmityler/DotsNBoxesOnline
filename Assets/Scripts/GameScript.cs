using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Linq;

public class GameScript : MonoBehaviour
{

    //----------------------------VARIABLES-------------------------------------
    private SocketManager _socketManager; //ref to socket manager script 
    [Range(2.0F, 8.0F)][SerializeField] private int _boardSize = 4; //Board size in game, (ie 4x4, max it can go is 8x8 before it goes off screen) (range is what shows in the slider 2-8)
    public Button cornerButton; //Prefab for buttons that surround the box
    public GameObject box; //Prefab of center of buttons aka box. Also hold what buttons surround it and need to be pressed to change its color.
    private GameObject _canvas; //Canvas obj (where all the UI is)
    [SerializeField] private GameObject _backgroundObj; //Background panel of the entire canvas (also where the gameboard is drawn)
    [SerializeField] private GameObject _boardSettingsObj, _localPlayerNamesObj; //Settings panel for picking board size and player amount //local player name inputs obj (toggles visibility)
    [SerializeField] private Slider _boardSizeSlider, _mpBoardSizeSlider; //slider on board setting panel to choose size of board // slider on multiplayer menu
    [SerializeField] private Text _boardSizeText, _mpBoardSizeText;//Text that displays slider count// text for slider on multiplayer menu
    [SerializeField] private Text _numberOfPlayerText, _mpLobbySizeText; //text that displays how many people are playing on settings screen // multiplayer lobby menu

    // -------------------Gameover screen variables--------------------------
    [SerializeField] private GameObject _gameoverObj; //gameover obj displays score screen and restart button
    [SerializeField] private Text _winnerText; //text that displays who won the game or if it was a tie
    [SerializeField] private GameObject _LCRestart, _MPRestart; //gameobjects that hold restart buttons for multiplayer or localplay
    [SerializeField] private Text[] _playerScoreTextboxes; //player 1 - 4 score text boxes
    [SerializeField] private Text[] _endgameScoreTextboxes; //endgame 1 - 4 place score text boxes
    [SerializeField] private Animator _endgameScoreAnimator; //animator on engame score screen (changes size of scorebox depending on how many players)
    [SerializeField] private Text[] _playerNameTextboxes; //player 1-4 name text boxes in score (during game score screen.)
    [SerializeField] private InputField[] _localPlayerNameInput; //player 1-4 name input for local games
    private string[] _localPlayerNames = new string[] { "Player 1", "Player 2", "Player 3", "Player 4" }; //player 1-4 local names.
    [SerializeField] private Animator _localNameInputAnimator; //local name input animator (changes how many names on local input screen)
    private int _numberOfPlayers = 2; //how many players are allowed in your lobby
    private bool _localGame = true; //are you playing a local game 
    private int[] _playerScores = new int[] { 0, 0, 0, 0 }; //player 1-4 scores (how many boxes have they collected)
    private Color32[] _localPlayerColors = new Color32[] { new Color32(0, 90, 188, 255), new Color32(137, 0, 0, 255), new Color32(1, 123, 0, 255), new Color32(209, 197, 0, 255) }; //Local players colors on the board.
    [SerializeField] private Animator _turnOrderAnimator; //turn order animator
    private int _myPlayerNumberMP = 0; //turn number / what player you are in the lobby
    private int[] _whosTurn = new int[] { 0, 0, 0, 0 }; //holds randomized turn order
    private int _turnRotation = 0; //placeholder for what turn you are currently on
    [SerializeField] private Text[] _playerTurnOrderText; //text boxes that display whos turn it is in the turn order 
    private GameBoard _gameBoard = new GameBoard(); //gameboard is every button, box and if the buttons have been clicked yet.
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
        PLAYINGMULTIPLAYER
    };

    private GAMESTATE _currentGamestate = GAMESTATE.STARTMENU; //current gamestate of game

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
        }
        if (_currentGamestate == GAMESTATE.PLAYING || _currentGamestate == GAMESTATE.PLAYINGMULTIPLAYER)//if you are in the game play loop
        {
            for (int i = 0; i < _numberOfPlayers; i++)
            {
                _playerScoreTextboxes[i].text = _playerScores[i].ToString(); //display score while game is running
            }
            _turnOrderAnimator.SetInteger("PlayerTurn", _turnRotation); //Change turn animation depending on turn rotation
        }
        if (_currentGamestate == GAMESTATE.GAMEOVER) //if you are on the gameover screen
        {
            DisplayWinner(); //Display the winner 
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
    public void RestartButton() //restart button on game over screen.
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
            _whosTurn[i] = 0; //reset turn order so you can re-randomize them.
            _playerScores[i] = 0; //set scores to 0
            _playerScoreTextboxes[i].text = _playerScores[i].ToString(); //display scores in text box
            if (_localGame) // if local game.
            {
                _playerTurnOrderText[i].text = _localPlayerNames[i]; //reset names of text boxes in the turn order so when it resets its back to players 1-4 in correct order
                _playerTurnOrderText[i].color = _localPlayerColors[i]; //reset color of turn order text boxes ^^
            }
        }
        _gameoverObj.SetActive(false); //make gameover panel invisible.
        _turnRotation = 0; // go back to the start of turn order
        _localNameInputAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //tell input animtor to show the correct number of players
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
    }
    public void AddPlayerButton() //Add button on board setting panel
    {
        if (_numberOfPlayers < 4)//add if num is less then max player
        {
            _numberOfPlayers++;
            _numberOfPlayerText.text = _numberOfPlayers.ToString(); //display how many are allowed in local lobby
            _mpLobbySizeText.text = _numberOfPlayers.ToString(); //display how many are allowed in MP lobby
        }
        if (_localGame) //if local
        {
            _localNameInputAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //Animate how many player name inputs are visable
            _turnOrderAnimator.SetInteger("PlayerAmount", _numberOfPlayers);//Animate how many players are shown in turn order
        }
        if(!_localGame) //if multiplayer
        {
            _turnOrderAnimator.SetInteger("PlayerAmount", _numberOfPlayers);//Animate how many players are shown in turn order

        }
    }
    public void MinusPlayerButton()
    {
        if (_numberOfPlayers > 2) //subtract if more then min player
        {
            _numberOfPlayers--;
            _numberOfPlayerText.text = _numberOfPlayers.ToString(); //display how many are allowed in local lobby
            _mpLobbySizeText.text = _numberOfPlayers.ToString(); //display how many are allowed in MP lobby
        }
        if (_localGame)
        {
            _localNameInputAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //Animate how many player name inputs are visable
            _turnOrderAnimator.SetInteger("PlayerAmount", _numberOfPlayers);//Animate how many players are shown in turn order
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

                    _localPlayerNames[i] = _localPlayerNameInput[i].text; //display name in text box
                    _playerNameTextboxes[i].text = _localPlayerNames[i] + ": "; //display names in current score screen.
                }
                ArrangeTurnOrder(i); //Arrange text and color of text boxes in bottom left
            }
        }

    }
    private void ArrangeTurnOrder(int m_turnOrder) //change names and color in turn order
    {
        _playerTurnOrderText[m_turnOrder].text = _localPlayerNames[(_whosTurn[m_turnOrder]) - 1]; //Change text to correct name
        _playerTurnOrderText[m_turnOrder].color = _localPlayerColors[(_whosTurn[m_turnOrder]) - 1]; //Change color of text box to players color
    }
    private void CreateGame() //create game board
    {
        for (int b = 0; b < _boardSize + 1; b++)
        {
            for (int i = 0; i < _boardSize; i++)
            {
                //create dictionary of ROW of buttons and set their placement, name, and other
                Button rButton = Instantiate(cornerButton, Vector3.zero, Quaternion.identity) as Button; //make new rButton from prefab
                var rectTransformRow = rButton.GetComponent<RectTransform>(); //get reference of buttons rectTransformRow
                rectTransformRow.SetParent(_backgroundObj.transform); //make rButton child of background panel obj
                rectTransformRow.sizeDelta = new Vector2(82, 20); //set buttons size
                //set buttons position on background panel obj
                rectTransformRow.position = new Vector2(_backgroundObj.GetComponent<RectTransform>().rect.width / 2 + (_boardSize / 2 * 80) - i * 80, _backgroundObj.GetComponent<RectTransform>().rect.height / 2 + (_boardSize / 2 * 80) + 35 - b * 80);
                rButton.name = (b.ToString() + i.ToString() + "r"); //set name of rButton
                _gameBoard.rowButtons.Add(rButton.name, rButton); //add rButton to dictionary with rButton name
                _gameBoard.rowButtons[rButton.name].onClick.AddListener(() => ButtonClicked(rButton, true, true)); //add listener to rButton so it knows when its clicked and which is clicked. ************ PASSES TRUE SO IT KNOWS ITS A ROW
                _gameBoard.rowClicked.Add(rButton.name, false); //add if rButton is clicked to dictionary with rButton name


                //create dictionary of COLUMNS of buttons and set their placement, name, and other (reversed b & i)**********
                Button cButton = Instantiate(cornerButton, Vector3.zero, Quaternion.identity) as Button; //make new cButton from prefab
                var rectTransformCol = cButton.GetComponent<RectTransform>(); //get reference of buttons rectTransformCol
                rectTransformCol.SetParent(_backgroundObj.transform);//make cButton child of canvas
                rectTransformCol.sizeDelta = new Vector2(82, 20); //set buttons size
                //set buttons position on canvas
                rectTransformCol.position = new Vector2(_backgroundObj.GetComponent<RectTransform>().rect.width / 2 + (_boardSize / 2 * 80) + 35 - b * 80, _backgroundObj.GetComponent<RectTransform>().rect.height / 2 + (_boardSize / 2 * 80) - i * 80);
                //set rotation of buttons.
                rectTransformCol.eulerAngles = new Vector3(rectTransformCol.transform.eulerAngles.x, rectTransformCol.transform.eulerAngles.y, 90);
                cButton.name = (i.ToString() + b.ToString() + "c");//set name of cButton
                _gameBoard.colButtons.Add(cButton.name, cButton); //add cButton to dictionary with cButton name
                _gameBoard.colButtons[cButton.name].onClick.AddListener(() => ButtonClicked(cButton, false, true));//add listener to cButton so it knows when its clicked and which is clicked. ************ PASSES FALSE SO IT KNOWS ITS A COL
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
                    _rectTransform.sizeDelta = new Vector2(75, 75);//set Boxes size
                    //set buttons position on canvas
                    _rectTransform.position = new Vector2(_backgroundObj.GetComponent<RectTransform>().rect.width / 2 + (_boardSize / 2 * 80) - i * 80, _backgroundObj.GetComponent<RectTransform>().rect.height / 2 + (_boardSize / 2 * 80) - b * 80); ;
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

    static int GetNextInt32(RNGCryptoServiceProvider rnd)
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


    public void ButtonClicked(Button m_b, bool m_row, bool _youPressed) //function is called when row or column button is pressed, passes button pressed & bool for whether it is a row or col button.
    {
        if (_localGame || (_whosTurn[_turnRotation] == _myPlayerNumberMP && !_localGame) || (!_localGame && !_youPressed)) //only click button if its local, your turn, or you are recieving a button
        {
            bool m_alreadyClicked = false; //temp holds if it has been pressed in the past
            if (m_row && _gameBoard.rowClicked[m_b.name] != true)//check it it is a ROW button && if it HASNT been pressed.
            {
                _gameBoard.rowClicked[m_b.name] = true;
            }
            else if (!m_row && _gameBoard.colClicked[m_b.name] != true)//check it it is a COL button && if  it HASNT been pressed.
            {
                _gameBoard.colClicked[m_b.name] = true;
            }
            else //tells script that this button has already been pressed in the past
            {
                m_alreadyClicked = true;
            }
            if (!m_alreadyClicked)//if button hasnt been clicked then change the buttons colour and check if the box needs to be filled in.
            {

                if (_whosTurn[_turnRotation] == 1)
                {//FIRST PLAYER
                    m_b.GetComponent<Image>().color = Color.blue;
                }
                else if (_whosTurn[_turnRotation] == 2)
                {//SECOND PLAYER
                    m_b.GetComponent<Image>().color = Color.red;
                }
                else if (_whosTurn[_turnRotation] == 3)
                {//THIRD PLAYER
                    m_b.GetComponent<Image>().color = Color.green;
                }
                else if (_whosTurn[_turnRotation] == 4)
                {//FOURTH PLAYER
                    m_b.GetComponent<Image>().color = Color.yellow;
                }
                // m_b.GetComponent<Image>().color = _localPlayerColors[_whosTurn[_turnRotation]-1];
                ColorBlock m_tempColor = m_b.GetComponent<Button>().colors; //make temporary color block to change alpha of button to 100%
                m_tempColor.normalColor = m_b.GetComponent<Image>().color; //set normal color to what color button should be.
                m_b.GetComponent<Button>().colors = m_tempColor; //set buttons color block to temp color block

                CheckBoxes(); //check if box needs to be filled in. (check if it is surrounded by 4 pressed buttons)

                if (!_localGame && _youPressed) //if multi AND you pressed button send to other players
                {
                    //Convert bool to int to send button to server
                    int m_tempint = 0;
                    if (m_row)
                    {
                        m_tempint = 1;
                    }
                    if (!m_row)
                    {
                        m_tempint = 0;
                    }
                    //send button to server
                    _socketManager.SendButtonMessage(m_b.name, m_tempint);
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
                    box.GetComponent<BoxScript>().ButtonSurrounded(_whosTurn[_turnRotation], _localPlayerColors[_whosTurn[_turnRotation] - 1]); //pass to box script to change its colour and depending on whos turn it is.
                    m_pointGained = true; //point is gained
                    _playerScores[_whosTurn[_turnRotation] - 1]++;//add points to whoevers turns score.

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
            for (int i = 0; i < _numberOfPlayers; i++)//Update score one last time!!
            {
                _playerScoreTextboxes[i].text = _playerScores[i].ToString();
            }
        }
        else
        {
            m_boxesLeftCheck = false; //reset for next check
        }
    }
    private void DisplayWinner() //Display Winner / Score screen scores.
    {
        _LCRestart.SetActive(false);
        _MPRestart.SetActive(false);
        if(_localGame){
            _LCRestart.SetActive(true);
        }else if(!_localGame){
            _MPRestart.SetActive(true);
        }
        _endgameScoreAnimator.SetInteger("NumberOfPlayers", _numberOfPlayers); //change size of score screen depending on player number
        _gameoverObj.SetActive(true); //show the endgame score screen and the restart btn
        int[] m_playerPlace = new int[] { 0, 0, 0, 0 }; //check what place each player finished in.
        for (int i = 0; i < _numberOfPlayers; i++)
        {
            for (int x = 0; x < _numberOfPlayers; x++)
            {
                if (i != x)
                {
                    if (_playerScores[i] > _playerScores[x]) //if a player has a greater score than another, add points to their score.
                    {
                        m_playerPlace[i]++;
                    }
                }
            }
        }
        int m_firstPlace = -1; //whos in first place, if its -1 no ones in first.
        List<int> m_secondPlace = new List<int>();
        List<int> m_thirdPlace = new List<int>();
        List<int> m_fourthPlace = new List<int>();
        for (int i = 0; i < _numberOfPlayers; i++)
        {
            //set players places
            if (m_playerPlace[i] == 3)
            {
                m_firstPlace = i;
            }
            if (m_playerPlace[i] == 2)
            {
                m_secondPlace.Add(i);
            }
            if (m_playerPlace[i] == 1)
            {
                m_thirdPlace.Add(i);
            }
            if (m_playerPlace[i] == 0)
            {
                m_fourthPlace.Add(i);
            }

        }
        if (m_firstPlace == -1) //if its a 4player game and -1 its a tie.
        {
            _winnerText.text = "Tie Game!";
        }

        int m_ctr = 0;
        //check that you have a player in first (if 4 player)
        if (m_firstPlace != -1)
        {
            _endgameScoreTextboxes[m_ctr].text = _localPlayerNames[m_firstPlace] + "      Boxes: " + _playerScores[m_firstPlace];
            _winnerText.text = _localPlayerNames[m_firstPlace] + " is the Winner!";
            m_ctr++;
        }
        foreach (int second in m_secondPlace) //cycle through anyplayers that tied for second
        {
            _endgameScoreTextboxes[m_ctr].text = _localPlayerNames[second] + "      Boxes: " + _playerScores[second];
            if (m_firstPlace == -1 && m_secondPlace.Count == 1)
            {
                _winnerText.text = _localPlayerNames[second] + " is the Winner!";
            }
            m_ctr++;
        }
        foreach (int third in m_thirdPlace)//cycle through anyplayers that tied for third
        {
            _endgameScoreTextboxes[m_ctr].text = _localPlayerNames[third] + "      Boxes: " + _playerScores[third];
            if (m_secondPlace.Count == 0 && m_thirdPlace.Count == 1)
            {
                _winnerText.text = _localPlayerNames[third] + " is the Winner!";
            }
            m_ctr++;
        }
        foreach (int fourth in m_fourthPlace)//cycle through anyplayers that tied for fourth
        {
            _endgameScoreTextboxes[m_ctr].text = _localPlayerNames[fourth] + "      Boxes: " + _playerScores[fourth];
            m_ctr++;
        }

    }


    public int GetPlayerSize()
    { //get player size froms script while its private
        return _numberOfPlayers;
    }
    public int GetBoardSize()
    { //get size of board from script while its private
        return _boardSize;
    }
    public void SetSizeofBoard(int m_sizeofboard)
    { //set size of board from multiplayer
        _boardSize = m_sizeofboard;
        if (m_sizeofboard == -1)
        {
            _boardSize = (int)_mpBoardSizeSlider.value; //make board the size that the slider is at.
        }
    }
    public void SetMyPlayerNumber(int m_myPlayerNumber) //get my turn number from the server (aka what player you are in the lobby)
    {
        _myPlayerNumberMP = m_myPlayerNumber;
    }
    public void MPSetTurnOrder(int[] m_turnorder) //Get the turn order from the server and set it
    {
       for (int i = 0; i < _numberOfPlayers; i++)
       {
           _whosTurn[i] = m_turnorder[i];
       }
    }
    public void StartMultiplayerGameBoard()//everything needed to start a multiplayer board
    {  

        _boardSettingsObj.SetActive(false); //hide board settings to see board
        CreateGame(); //create board based off board size

    }
    public void MPButtonClicked(string m_bName, int m_row) //What to do when you get a button click from another player
    {
        //convert int from server to bool for client
        bool m_tempBool = false;
        if (m_row == 0)
        {
            m_tempBool = false;
        }
        if (m_row == 1)
        {
            m_tempBool = true;
        }
        ButtonClicked(GameObject.Find(m_bName).GetComponent<Button>(), m_tempBool, false); //false to say you didnt press it you got it from the server. 
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

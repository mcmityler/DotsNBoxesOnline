using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{

    //----------------------------VARIABLES-------------------------------------
    [Range(2.0F, 8.0F)] //slider in inspector, goes from 2 - 8
    [SerializeField] private int _boardSize = 4; //Board size in game, (ie 4x4, max it can go is 8x8 before it goes off screen)
    public Button cornerButton; //reference to the prefab for each button.
    public GameObject box; //reference to prefab for the middle of the button (what changes colour to show who scored the point). also holds what buttons center takes to change colour.
    private GameObject _canvas; //reference to canvas obj
    [SerializeField] private GameObject _backgroundObj; //reference to background panel obj

    [SerializeField] private GameObject _boardSettingsObj, _localPlayerNamesObj; //reference to panel where you select board size. and the obj where the player enters their names if its a local game
    [SerializeField] private GameObject _winnerObj; //reference to obj that displays the games winner and the restart button
    [SerializeField] private Text _winnterText; //refference to gameobject that displays who won the game.
    [SerializeField] private Slider _boardSizeSlider; //Reference to slider on board size panel.
    [SerializeField] private Text _boardSizeText;//Reference to size text on board size panel.
    private int[] _whosTurn = new int[] {0,0,0,0}; //Whos turn is it?
    [SerializeField] private Text[] _playerScoreTextboxes; //reference to  player 1 - 4 score text boxes
    [SerializeField] private Text[] _playerNameTextboxes; //reference to  player 1 & 2 score text boxes and Player 1 and 2 Name textboxes
    [SerializeField] private InputField[] _localPlayerNameInput; //reference to player 1, 2, 3, and 4 input boxes
    private string[] _localPlayerNames= new string[] {"Player 1","Player 2","Player 3","Player 4"}; //what are the local entered names saved
    [SerializeField] private int _maxNameLength = 7; //how long can the players name be in the input
    private int _numberOfPlayers = 2; //how many players are playing
    [SerializeField] private Text _numberOfPlayerText; //reference to display how many players you want playing
    private int _turnRotation = 0; //what turn are you on.
    private bool _localGame = true; //are you playing a local game 
    private int[] _playerScores = new int[] {0,0,0,0}; //player 1-4 scores 

    private Color32[] _localPlayerColors = new Color32[] {new Color32(0,90,188,255),new Color32(137,0,0,255), new Color32(1,123,0,255), new Color32(209,197,0,255)}; //Local players colors on the board.

    [SerializeField] private Text[] _playerTurnOrderText; //reference to the textboxes for players turn order.
    [SerializeField] private GameObject[] _localPlayerInputObjs; //Reference to the local player name input text boxes to change position and visibility 
    private GameBoard _gameBoard = new GameBoard(); // gameboard is every button, box and if the buttons have been clicked yet.
    private enum GAMESTATE { //Enum for game state / what point the game is currently at.
        SETTINGS, 
        PLAYING, 
        GAMEOVER,
        RESTART
    };

    private GAMESTATE _currentGamestate = GAMESTATE.SETTINGS; //Actual reference to current game state


    [SerializeField] private Animator _turnOrderAnimator; //reference to what animates the turn order
    [SerializeField] private Animator _localNameInputAnimator; //reference to what animates the local name input when players are added/subtracted

    //----------------------------FUNCTIONS-------------------------------------
    void Awake(){ //when this gameobject is awoken do ...
         _canvas = GameObject.FindGameObjectWithTag("Canvas"); //give canvas reference to the canvas obj
    }
    void Update(){
        if(_currentGamestate == GAMESTATE.SETTINGS){ //if you are on the board settings menu
            _boardSize = (int) _boardSizeSlider.value; //make board the size that the slider is at.
            _boardSizeText.text = (_boardSizeSlider.value).ToString(); //make board size text on board settings panel update
            if(_localGame){ //if you are playing a local game
                for(int i = 0; i < _numberOfPlayers; i++){ //Cycle how many players are in the game
                    if(_localPlayerNameInput[i].text.Length > _maxNameLength){ //Check to see if text box is less then desired length
                        _localPlayerNameInput[i].text = _localPlayerNameInput[i].text.Substring(0, _localPlayerNameInput[i].text.Length - 1); //remove last character entered if it is longer then desired length
                    }
                }
               
            }
           
        }
        if(_currentGamestate == GAMESTATE.PLAYING){ //if you are in the game play loop
            for(int i = 0; i < _numberOfPlayers; i++){
                _playerScoreTextboxes[i].text = _playerScores[i].ToString();
            }
            _turnOrderAnimator.SetInteger("PlayerTurn", _turnRotation); //Change turn animation depending on turn rotation
        }
        if (_currentGamestate == GAMESTATE.GAMEOVER){
            DisplayWinner();
            
        }
    }
    public void RestartButton(){
        
         foreach(GameObject _b in GameObject.FindGameObjectsWithTag("button")){ //find all objects with the tag button to delete them from the scene
            Destroy(_b);
        }
        foreach(GameObject _box in _gameBoard.boxes){ ///find all boxes and destroy them
            Destroy(_box);
        }
        _gameBoard = new GameBoard();
        for (int i = 0; i < 4; i++) 
        {
            _whosTurn[i] = 0; //reset turns so you can rerandomize them.
            _playerScores[i] = 0; //set scores to 0
            _playerScoreTextboxes[i].text = _playerScores[i].ToString(); //display scores in text box
            if(_localGame){
                _playerTurnOrderText[i].text = _localPlayerNames[i];
                _playerTurnOrderText[i].color = _localPlayerColors[i];
            }
        }
        _winnerObj.SetActive(false); //make gg text disappear.
        if(_localGame){
            _currentGamestate = GAMESTATE.SETTINGS; //change gamestate 
            _boardSettingsObj.SetActive(true);  //make board setting visable
            //_localPlayerNamesObj.SetActive(false); //Make name input invisible.
        }
        _turnRotation = 0; // go back to the start of turn order
        _localNameInputAnimator.SetInteger("PlayerAmount", _numberOfPlayers);
        while( _turnOrderAnimator.GetInteger("PlayerTurn") != 0){
            Debug.Log("hey");
                    var m_i = _turnOrderAnimator.GetInteger("PlayerTurn");
                    if(m_i > _numberOfPlayers){
                        _turnOrderAnimator.SetInteger("PlayerTurn", 0);
                    }
                    else{
                        _turnOrderAnimator.SetInteger("PlayerTurn", m_i+1);
                    }
                    
        }
        
    }
    public void AddPlayerButton(){
        if(_numberOfPlayers < 4){
            _numberOfPlayers ++;
            _numberOfPlayerText.text = _numberOfPlayers.ToString();
        }
        if(_localGame){
            _localNameInputAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //Animate how many player name inputs are visable
            _turnOrderAnimator.SetInteger("PlayerAmount", _numberOfPlayers);//Animate how many players are shown in turn order
        }
    }public void MinusPlayerButton(){
        if(_numberOfPlayers > 2){
            _numberOfPlayers --;
            _numberOfPlayerText.text = _numberOfPlayers.ToString();
        }
        if(_localGame){
            _localNameInputAnimator.SetInteger("PlayerAmount", _numberOfPlayers); //Animate how many player name inputs are visable
            _turnOrderAnimator.SetInteger("PlayerAmount", _numberOfPlayers);//Animate how many players are shown in turn order
        }
    }

    public void GameStartButton(){ //when you click the start button on board setting screen
        _boardSettingsObj.SetActive(false); //make panel disappear.
        _boardSize = (int) _boardSizeSlider.value; //make board the size that the slider is at.
        CreateGame(); //create the game board with the size in slider
        _currentGamestate = GAMESTATE.PLAYING;//Change gamestate to playing game
        RandomizeTurns();
        if(_localGame){//if you entered a name and its a local game make player name appear in text boxes
            
            for (int i = 0; i < _numberOfPlayers; i++) //cycle all players in game
            {
                
                 if(_localPlayerNameInput[i].text != ""){  //check names arent left blank
                 
                    _localPlayerNames[i] = _localPlayerNameInput[i].text; //set local player names to what was inputed if anything.
                    Debug.Log(_localPlayerNames[i]);
                    _playerNameTextboxes[i].text = _localPlayerNames[i] + ": "; //set the text boxes at the score to the names.
                    
                   // _playerTurnOrderText[i].text = _localPlayerNames[i]; // Set the text boxes in the turn order to the names.
                
                }
                ArrangeTurnOrder(i); //Arrange text and color of text boxes in bottom left
            }
        }
        
    }
    private void ArrangeTurnOrder(int m_turnOrder){
        Debug.Log(_whosTurn[m_turnOrder]);
        _playerTurnOrderText[m_turnOrder].text = _localPlayerNames[(_whosTurn[m_turnOrder])-1]; //Change text to correct name
        _playerTurnOrderText[m_turnOrder].color = _localPlayerColors[(_whosTurn[m_turnOrder])-1]; //Change color of text box to players color

    }
    private void CreateGame(){

        


        for(int b = 0; b<_boardSize + 1; b++){
            for(int i = 0; i < _boardSize ; i++){
                //create dictionary of ROW of buttons and set their placement, name, and other
                Button rButton =  Instantiate(cornerButton, Vector3.zero, Quaternion.identity) as Button; //make new rButton from prefab
                var rectTransformRow = rButton.GetComponent<RectTransform>(); //get reference of buttons rectTransformRow
                rectTransformRow.SetParent(_backgroundObj.transform); //make rButton child of background panel obj
                rectTransformRow.sizeDelta = new Vector2(82,20); //set buttons size
                //set buttons position on background panel obj
                rectTransformRow.position = new Vector2(_backgroundObj.GetComponent<RectTransform>().rect.width/2 + (_boardSize/2*80) - i * 80,_backgroundObj.GetComponent<RectTransform>().rect.height/2 + (_boardSize/2*80) + 35 - b * 80);
                rButton.name = (b.ToString() + i.ToString() + "r"); //set name of rButton
                _gameBoard.rowButtons.Add(rButton.name, rButton); //add rButton to dictionary with rButton name
                _gameBoard.rowButtons[rButton.name].onClick.AddListener (() => ButtonClicked(rButton, true)); //add listener to rButton so it knows when its clicked and which is clicked. ************ PASSES TRUE SO IT KNOWS ITS A ROW
                _gameBoard.rowClicked.Add(rButton.name,false); //add if rButton is clicked to dictionary with rButton name


                //create dictionary of COLUMNS of buttons and set their placement, name, and other (reversed b & i)**********
                Button cButton = Instantiate(cornerButton, Vector3.zero, Quaternion.identity) as Button; //make new cButton from prefab
                var rectTransformCol = cButton.GetComponent<RectTransform>(); //get reference of buttons rectTransformCol
                rectTransformCol.SetParent(_backgroundObj.transform);//make cButton child of canvas
                rectTransformCol.sizeDelta = new Vector2(82,20); //set buttons size
                //set buttons position on canvas
                rectTransformCol.position = new Vector2(_backgroundObj.GetComponent<RectTransform>().rect.width/2 + (_boardSize/2*80) + 35 - b * 80,_backgroundObj.GetComponent<RectTransform>().rect.height/2 + (_boardSize/2*80) - i * 80);
                //set rotation of buttons.
                rectTransformCol.eulerAngles = new Vector3(rectTransformCol.transform.eulerAngles.x, rectTransformCol.transform.eulerAngles.y, 90);
                cButton.name = (i.ToString() + b.ToString()+ "c");//set name of cButton
                _gameBoard.colButtons.Add(cButton.name, cButton); //add cButton to dictionary with cButton name
                _gameBoard.colButtons[cButton.name].onClick.AddListener (() => ButtonClicked(cButton, false));//add listener to cButton so it knows when its clicked and which is clicked. ************ PASSES FALSE SO IT KNOWS ITS A COL
                _gameBoard.colClicked.Add(cButton.name,false);//add if cButton is clicked to dictionary with cButton name
            }
        }
        //create list of BOXES and set their placement, name, and other
        for(int b = 0; b<_boardSize; b++){
            for(int i = 0; i < _boardSize ; i++){
                if(_gameBoard.colButtons.ContainsKey(b.ToString() + (i + 1).ToString()+ "c") && _gameBoard.rowButtons.ContainsKey((b + 1).ToString() + i .ToString() + "r")){ //check that the boxes are within the correct buttons. (contains key searches dictionary for if that key exists)
                    GameObject _box = Instantiate(box, Vector3.zero, Quaternion.identity) as GameObject;//make box obj from prefab
                    var _rectTransform = _box.GetComponent<RectTransform>(); //get reference of buttons rectTransform
                    _rectTransform.SetParent(_backgroundObj.transform);//make button child of canvas
                    _rectTransform.SetAsFirstSibling(); //make sure box is behind buttons
                    _rectTransform.sizeDelta = new Vector2(75,75);//set Boxes size
                    //set buttons position on canvas
                    _rectTransform.position = new Vector2(_backgroundObj.GetComponent<RectTransform>().rect.width/2 + (_boardSize/2*80) - i * 80,_backgroundObj.GetComponent<RectTransform>().rect.height/2 + (_boardSize/2*80) - b * 80);;
                    //add buttons that are surrounding boxes to list within the box prefab.
                    _box.GetComponent<BoxScript>().boxLines(_gameBoard.rowButtons[b.ToString() + i.ToString() + "r"]); 
                    _box.GetComponent<BoxScript>().boxLines(_gameBoard.rowButtons[(b+1).ToString() + i.ToString()+ "r"]);
                    _box.GetComponent<BoxScript>().boxLines(_gameBoard.colButtons[b.ToString() + i.ToString()+ "c"]);
                    _box.GetComponent<BoxScript>().boxLines(_gameBoard.colButtons[b.ToString() + (i + 1).ToString()+ "c"]);
                    _box.tag = "unchecked";//tag box that it hasnt been taken for points.
                    _box.name = (b.ToString() + i.ToString() + "b"); //name box given x,y coords on board.
                    _gameBoard.boxes.Add(_box); //add box to boxes list.
                }
            }
        }
    }

    private void RandomizeTurns(){
        
        for (int i = 0; i < _numberOfPlayers; i++)
        {
                if(_whosTurn[i] == 0){
                    _whosTurn[i] = Random.Range(1, _numberOfPlayers+1);
                }
                for (int x = 0; x < i; x++)
                {
                    if(_whosTurn[x] == _whosTurn[i] && x!=i){
                        _whosTurn[i] = 0;
                    }
                }
            //Debug.Log(_whosTurn[i]);
        }
        for (int i = 0; i < _numberOfPlayers; i++)
        {
            if(_whosTurn[i] == 0){
                RandomizeTurns();
            }
        }
    }

    public void ButtonClicked(Button m_b, bool m_row){ //function is called when row or column button is pressed, passes button pressed & bool for whether it is a row or col button.
        bool m_alreadyClicked = false; //temp holds if it has been pressed in the past
        if(m_row && _gameBoard.rowClicked[m_b.name] != true){  //check it it is a ROW button && if it HASNT been pressed.
            _gameBoard.rowClicked[m_b.name] = true;
        }else if(!m_row && _gameBoard.colClicked[m_b.name] != true){//check it it is a COL button && if  it HASNT been pressed.
            _gameBoard.colClicked[m_b.name] = true;
        }else{ //tells script that this button has already been pressed in the past
            m_alreadyClicked = true;
        }
        if(!m_alreadyClicked){ //if button hasnt been clicked then change the buttons colour and check if the box needs to be filled in.
            
            if(_whosTurn[_turnRotation] == 1){//FIRST PLAYER
                m_b.GetComponent<Image>().color = Color.blue;
            }else if(_whosTurn[_turnRotation] == 2){//SECOND PLAYER
                m_b.GetComponent<Image>().color = Color.red;
            }else if(_whosTurn[_turnRotation] == 3){//THIRD PLAYER
                m_b.GetComponent<Image>().color = Color.green;
            }
            else if(_whosTurn[_turnRotation] == 4){//FOURTH PLAYER
                m_b.GetComponent<Image>().color = Color.yellow;
            }
           // m_b.GetComponent<Image>().color = _localPlayerColors[_whosTurn[_turnRotation]-1];
            ColorBlock m_tempColor = m_b.GetComponent<Button>().colors; //make temporary color block to change alpha of button to 100%
            m_tempColor.normalColor = m_b.GetComponent<Image>().color; //set normal color to what color button should be.
            m_b.GetComponent<Button>().colors = m_tempColor; //set buttons color block to temp color block

            CheckBoxes(); //check if box needs to be filled in. 
        }
    }
    private void CheckBoxes(){
        
        bool m_didCheck = false; //holds whether there are any unclaimed boxes left
        bool[] m_clicked = new bool[4]; //temp array to hold bools of whether its been clicked.
        bool m_pointGained = false; //holds whether you scored a point or not, basically to change turns, if point gained dont change turn.
        
        foreach(GameObject box in _gameBoard.boxes ){ //go through list of boxes in game
            if(box.tag == "unchecked"){ //make sure box hasnt been claimed already
                List<Button> temp =  box.GetComponent<BoxScript>().buttonList; //temp list of buttons surrounding box
                int counter = 0; //counter to distinguish buttons between row and column
                foreach(Button but in temp){
                    if(counter < 2){ //first two buttons stored on box are always ROW buttons
                        //check and add whether button was m_clicked to temp list
                        if(_gameBoard.rowClicked[but.name]){
                            m_clicked[counter] = true; 
                        }else{
                             m_clicked[counter] = false ;
                        }
                    }else if (counter < 4){//second two buttons stored on box are always COL buttons
                        //check and add whether button was m_clicked to temp list
                        if(_gameBoard.colClicked[but.name]){
                            m_clicked[counter] = true; 
                        }else{ 
                            m_clicked[counter] = false;
                        }

                    }else{ //button shouldnt exist
                        Debug.Log("button shouldnt exist");
                    }   
                    counter ++;
                    
                }
                if(m_clicked[0] && m_clicked[1] && m_clicked[2] && m_clicked[3]){ // if all buttons in temp m_clicked array were pressed do this.
                    box.GetComponent<BoxScript>().ButtonSurrounded(_whosTurn[_turnRotation], _localPlayerColors[_whosTurn[_turnRotation]-1]); //pass to box script to change its colour and depending on whos turn it is.
                    m_pointGained = true; //point is gained
                    _playerScores[_whosTurn[_turnRotation]-1]++;//add points to whoevers turns score.
                    
                }
                
            }
        }
        //if point isnt gained change
        if(!m_pointGained){
                _turnRotation ++;
                if(_turnRotation >= _numberOfPlayers){
                    _turnRotation = 0;
                }
        }
        //check if there are any unchecked boxes left
        foreach(GameObject box in _gameBoard.boxes ){
            if(box.tag == "unchecked"){
                m_didCheck = true;
                break;
            }
        } 
        //if no more unchecked boxes left end game.
        if(!m_didCheck && _currentGamestate != GAMESTATE.GAMEOVER){
            _currentGamestate = GAMESTATE.GAMEOVER;
        }else{
            m_didCheck = false;
        }
        
    }
    private void DisplayWinner(){
        _winnerObj.SetActive(true); //show the player who won and the restart btn
        int[] m_playerPlace = new int[] {0,0,0,0};
        for (int i = 0; i < _numberOfPlayers; i++)
        {
           for (int x = 0; x < _numberOfPlayers; x++)
            { 
                if(i != x){
                    if(_playerScores[i] > _playerScores[x]){
                        m_playerPlace[i] ++;
                    }
                }
            }
        }
        for (int i = 0; i < _numberOfPlayers; i++)
        {
            if(m_playerPlace[i] == 3 && _numberOfPlayers == 4){ //first place
                _winnterText.text = _localPlayerNames[i] + " is the Winner!";
            }else if(m_playerPlace[i] == 2 && _numberOfPlayers == 3){ //second or first
                _winnterText.text = _localPlayerNames[i] + " is the Winner!";
            }else if(m_playerPlace[i] == 1 && _numberOfPlayers == 2){ //third or second or first
                _winnterText.text = _localPlayerNames[i] + " is the Winner!";
            }else if(m_playerPlace[i] == 0){ //last or tie 
                
            }else{//players had the same score and tied
                _winnterText.text = "Tie Game!";
            }
        }
    }

    // -------------------------CLASSES ---------------------------------------
    [System.Serializable] class GameBoard{
        public Dictionary<string, Button> colButtons = new Dictionary<string, Button>(); //dictionary of buttons in column named by their position on a graph, ie 00, 01, 10 (first number is x value, 2nd num is y value ie xy or (0,0) would be 00)
        public Dictionary<string, Button> rowButtons = new Dictionary<string, Button>();//dictionary of buttons in row named by their position on a graph like above
        public Dictionary<string, bool> rowClicked = new Dictionary<string, bool>();//dictionary of bools in row named by their position on a graph, tells whether buttons been pressed or not
        public Dictionary<string, bool> colClicked = new Dictionary<string, bool>();//dictionary of bools in column named by their position on a graph, tells whether buttons been pressed or not
        public List<GameObject> boxes = new List<GameObject>(); //list of boxed (the center of whats between 4 buttons and what changes colour)

        

    }
}

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
    [SerializeField] private GameObject _backgroundPanel; //reference to background panel obj

    [SerializeField] private GameObject _boardSettingsPanel; //reference to panel where you select board size.
    [SerializeField] private Slider _boardSizeSlider; //Reference to slider on board size panel.
    [SerializeField] private Text _boardSizeText;//Reference to size text on board size panel.

    private GameBoard _gameBoard = new GameBoard();
    private enum GAMESTATE { //Enum for game state / what point the game is currently at.
        SETTINGS, 
        PLAYING, 
        GAMEOVER
    };

    private GAMESTATE _currentGamestate= GAMESTATE.SETTINGS; //Actual reference to current game state


    private bool _firstPlayersTurn = false;

    //----------------------------FUNCTIONS-------------------------------------
    void Awake(){ //when this gameobject is awoken do ...
         _canvas = GameObject.FindGameObjectWithTag("Canvas"); //give canvas reference to the canvas obj
    }
    void Update(){
        if(_currentGamestate == GAMESTATE.SETTINGS){ //if you are on the board settings menu
            _boardSize = (int) _boardSizeSlider.value; //make board the size that the slider is at.
            _boardSizeText.text = (_boardSizeSlider.value).ToString(); //make board size text on board settings panel update
        }
    }
    public void GameStartButton(){ //when you click the start button on board setting screen
        _boardSettingsPanel.SetActive(false); //make panel disappear.
        _boardSize = (int) _boardSizeSlider.value; //make board the size that the slider is at.
        CreateGame(); //create the game board with the size in slider
        _currentGamestate = GAMESTATE.PLAYING;//Change gamestate to playing game

    }
    private void CreateGame(){
        
        for(int b = 0; b<_boardSize + 1; b++){
            for(int i = 0; i < _boardSize ; i++){
                //create dictionary of ROW of buttons and set their placement, name, and other
                Button rButton =  Instantiate(cornerButton, Vector3.zero, Quaternion.identity) as Button; //make new rButton from prefab
                var rectTransformRow = rButton.GetComponent<RectTransform>(); //get reference of buttons rectTransformRow
                rectTransformRow.SetParent(_backgroundPanel.transform); //make rButton child of background panel obj
                rectTransformRow.sizeDelta = new Vector2(82,20); //set buttons size
                //set buttons position on background panel obj
                rectTransformRow.position = new Vector2(_backgroundPanel.GetComponent<RectTransform>().rect.width/2 + (_boardSize/2*80) - i * 80,_backgroundPanel.GetComponent<RectTransform>().rect.height/2 + (_boardSize/2*80) + 35 - b * 80);
                rButton.name = (b.ToString() + i.ToString() + "r"); //set name of rButton
                _gameBoard.rowButtons.Add(rButton.name, rButton); //add rButton to dictionary with rButton name
                _gameBoard.rowButtons[rButton.name].onClick.AddListener (() => ButtonClicked(rButton, true)); //add listener to rButton so it knows when its clicked and which is clicked. ************ PASSES TRUE SO IT KNOWS ITS A ROW
                _gameBoard.rowClicked.Add(rButton.name,false); //add if rButton is clicked to dictionary with rButton name


                //create dictionary of COLUMNS of buttons and set their placement, name, and other (reversed b & i)**********
                Button cButton = Instantiate(cornerButton, Vector3.zero, Quaternion.identity) as Button; //make new cButton from prefab
                var rectTransformCol = cButton.GetComponent<RectTransform>(); //get reference of buttons rectTransformCol
                rectTransformCol.SetParent(_backgroundPanel.transform);//make cButton child of canvas
                rectTransformCol.sizeDelta = new Vector2(82,20); //set buttons size
                //set buttons position on canvas
                rectTransformCol.position = new Vector2(_backgroundPanel.GetComponent<RectTransform>().rect.width/2 + (_boardSize/2*80) + 35 - b * 80,_backgroundPanel.GetComponent<RectTransform>().rect.height/2 + (_boardSize/2*80) - i * 80);
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
                    _rectTransform.SetParent(_backgroundPanel.transform);//make button child of canvas
                    _rectTransform.SetAsFirstSibling(); //make sure box is behind buttons
                    _rectTransform.sizeDelta = new Vector2(75,75);//set Boxes size
                    //set buttons position on canvas
                    _rectTransform.position = new Vector2(_backgroundPanel.GetComponent<RectTransform>().rect.width/2 + (_boardSize/2*80) - i * 80,_backgroundPanel.GetComponent<RectTransform>().rect.height/2 + (_boardSize/2*80) - b * 80);;
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
        //ggText.GetComponent<RectTransform>().SetAsLastSibling(); // make the gg text be on top of everything else. 
    }

    

    public void ButtonClicked(Button _b, bool _row){ //function is called when row or column button is pressed, passes button pressed & bool for whether it is a row or col button.
        bool alreadyClicked = false; //temp holds if it has been pressed in the past
        if(_row && _gameBoard.rowClicked[_b.name] != true){  //check it it is a ROW button && if it HASNT been pressed.
            _gameBoard.rowClicked[_b.name] = true;
        }else if(!_row && _gameBoard.colClicked[_b.name] != true){//check it it is a COL button && if  it HASNT been pressed.
            _gameBoard.colClicked[_b.name] = true;
        }else{ //tells script that this button has already been pressed in the past
            alreadyClicked = true;
        }
        if(!alreadyClicked){ //if button hasnt been clicked then change the buttons colour and check if the box needs to be filled in.
             if(_firstPlayersTurn){
                _b.GetComponent<Image>().color = Color.blue;
            }else if(!_firstPlayersTurn){
                _b.GetComponent<Image>().color = Color.red;
            }
            ColorBlock m_tempColor = _b.GetComponent<Button>().colors; //make temporary color block to change alpha of button to 100%
            m_tempColor.normalColor = _b.GetComponent<Image>().color; //set normal color to what color button should be.
            _b.GetComponent<Button>().colors = m_tempColor; //set buttons color block to temp color block

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
                    box.GetComponent<BoxScript>().ButtonSurrounded(_firstPlayersTurn); //pass to box script to change its colour and depending on whos turn it is.
                    m_pointGained = true; //point is gained
                    //add points to whoevers turns score.
                    if(_firstPlayersTurn){ 
                        //p1Score++;
                    }else if(!_firstPlayersTurn){
                        //p2Score++;
                    }
                    
                }
                
            }
        }
        //if point isnt gained change
        if(!m_pointGained){
                _firstPlayersTurn = !_firstPlayersTurn;
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

    // -------------------------CLASSES ---------------------------------------
    [System.Serializable] class GameBoard{
        public Dictionary<string, Button> colButtons = new Dictionary<string, Button>(); //dictionary of buttons in column named by their position on a graph, ie 00, 01, 10 (first number is x value, 2nd num is y value ie xy or (0,0) would be 00)
        public Dictionary<string, Button> rowButtons = new Dictionary<string, Button>();//dictionary of buttons in row named by their position on a graph like above
        public Dictionary<string, bool> rowClicked = new Dictionary<string, bool>();//dictionary of bools in row named by their position on a graph, tells whether buttons been pressed or not
        public Dictionary<string, bool> colClicked = new Dictionary<string, bool>();//dictionary of bools in column named by their position on a graph, tells whether buttons been pressed or not
        public List<GameObject> boxes = new List<GameObject>(); //list of boxed (the center of whats between 4 buttons and what changes colour)

    }
}

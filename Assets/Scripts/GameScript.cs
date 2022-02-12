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

    //----------------------------FUNCTIONS-------------------------------------
    void Awake(){ //when this gameobject is awoken do ...
         _canvas = GameObject.FindGameObjectWithTag("Canvas"); //give canvas reference to the canvas obj
    }
    void Update(){
        if(_currentGamestate == GAMESTATE.SETTINGS){ //if you are on the board settings menu
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
        //create dictionary of ROW of buttons and set their placement, name, and other
        for(int b = 0; b<_boardSize + 1; b++){
            for(int i = 0; i < _boardSize ; i++){
                Button button =  Instantiate(cornerButton, Vector3.zero, Quaternion.identity) as Button; //make new button from prefab
                var rectTransform = button.GetComponent<RectTransform>(); //get reference of buttons rectTransform
                rectTransform.SetParent(_backgroundPanel.transform); //make button child of background panel obj
                rectTransform.offsetMin = Vector2.zero; //make off set 0
                rectTransform.offsetMax = Vector2.zero;//make off set 0
                rectTransform.sizeDelta = new Vector2(82,20); //set buttons size
                //set buttons position on background panel obj
                rectTransform.position = new Vector2(_backgroundPanel.GetComponent<RectTransform>().rect.width/2 + (_boardSize/2*80) - i * 80,_backgroundPanel.GetComponent<RectTransform>().rect.height/2 + (_boardSize/2*80) + 35 - b * 80);
                button.name = b.ToString() + i.ToString(); //set name of button
                _gameBoard.rowButtons.Add(button.name, button); //add button to dictionary with button name
               // _gameBoard.rowButtons[button.name].onClick.AddListener (() => ButtonClicked(button, true)); //add listener to button so it knows when its clicked and which is clicked.
                _gameBoard.rowClicked.Add(button.name,false); //add if button is clicked to dictionary with button name
            }
        }
        //create dictionary of COLUMNS of buttons and set their placement, name, and other
          for(int b = 0; b<_boardSize ; b++){
            for(int i = 0; i < _boardSize + 1; i++){
                Button button = Instantiate(cornerButton, Vector3.zero, Quaternion.identity) as Button; //make new button from prefab
                var rectTransform = button.GetComponent<RectTransform>(); //get reference of buttons rectTransform
                rectTransform.SetParent(_backgroundPanel.transform);//make button child of canvas
                rectTransform.offsetMin = Vector2.zero;//make off set 0
                rectTransform.offsetMax = Vector2.zero;//make off set 0
                rectTransform.sizeDelta = new Vector2(82,20); //set buttons size
                //set buttons position on canvas
                rectTransform.position = new Vector2(_backgroundPanel.GetComponent<RectTransform>().rect.width/2 + (_boardSize/2*80) + 35 - i * 80,_backgroundPanel.GetComponent<RectTransform>().rect.height/2 + (_boardSize/2*80) - b * 80);
                //set rotation of buttons.
                rectTransform.eulerAngles = new Vector3(rectTransform.transform.eulerAngles.x, rectTransform.transform.eulerAngles.y, 90);
                button.name = b.ToString() + i.ToString();//set name of button
                _gameBoard.colButtons.Add(button.name, button); //add button to dictionary with button name
                //_gameBoard.colButtons[b.ToString() + i.ToString()].onClick.AddListener (() => ButtonClicked(button, false));//add listener to button so it knows when its clicked and which is clicked.
                _gameBoard.colClicked.Add(button.name,false);//add if button is clicked to dictionary with button name
            }
        }
        //create list of BOXES and set their placement, name, and other
        for(int b = 0; b<_boardSize; b++){
            for(int i = 0; i < _boardSize ; i++){
                if(_gameBoard.colButtons.ContainsKey(b.ToString() + (i + 1).ToString()) && _gameBoard.rowButtons.ContainsKey((b + 1).ToString() + i .ToString())){ //check that the boxes are within the correct buttons.
                    GameObject _box = Instantiate(box, Vector3.zero, Quaternion.identity) as GameObject;//make box obj from prefab
                    var _rectTransform = _box.GetComponent<RectTransform>(); //get reference of buttons rectTransform
                    _rectTransform.SetParent(_backgroundPanel.transform);//make button child of canvas
                    _rectTransform.offsetMin = Vector2.zero;//make off set 0
                    _rectTransform.offsetMax = Vector2.zero;//make off set 0
                    _rectTransform.SetAsFirstSibling(); //make sure box is behind buttons
                    _rectTransform.sizeDelta = new Vector2(75,75);//set buttons size
                    //set buttons position on canvas
                    _rectTransform.position = new Vector2(_backgroundPanel.GetComponent<RectTransform>().rect.width/2 + (_boardSize/2*80) - i * 80,_backgroundPanel.GetComponent<RectTransform>().rect.height/2 + (_boardSize/2*80) - b * 80);;
                    //add buttons that are surrounding boxes to list within the box prefab.
                    //_box.GetComponent<BoxScript>().boxLines(rowButtons[b.ToString() + i.ToString()]); 
                    //_box.GetComponent<BoxScript>().boxLines(rowButtons[(b+1).ToString() + i.ToString()]);
                   // _box.GetComponent<BoxScript>().boxLines(colButtons[b.ToString() + i.ToString()]);
                   // _box.GetComponent<BoxScript>().boxLines(colButtons[b.ToString() + (i + 1).ToString()]);
                   // _box.tag = "unchecked";//tag box that it hasnt been taken for points.
                    _box.name = (b + i).ToString(); //name box given x,y coords on board.
                    //boxes.Add(_box); //add box to boxes list.
                }
            }
        }
        //ggText.GetComponent<RectTransform>().SetAsLastSibling(); // make the gg text be on top of everything else. 
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

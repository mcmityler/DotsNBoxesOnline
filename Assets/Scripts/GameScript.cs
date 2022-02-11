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

    //----------------------------FUNCTIONS-------------------------------------


    // -------------------------CLASSES ---------------------------------------
    [System.Serializable] class GameBoard{
        Dictionary<string, Button> colButtons = new Dictionary<string, Button>(); //dictionary of buttons in column named by their position on a graph, ie 00, 01, 10 (first number is x value, 2nd num is y value ie xy or (0,0) would be 00)
        Dictionary<string, Button> rowButtons = new Dictionary<string, Button>();//dictionary of buttons in row named by their position on a graph like above
        Dictionary<string, bool> rowClicked = new Dictionary<string, bool>();//dictionary of bools in row named by their position on a graph, tells whether buttons been pressed or not
        Dictionary<string, bool> colClicked = new Dictionary<string, bool>();//dictionary of bools in column named by their position on a graph, tells whether buttons been pressed or not
        public List<GameObject> boxes = new List<GameObject>(); //list of boxed (the center of whats between 4 buttons and what changes colour)

    }
}

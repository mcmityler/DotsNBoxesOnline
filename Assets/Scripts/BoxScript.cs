using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxScript : MonoBehaviour
{
    public List<Button> buttonList = new List<Button>(); //List of what buttons this box is surrounded by is passed to it through the creation of the box.
    public void boxLines(Button _button){ //function to add buttons to list of buttons surrounding this box.
        buttonList.Add(_button);
    }
    public void ButtonSurrounded(bool _fpTurn){
        //change obj tag
        gameObject.tag = "checked";
        //change colour of obj depending on players turn
        if(_fpTurn){

            gameObject.GetComponent<Image>().color = Color.blue;
        }
        else if(!_fpTurn){

            gameObject.GetComponent<Image>().color = Color.red;
        }
    }
}

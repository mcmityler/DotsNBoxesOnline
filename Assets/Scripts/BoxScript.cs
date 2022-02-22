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
    public void ButtonSurrounded(int m_whosTurn, Color32 m_turnColor){
        //change obj tag
        gameObject.tag = "checked";
        gameObject.GetComponent<Image>().color = m_turnColor;
        //change colour of obj depending on players turn
        /*if(m_whosTurn == 1){ //First player turn

            gameObject.GetComponent<Image>().color = m_turnColor;
        }
        else if(m_whosTurn == 2){//Second player turn

            gameObject.GetComponent<Image>().color = Color.red;
        }else if(m_whosTurn == 3){//Third player turn

            gameObject.GetComponent<Image>().color = Color.green;
        }else if(m_whosTurn == 4){//Fourth player turn

            gameObject.GetComponent<Image>().color = Color.yellow;
        }*/
    }
}

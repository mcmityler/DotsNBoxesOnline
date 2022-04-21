using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxScript : MonoBehaviour
{
    private Color32 _darkblue = new Color32(0, 90, 188, 255);
    private Color32 _darkred = new Color32(137, 0, 0, 255);
    private Color32 _darkgreen = new Color32(1, 123, 0, 255);
    private Color32 _darkyellow = new Color32(209, 197, 0, 255);
    public List<Button> buttonList = new List<Button>(); //List of what buttons this box is surrounded by is passed to it through the creation of the box.
    public void boxLines(Button _button){ //function to add buttons to list of buttons surrounding this box.
        buttonList.Add(_button);
    }
    public void ButtonSurrounded(int m_whosTurn, Color32 m_turnColor, Color32 m_blue, Color32 m_red, Color32 m_green, Color32 m_yellow){
        Color32 m_changeColorTo = new Color32 (0,0,0,0);
        if(m_turnColor.Equals(m_blue)){ //blue
            m_changeColorTo = _darkblue;
        }
        else if(m_turnColor.Equals(m_red)){ //red
            m_changeColorTo = _darkred;
        }
        else if(m_turnColor.Equals(m_green)){ //green
            m_changeColorTo = _darkgreen;
        }
        else if(m_turnColor.Equals(m_yellow)){ //yellow
            m_changeColorTo = _darkyellow;
        }
        //change obj tag
        gameObject.tag = "checked";
        gameObject.GetComponent<Animator>().SetBool("fillBox", true);
        gameObject.GetComponent<Image>().color = m_changeColorTo;
        gameObject.GetComponent<AudioSource>().Play();
        
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

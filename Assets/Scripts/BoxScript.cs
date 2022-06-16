using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*

THIS SCRIPT HAS EVERYTHING THAT DEALS WITH THE BOX CENTERS
by Tyler McMillan
*/
public class BoxScript : MonoBehaviour
{
    //-------------------------Dark Colours---------------------------
    private Color32 _darkblue = new Color32(0, 90, 188, 255);
    private Color32 _darkred = new Color32(137, 0, 0, 255);
    private Color32 _darkgreen = new Color32(1, 123, 0, 255);
    private Color32 _darkyellow = new Color32(209, 197, 0, 255);
    private Color32 _darkpurple = new Color32(138, 0, 156, 255);
    private Color32 _darkorange = new Color32(255, 79, 0, 255);
    private Color32 _darklightblue = new Color32(0, 152, 154, 255);
    public List<Button> buttonList = new List<Button>(); //List of what buttons this box is surrounded by is passed to it through the creation of the box.
    public void boxLines(Button _button){ //function to add buttons to list of buttons surrounding this box.
        buttonList.Add(_button);
    }
    public void ButtonSurrounded( Color32 m_turnColor, Color32 m_blue, Color32 m_red, Color32 m_green, Color32 m_yellow, Color32 m_purple, Color32 m_orange, Color32 m_lightblue){
        Color32 m_changeColorTo = new Color32 (0,0,0,0); //hold what colour to turn the box into
        
        //------------Change colour to dark version --------------
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
        else if(m_turnColor.Equals(m_purple)){ //purple
            m_changeColorTo = _darkpurple;
        }
        else if(m_turnColor.Equals(m_orange)){ //orange
            m_changeColorTo = _darkorange;
        }
        else if(m_turnColor.Equals(m_lightblue)){ //lightblue
            m_changeColorTo = _darklightblue;
        }
        Debug.Log(m_changeColorTo);
        //change obj tag
        gameObject.tag = "checked";
        if(Screen.width >= 1800 || !Application.isEditor){
         gameObject.GetComponent<Animator>().SetBool("fullScreen", true); //play animation
        }
        gameObject.GetComponent<Animator>().SetBool("fillBox", true); //play animation
        gameObject.GetComponent<Image>().color = m_changeColorTo; //Change box colour 
        gameObject.GetComponent<AudioSource>().Play(); //play fill sound
        
    }
}

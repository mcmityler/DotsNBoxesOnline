using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColourScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler// required interface when using the OnPointerEnter method.
{
    //colour references because they have different alpha levels
    private Color32 _blue = new Color32(0, 16, 255, 60);
    private Color32 _red = new Color32(255, 0, 10, 30);
    private Color32 _green = new Color32(37,181,27, 30);
    private Color32 _yellow = new Color32(255, 246, 0, 30);
    private Color32 _purple = new Color32(225, 0, 255, 30);
    private Color32 _orange = new Color32(255, 129, 0, 30);
    private Color32 _lightblue = new Color32(63,163,163, 30);
    private Color32 _black = new Color32(0, 0, 0, 90);
    [SerializeField] private Image _buttonBlurBG; //background of button(blur)
    [SerializeField] private string _thisButtonsColour; //string set on button to tell it what colour it is
    private SocketManager _socketManager; //reference to socket manager
    private string _myCurrentColour; //what colour is the player currently using
    void Awake(){
        _socketManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<SocketManager>(); //get reference
    }
    void Update(){
        if(_buttonBlurBG.IsActive()){ //if the blur bg is active, check what colour the player is using
            string m_tempColour = _socketManager.GetMyColour();
            if(m_tempColour != _myCurrentColour){

                _myCurrentColour = m_tempColour;
                if(_myCurrentColour != _thisButtonsColour){ //if you change from this button colour to another reset back to originalblur color
                    ChangeToOriginal();
                }
            }
        }
        if(_myCurrentColour == _thisButtonsColour){ //if my colour selected is this button, set blur to black
             _buttonBlurBG.color = _black;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)//while cursor enters button
    {
        if(_myCurrentColour != _thisButtonsColour){
            _buttonBlurBG.color = Color.white;
        }
        
    }
    public void OnPointerExit(PointerEventData eventData) //while cursor exits button
    {
        ChangeToOriginal();
    }
    void ChangeToOriginal(){ //change button to colour on _thisButtonsColour(set in inspector)
        Color32 _tempColour = new Color32 (0,0,0,0);
        switch (_thisButtonsColour)
        {
            case "blue":
                _tempColour = _blue;
                break;
            case "red":
            _tempColour = _red;
                break;
            case "green":
            _tempColour = _green;
                break;
            case "yellow":
            _tempColour = _yellow;
                break;
            case "purple":
            _tempColour = _purple;
                break;
            case "orange":
            _tempColour = _orange;
            Debug.Log("set to colour");
                break;
            case "lightblue":
            _tempColour = _lightblue;
                break;
        }
        if(_myCurrentColour != _thisButtonsColour){
            _buttonBlurBG.color = _tempColour;
        }
    }

   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public GameScript gameScript;
    [SerializeField] private GameObject _mainMenuObj, _helpScreenObj, _creditScreenObj, _loginScreenObj;

    public void LocalGameButton(){
        gameScript.SetLocalGame(true);
        _mainMenuObj.SetActive(false);
    }
    public void HelpScreenButton(){
        _helpScreenObj.SetActive(true);
    }
    public void CreditScreenButton(){
        _creditScreenObj.SetActive(true);
    }
     public void MultiplayerScreenButton(){
        _loginScreenObj.SetActive(true);
    }public void BackButton(){
        _creditScreenObj.SetActive(false);
        _helpScreenObj.SetActive(false);
        _loginScreenObj.SetActive(false);
        _mainMenuObj.SetActive(true);
    }public void QuitButton(){
        Application.Quit();
    }



    string _tempUser = "";
    string _tempPassword = "";

    bool _correctUserandPassword = false;
    [SerializeField] Text _incorrectText;
    public void LoginButton(){
        if(_tempPassword == "" || _tempUser == ""){ //if username or password is left blank display error message
            Debug.Log("must enter a username and password");
            _incorrectText.text = "Incorrect User/Password";
        }else{
            //SEND password and user to server and examine them.
            //Check if user name is in system

            //Check if password matches username in system
            _correctUserandPassword = true;
            _incorrectText.text = ""; //Reset back to nothing once you enter a correct U & P
        }

        //if the user and password are correct login...
        if(_correctUserandPassword){
            
        }
        

    }
    
}

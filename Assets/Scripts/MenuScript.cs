using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    public GameScript gameScript; //reference to gamescript
    [SerializeField] private GameObject _mainMenuObj, _helpScreenObj, _creditScreenObj, _loginScreenObj; //reference to screen objects to in/visable

    public void LocalGameButton()//called by local game button on main menu
    {
        gameScript.SetLocalGame(true); //tell gamescript its a local game
        _mainMenuObj.SetActive(false);
    }
    public void HelpScreenButton()//called by help button on main menu
    {
        _helpScreenObj.SetActive(true);
    }
    public void CreditScreenButton()//called by credit button on main menu
    {
        _creditScreenObj.SetActive(true);
    }
    public void MultiplayerScreenButton()//called by multiplayer button on main menu
    {
        _loginScreenObj.SetActive(true);
        gameScript.SetLocalGame(false); //tell gamescript its a multiplayer game
    }
    public void BacktoMainButton()//called by back buttons on every screen ==> goes to main menu.
    {
        _creditScreenObj.SetActive(false);
        _helpScreenObj.SetActive(false);
        _loginScreenObj.SetActive(false);
        _mainMenuObj.SetActive(true);
    }
    public void QuitButton()//called by quit button on main menu
    {
        Application.Quit();
    }

}

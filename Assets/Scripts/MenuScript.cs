using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    private GameScript _gameScript; //reference to gamescript
    private SocketManager _socketManager;
    [SerializeField] private GameObject _mainMenuObj, _helpScreenObj, _creditScreenObj, _loginScreenObj, _lobbyMenuSceenScreenObj, _myAccountScreenObj, _keyLobbyObj, _pauseMenuObj, _connectionObj, _colourScreenObj, _localSettingsObj; //reference to screen objects to in/visable
    private bool _myAccountScreenVisable,_keyLobbyVisable, _pauseMenuVisable = false; //bool to control screen visability toggle (so i can use the same func for back button and my account button)

    void Awake(){
        _gameScript = this.GetComponent<GameScript>();///set reference to gamescript
        _socketManager = this.GetComponent<SocketManager>();///set reference to socketmanager
        
    }
    void Update(){
        if (Input.GetButtonDown("Escape")) 
        {
            _pauseMenuObj.GetComponent<RectTransform>().SetAsLastSibling(); //so that i can have the pause menu behind background when starting so you dont see beginning animation.
            PauseMenuToggle();
        }
    }
    public void LocalGameButton()//called by local game button on main menu
    {
        _gameScript.SetLocalGame(true); //tell gamescript its a local game
        _mainMenuObj.SetActive(false);
    }
    public void HelpScreenButton()//called by help button on main menu
    {
        _helpScreenObj.SetActive(true);
        _socketManager.SetSOCKETGameState("HELPSCREEN");
        _gameScript.SetGSGameState("HELPSCREEN");
    }
    public void CreditScreenButton()//called by credit button on main menu
    {
        _creditScreenObj.SetActive(true);
        _socketManager.SetSOCKETGameState("CREDITSCREEN");
        _gameScript.SetGSGameState("CREDITSCREEN");
    }
    public void MultiplayerScreenButton()//called by multiplayer button on main menu
    {
        _loginScreenObj.SetActive(true);
        _gameScript.SetLocalGame(false); //tell gamescript its a multiplayer game
        _socketManager.startUDP(); //Start UDP to connect to AWS
        _connectionObj.SetActive(true);
    }
    public void BacktoMainButton()//called by back buttons on every screen ==> goes to main menu.
    {
        //hide all screens that need to be hidden
        _mainMenuObj.SetActive(true);
        _helpScreenObj.SetActive(false);
        _creditScreenObj.SetActive(false);
        _loginScreenObj.SetActive(false);
        _lobbyMenuSceenScreenObj.SetActive(false); 
        _myAccountScreenObj.SetActive(false);
        _keyLobbyObj.SetActive(false);
        _pauseMenuObj.SetActive(false);
        _connectionObj.SetActive(false); 
        _colourScreenObj.SetActive(false);

        //show screens that need to be shown
        _localSettingsObj.SetActive(true);

        _gameScript.RestartButton(true); //destroy gameboard and reset names
        _mainMenuObj.SetActive(true); //set main menu active
        //change gamestates.
        _socketManager.SetSOCKETGameState("STARTMENU"); 
        _gameScript.SetGSGameState("STARTMENU");
        _socketManager.OnDestroy(); //destroy UDP if it is running and you go back to main menu
    }
    public void QuitButton()//called by quit button on main menu / lobby menu
    {
        _socketManager.ExitGameButton();
        Application.Quit();
    }
    public void OpenLobbyMenu()//called by login button // show lobby menu
    {
        //show lobby menu
        _lobbyMenuSceenScreenObj.SetActive(true);
        _loginScreenObj.SetActive(false);
        _colourScreenObj.SetActive(false);
        _myAccountScreenObj.SetActive(false);
        //set gamestates.
        _socketManager.SetSOCKETGameState("LOBBYMENU");
        _gameScript.SetGSGameState("LOBBYMENU");
    }
    public void BackToLoginScreen(){ //called from back button on multiplayer menu screen
        //show login screen
        _lobbyMenuSceenScreenObj.SetActive(false);
        _loginScreenObj.SetActive(true);
        _socketManager.SetUserNull();
        //set gamestates
        _socketManager.SetSOCKETGameState("LOGINREGISTER");
        _gameScript.SetGSGameState("LOGINREGISTER");
    }
    public void MyAccountScreenToggle(){ //toggle my account screen visibility
        _myAccountScreenObj.SetActive(true);
        _colourScreenObj.SetActive(false);
        _lobbyMenuSceenScreenObj.SetActive(false);
        _socketManager.SetSOCKETGameState("MYACCOUNT");
        _gameScript.SetGSGameState("MYACCOUNT");
    }
    public void LobbyKeyScreenVisable(){ //toggle lobbyKey screen visibility
        _keyLobbyObj.SetActive(true);
        _lobbyMenuSceenScreenObj.SetActive(false);
    }
     public void LobbyKeyScreenInvisable(){ //toggle lobbyKey screen visibility
        _keyLobbyObj.SetActive(false);
        _lobbyMenuSceenScreenObj.SetActive(true);
    }
    public void PauseMenuToggle(){ //toggle pause menu visibility
        
        _pauseMenuVisable = !_pauseMenuVisable;
        
        _pauseMenuObj.GetComponent<Animator>().SetBool("Paused", _pauseMenuVisable); //animator instead of setting active so that i can hopefully change colours while its not there.
    }
    public void StartMPGame(){ //turn off multiplayer screens so you can see the gameboard
        _mainMenuObj.SetActive(false);
        _lobbyMenuSceenScreenObj.SetActive(false);
        _loginScreenObj.SetActive(false);
        _keyLobbyObj.SetActive(false);
    }
    public void StopMPGame(){ //turn back on the multiplayer screens so you can see UI
        _mainMenuObj.SetActive(true);
        _lobbyMenuSceenScreenObj.SetActive(true);
        _loginScreenObj.SetActive(true);
    }
    public void TimedoutMPGame(){ //turn on/off screens needed when you time out (back to main menu)
        _connectionObj.SetActive(false);
        _mainMenuObj.SetActive(true);
        _loginScreenObj.SetActive(false);
    }
    public void ColourScreen(){ //turn on colour screen when button clicked inside of my account screen
        _colourScreenObj.SetActive(true);
        _myAccountScreenObj.SetActive(false);
        _socketManager.SelectColourButtonOnOpen();
        _socketManager.SetSOCKETGameState("COLOURSCREEN");
        _gameScript.SetGSGameState("COLOURSCREEN");
    }

}

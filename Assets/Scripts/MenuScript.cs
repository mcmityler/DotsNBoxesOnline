using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    private GameScript _gameScript; //reference to gamescript
    private SocketManager _socketManager;
    [SerializeField] private GameObject _mainMenuObj, _helpScreenObj, _creditScreenObj, _loginScreenObj, _lobbyMenuSceenScreenObj, _myAccountScreenObj, _keyLobbyObj, _pauseMenuObj, _connectionObj, _colourScreenObj; //reference to screen objects to in/visable
    private bool _myAccountScreenVisable,_keyLobbyVisable, _pauseMenuVisable = false; //bool to control screen visability toggle (so i can use the same func for back button and my account button)

    void Awake(){
        _gameScript = this.GetComponent<GameScript>();///set reference to gamescript
        _socketManager = this.GetComponent<SocketManager>();///set reference to socketmanager
    }
    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
        _socketManager.SetSOCKETGameState("STARTMENU");
        _gameScript.SetGSGameState("STARTMENU");
        _creditScreenObj.SetActive(false);
        _helpScreenObj.SetActive(false);
        _loginScreenObj.SetActive(false);
        _lobbyMenuSceenScreenObj.SetActive(false);
        _connectionObj.SetActive(false);

        _mainMenuObj.SetActive(true);

        _socketManager.OnDestroy(); //destroy UDP if it is running and you go back to main menu
    }
    public void QuitButton()//called by quit button on main menu / lobby menu
    {
        _socketManager.ExitGameButton();
        Application.Quit();
    }
    public void LobbyMenuSceenButton()//called by multiplayer button on main menu 
    {
        //show lobby menu
        _lobbyMenuSceenScreenObj.SetActive(true);
        _colourScreenObj.SetActive(false);
        _myAccountScreenObj.SetActive(false);
        //set gamestates.
        _socketManager.SetSOCKETGameState("LOBBYMENU");
        _gameScript.SetGSGameState("LOBBYMENU");
    }
    public void MyAccountScreenToggle(){ //toggle my account screen visibility
        _myAccountScreenObj.SetActive(true);
        _socketManager.SetSOCKETGameState("MYACCOUNT");
        _gameScript.SetGSGameState("MYACCOUNT");
    }
    public void LobbyKeyScreenToggle(){ //toggle lobbyKey screen visibility
        _keyLobbyVisable = !_keyLobbyVisable;
        _keyLobbyObj.SetActive(_keyLobbyVisable);
    }
    public void PauseMenuToggle(){ //toggle lobbyKey screen visibility
        _pauseMenuVisable = !_pauseMenuVisable;
        _pauseMenuObj.SetActive(_pauseMenuVisable);
    }
    public void StartMPGame(){
        _mainMenuObj.SetActive(false);
        _lobbyMenuSceenScreenObj.SetActive(false);
        _loginScreenObj.SetActive(false);
    }
    public void StopMPGame(){
        _mainMenuObj.SetActive(true);
        _lobbyMenuSceenScreenObj.SetActive(true);
        _loginScreenObj.SetActive(true);
    }
    public void TimedoutMPGame(){
        _connectionObj.SetActive(false);
        _mainMenuObj.SetActive(true);
        _loginScreenObj.SetActive(false);
    }
    public void checkmarkSound(Toggle _passwordCheckbox){
        
        if(_passwordCheckbox.isOn){
            FindObjectOfType<AudioManager>().Play("checkSound");
        }else{
            FindObjectOfType<AudioManager>().Play("uncheckSound");
        }
        _socketManager.PasswordVisibleToggle(_passwordCheckbox);
    }

    public void ColourScreen(){
        _colourScreenObj.SetActive(true);
        _socketManager.SetColourButtonSelected();
        _socketManager.SetSOCKETGameState("COLOURSCREEN");
        _gameScript.SetGSGameState("COLOURSCREEN");
    }

}

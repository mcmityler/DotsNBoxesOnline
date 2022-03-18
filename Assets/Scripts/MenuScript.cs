using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    private GameScript _gameScript; //reference to gamescript
    private SocketManager _socketManager;
    [SerializeField] private GameObject _mainMenuObj, _helpScreenObj, _creditScreenObj, _loginScreenObj, _lobbyMenuSceenScreenObj, _myAccountScreenObj, _keyLobbyObj, _pauseMenuObj; //reference to screen objects to in/visable
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
    }
    public void CreditScreenButton()//called by credit button on main menu
    {
        _creditScreenObj.SetActive(true);
    }
    public void MultiplayerScreenButton()//called by multiplayer button on main menu
    {
        _loginScreenObj.SetActive(true);
        _gameScript.SetLocalGame(false); //tell gamescript its a multiplayer game
        _socketManager.startUDP(); //Start UDP to connect to AWS
    }
    public void BacktoMainButton()//called by back buttons on every screen ==> goes to main menu.
    {
        _creditScreenObj.SetActive(false);
        _helpScreenObj.SetActive(false);
        _loginScreenObj.SetActive(false);
        _lobbyMenuSceenScreenObj.SetActive(false);
        
        _mainMenuObj.SetActive(true);

        _socketManager.DestroyUDP(); //destroy UDP if it is running and you go back to main menu
    }
    public void QuitButton()//called by quit button on main menu / lobby menu
    {
        Application.Quit();
    }
    public void LobbyMenuSceenButton()//called by multiplayer button on main menu 
    {
        //show lobby menu
        _lobbyMenuSceenScreenObj.SetActive(true);
        //set gamestates.
        _socketManager.SetSOCKETGameState("LOBBYMENU");
        _gameScript.SetGSGameState("LOBBYMENU");
    }
    public void MyAccountScreenToggle(){ //toggle my account screen visibility
        _myAccountScreenVisable = !_myAccountScreenVisable;
        _myAccountScreenObj.SetActive(_myAccountScreenVisable);
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
        _mainMenuObj.SetActive(true);
    }

}

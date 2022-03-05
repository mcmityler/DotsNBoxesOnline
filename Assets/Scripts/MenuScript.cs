using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    public GameScript gameScript;
    [SerializeField] private GameObject _mainMenuObj, _helpScreenObj, _creditScreenObj;

    public void LocalGameButton(){
        gameScript.SetLocalGame(true);
        _mainMenuObj.SetActive(false);
    }
    public void HelpScreenButton(){
        _helpScreenObj.SetActive(true);
    }
    public void CreditScreenButton(){
        _creditScreenObj.SetActive(true);
    }public void BackButton(){
        _creditScreenObj.SetActive(false);
        _helpScreenObj.SetActive(false);
        _mainMenuObj.SetActive(true);
    }public void QuitButton(){
        Application.Quit();
    }
    
}

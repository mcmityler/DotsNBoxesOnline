using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabScript : MonoBehaviour
{
    [SerializeField] private InputField _usernameInput, _passwordInput;
    private int _loginScreenInputCounter = -1;

    void Update(){
        if(Input.GetKeyDown(KeyCode.Tab) && Input.GetKeyDown(KeyCode.LeftShift)){
            _loginScreenInputCounter --;
            if(_loginScreenInputCounter < 0){
                _loginScreenInputCounter = 1;
            }
            SelectLoginInput();
        }
        else if(Input.GetKeyDown(KeyCode.Tab)){
            _loginScreenInputCounter ++;
            if(_loginScreenInputCounter > 1){
                _loginScreenInputCounter = 0;
            }
            SelectLoginInput();
        }
    }
    void SelectLoginInput(){
        switch(_loginScreenInputCounter){
            case 0:
                _usernameInput.Select();
                break;
            case 1:
                _passwordInput.Select();
                break;
        }
    }
    public void UsernameSelected() {
        _loginScreenInputCounter = 0;
    }
    public void PasswordSelected() {
        _loginScreenInputCounter = 1;
    }
}

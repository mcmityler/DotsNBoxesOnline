using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/*

THIS SCRIPT HAS EVERYTHING TO DO WITH TABING BETWEEN TEXT BOXES
by Tyler McMillan
*/
public class TabScript : MonoBehaviour
{
    [SerializeField] private TMP_InputField _usernameInput, _passwordInput;
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

    public void PasswordVisibleToggle(Toggle m_passwordVisibility)
    {
        if (m_passwordVisibility.isOn)
        {
            _passwordInput.contentType = TMP_InputField.ContentType.Password;
        }
        else
        {
            _passwordInput.contentType = TMP_InputField.ContentType.Standard;
        }
        PasswordSelected();
        SelectLoginInput();
        _passwordInput.caretPosition = _passwordInput.text.Length;
        //_passwordInput.MoveTextEnd(false);
    }
}

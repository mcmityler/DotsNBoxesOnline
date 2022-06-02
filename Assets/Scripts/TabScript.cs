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

    [SerializeField] private TMP_InputField[] _localNameInput;
    private int _localInputCounter = -1;

    void Update()
    {

        if (Input.GetButton("Shift"))
        {
            if (Input.GetButtonDown("Tab")) //USE INPUT MANAGER SO THAT U CAN ACTUALLY USE SHIFT TAB OR ELSE IT DOESNT WORK
            {
                Debug.Log("shift and tab");
                if (_usernameInput.IsActive())
                {
                    _loginScreenInputCounter--;
                    if (_loginScreenInputCounter < 0)
                    {
                        _loginScreenInputCounter = 1;
                    }
                    SelectLoginInput();
                }
                if (_localNameInput[0].IsActive())
                {

                    _localInputCounter--;
                    if (_localInputCounter < 0)
                    {
                        _localInputCounter = (GameObject.Find("GameplayManager").GetComponent<GameScript>().GetLobbySize()) - 1;
                    }
                    SelectLocalNameInput();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("just tab");
            if (_usernameInput.IsActive())
            {
                _loginScreenInputCounter++;
                if (_loginScreenInputCounter > 1)
                {
                    _loginScreenInputCounter = 0;
                }
                SelectLoginInput();
            }
            if (_localNameInput[0].IsActive())
            {
                _localInputCounter++;
                if (_localInputCounter > (GameObject.Find("GameplayManager").GetComponent<GameScript>().GetLobbySize()) - 1)
                {
                    _localInputCounter = 0;
                }
                SelectLocalNameInput();
            }
        }

        if (!_usernameInput.IsActive())
        {
            _loginScreenInputCounter = -1;
        }

    }
    void SelectLoginInput()
    {
        switch (_loginScreenInputCounter)
        {
            case 0:
                _usernameInput.Select();
                break;
            case 1:
                _passwordInput.Select();
                break;
        }
    }
    public void UsernameSelected()
    {
        _loginScreenInputCounter = 0;
    }
    public void PasswordSelected()
    {
        _loginScreenInputCounter = 1;
    }
    void SelectLocalNameInput()
    {
        switch (_localInputCounter)
        {
            case 0:
                _localNameInput[0].Select();
                break;
            case 1:
                _localNameInput[1].Select();
                break;
            case 2:
                _localNameInput[2].Select();
                break;
            case 3:
                _localNameInput[3].Select();
                break;
        }
    }
    public void LocalNameInputSelected(int m_playerNum)
    {
        _localInputCounter = m_playerNum;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingScript : MonoBehaviour
{
    [SerializeField] private Animator settingAnimator;
    public void ToggleFullScreen(){
        if(Screen.fullScreen){
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }else{
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
    }
    public void ToggleSettingsMenu(Toggle m_settingToggle){ //toggle for settings tool bar, open if toggle pressed once, close if pressed a second time
        settingAnimator.SetBool("SettingsOpen", m_settingToggle.isOn);
    }
}

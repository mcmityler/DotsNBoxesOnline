using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingScript : MonoBehaviour
{
    
    public void ToggleFullScreen(){
        if(Screen.fullScreen){
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }else{
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
    }
}

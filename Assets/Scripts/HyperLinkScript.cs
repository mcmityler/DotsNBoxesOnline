using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*

THIS SCRIPT HAS EVERYTHING THAT DEALS WITH HYPERLINKS ON THE CREDIT SCREEN
by Tyler McMillan
*/
public class HyperLinkScript : MonoBehaviour
{
    //Called when you press "highlighted text"(aka buttons) on credit screen
    public void linkedInURL(){
        Application.OpenURL("https://www.linkedin.com/in/tyler-mcmillan-580603216/");
    }
     public void gitHubURL(){
        Application.OpenURL("https://github.com/mcmityler");
    }
    public void itchioURL(){
        Application.OpenURL("https://mcmityler.itch.io/");
    }
    public void musicURL(){
        Application.OpenURL("https://www.playonloop.com/");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperLinkScript : MonoBehaviour
{
    
    public void linkedInURL(){
        Application.OpenURL("https://www.linkedin.com/in/tyler-mcmillan-580603216/");
    }
     public void gitHubURL(){
        Application.OpenURL("https://github.com/mcmityler");
    }
    public void itchioURL(){
        Application.OpenURL("https://mcmityler.itch.io/");
    }
}

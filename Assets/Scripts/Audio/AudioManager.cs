using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds; //list of sounds in game
    public static AudioManager instance; //instance of audio manager to make sure there is only one in game
    //Initialization
    void Awake()
    {
        if(instance == null){ //check only one instance
            instance = this;
        }else{
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject); //dont destroy if changing scenes

        foreach(Sound s in sounds) //add sounds to game
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }


    //FindObjectOfType<AudioManager>().Play("AUDIOCLIPNAME");
    public void Play(string name) //called from other scripts to play audio 
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name +" was not found!");
            return;
        }
        s.source.Play();
    }

    public void PlayWithPitch(string name, float pitchNum) //called from other scripts to play audio 
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        
        s.source.volume = s.volume;
        s.pitch = pitchNum;
        s.source.pitch = s.pitch;
        Debug.Log(s.pitch);
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name +" was not found!");
            return;
        }
        s.source.Play();
    }
}

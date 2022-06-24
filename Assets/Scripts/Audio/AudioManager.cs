using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.UI;
/*

THIS SCRIPT HAS EVERYTHING THAT DEALS WITH THE AUDIO
by Tyler McMillan
*/
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds; //list of sounds in game
    public static AudioManager instance; //instance of audio manager to make sure there is only one in game
    private TabScript _tabScript;
    private bool _soundMuted = false; //Should you hear sounds (for sound toggle)
    private bool _musicMuted = true; //should you hear Background music (start on true so that you can toggle false, on awake)
    //Initialization
    void Awake()
    {
        if (instance == null)
        { //check only one instance
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject); //dont destroy if changing scenes

        foreach (Sound s in sounds) //add sounds to game
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        _tabScript = FindObjectOfType<TabScript>();
        ToggleBackgroundMusic();
    }


    //FindObjectOfType<AudioManager>().Play("AUDIOCLIPNAME");
    public void Play(string name) //called from other scripts to play audio 
    {
        if (_soundMuted == false || name == "backgroundMusic") //check if the sounds are muted (through sound toggle)
            {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " was not found!");
                return;
            }
            s.source.Play();
        }
    }

    public void PlayWithPitch(string name, float pitchNum) //called from other scripts to play audio 
    {
        if (_soundMuted == false)//check if the sounds are muted (through sound toggle)
        {


            Sound s = Array.Find(sounds, sound => sound.name == name);

            s.source.volume = s.volume;
            s.pitch = pitchNum;
            s.source.pitch = s.pitch;
            Debug.Log(s.pitch);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " was not found!");
                return;
            }
            s.source.Play();
        }
    }
    public void SliderPitch(Slider m_slider) //change & play slider pitch depending on what value the slider is at
    {
        int m_sliderValue = (int)m_slider.value;
        float m_pitch = m_sliderValue * 0.25f;
        PlayWithPitch("sliderClick", m_pitch);
    }
    public void checkmarkSound(Toggle _passwordCheckbox) //Play checkmark sound (different one depending on whether its checked or not)
    {

        if (_passwordCheckbox.isOn)
        {
            Play("checkSound");
        }
        else
        {
            Play("uncheckSound");
        }
        _tabScript.PasswordVisibleToggle(_passwordCheckbox); //change textbox visibility 
    }
    public void ToggleSound()
    {
        _soundMuted = !_soundMuted;
    }
    public void ToggleBackgroundMusic(){
        _musicMuted = !_musicMuted;
        if(_musicMuted == false){
            Play("backgroundMusic");
        }else{
            Sound s = Array.Find(sounds, sound => sound.name == "backgroundMusic");
            if (s == null)
            {
                Debug.LogWarning("Sound: backgroundMusic was not found!");
                return;
            }
            s.source.Pause();
        }
    }
    public bool IsSoundMuted(){
        return _soundMuted;
    }
}

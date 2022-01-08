using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer Mixer;

    public void ToggleMusic() => MusicEnabled = !MusicEnabled;
    public void ToggleSound() => SoundEnabled = !SoundEnabled;


    public MenuButtonBehaviour MusicButton, SoundButton;


    void Start()
    {
        UpdateLabels();
    }
    
    void UpdateLabels()
    {
        MusicButton.SetState(!MusicEnabled);
        SoundButton.SetState(!SoundEnabled);
    }
    
    public bool MusicEnabled
    {
        get => PlayerPrefs.GetInt("music", 1) == 1;
        set
        {
            var previous = MusicEnabled;

            if (previous && !value)
            {
                // turn off.
                Mixer.SetVolume("MusicVolume", 0);
                
            } else if (!previous && value)
            {
                // turn on.
                Mixer.SetVolume("MusicVolume", 1);
            }
            
            PlayerPrefs.SetInt("music", value ? 1 : 0);
            UpdateLabels();
        }
    } 
    
    public bool SoundEnabled
    {
        get => PlayerPrefs.GetInt("sound", 1) == 1;
        set
        {
            var previous = SoundEnabled;

            if (previous && !value)
            {
                // turn off.
                Mixer.SetVolume("SoundVolume", 0);
                
            } else if (!previous && value)
            {
                // turn on.
                Mixer.SetVolume("SoundVolume", 1);
            }
            
            PlayerPrefs.SetInt("sound", value ? 1 : 0);
            UpdateLabels();
        }
    } 
}

public static class AudioExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mixer"></param>
    /// <param name="exposedName">The name of 'The Exposed to Script' variable</param>
    /// <param name="value">value must be between 0 and 1</param>
    public static void SetVolume(this AudioMixer mixer, string exposedName, float value)
    {
        mixer.SetFloat(exposedName, Mathf.Lerp(-80.0f, 0.0f, Mathf.Clamp01(value)));
    }
 
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mixer"></param>
    /// <param name="exposedName">The name of 'The Exposed to Script' variable</param>
    /// <returns></returns>
    public static float GetVolume(this AudioMixer mixer, string exposedName)
    {
        if (mixer.GetFloat(exposedName, out float volume))
        {
            return Mathf.InverseLerp(-80.0f, 0.0f, volume);
        }
 
        return 0f;
    }
}
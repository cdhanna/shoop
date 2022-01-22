using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer Mixer;
    public PostProcessProfile Profile;
    public Camera Camera;

    public void ToggleMusic() => MusicEnabled = !MusicEnabled;
    public void ToggleSound() => SoundEnabled = !SoundEnabled;
    public void ToggleVFX() => GoodVFXEnabled = !GoodVFXEnabled;


    public MenuButtonBehaviour MusicButton, SoundButton, VFXButton;


    void Start()
    {
        UpdateLabels();
        UpdateVfxSettings(GoodVFXEnabled);
    }
    
    void UpdateLabels()
    {
        if (MusicButton)
            MusicButton.SetState(!MusicEnabled);
        if (SoundButton)
            SoundButton.SetState(!SoundEnabled);
        if (VFXButton)
            VFXButton.SetState(!GoodVFXEnabled);
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

    public bool GoodVFXEnabled
    {
        get => PlayerPrefs.GetInt("vfx", 1) == 1;
        set
        {
            var previous = GoodVFXEnabled;
            if (previous && !value)
            {
                UpdateVfxSettings(false);
            } else if (!previous && value)
            {
                UpdateVfxSettings(true);
            }
            PlayerPrefs.SetInt("vfx", value ? 1 : 0);
            UpdateLabels();
        }
    }

    private void UpdateVfxSettings(bool enabled)
    {
        Profile.GetSetting<Bloom>().enabled.value = enabled;
        Profile.GetSetting<Vignette>().enabled.value = enabled;
        Profile.GetSetting<ChromaticAberration>().enabled.value = enabled;

        if (Camera)
        Camera.GetComponent<PostProcessVolume>().enabled = enabled;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsInit : MonoBehaviour
{
    public AudioMixer Mixer;
    public SettingsManager Settings;
    void Start()
    {
        Mixer.SetVolume("MusicVolume", Settings.MusicEnabled ? 1 : 0);
        Mixer.SetVolume("SoundVolume", Settings.SoundEnabled ? 1 : 0);

    }
}

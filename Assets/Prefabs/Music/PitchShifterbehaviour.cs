using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PitchShifterbehaviour : MonoBehaviour
{

    public AudioSource Source;
    public AudioMixerGroup Group;

    [Range(.01f, 2f)]
    public float Speed = 1;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Source.pitch = Speed;
        Group.audioMixer.SetFloat("pitchBend", 1f / Speed);
    }
}

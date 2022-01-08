using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBehaviour : MonoBehaviour
{
    public SoundManifestObject SoundManifestObject;
    public AudioSource MusicSource;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!MusicSource.isPlaying)
        {
            PlayRandom();
        }
    }

    void PlayRandom()
    {
        MusicSource.clip = SoundManifestObject.Musics.GetRandom();
        MusicSource.Play();
        MusicSource.loop = false;
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SoundManifestObject : ScriptableObject
{

    public List<AudioClip> Musics;
    
    public List<AudioClip> SelectionSounds;
    public AudioClip AcceptSound;
    public AudioClip UndoSound;
    public List<AudioClip> DeselectSounds;
    public AudioClip HintStartSound;
    public AudioClip HintAcceptSound;
    public List<AudioClip> SpendStarSounds;
    public AudioClip MenuHoverSound;
    public AudioClip MenuPushButtonSound;


    public AudioClip GetRandomDeselectSound() => DeselectSounds[Random.Range(0, DeselectSounds.Count)];

    public AudioClip ShowStarGain;
    public AudioClip LoseStar;
}


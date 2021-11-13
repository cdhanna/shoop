using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GamePieceObject : ScriptableObject
{
    public int Code;
    public Sprite Sprite;
    public Color Color = UnityEngine.Color.white;
    public float ScaleMult = 1;
}

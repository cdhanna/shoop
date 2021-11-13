using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NumberAssetObject : ScriptableObject
{
    public List<Sprite> NumberSprites;

    public Sprite GetSpriteForNumber(int movesLeft)
    {
        return NumberSprites[movesLeft];
    }
}

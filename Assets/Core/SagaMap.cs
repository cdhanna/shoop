using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SagaMap : ScriptableObject
{
    public List<SagaMapEntry> Levels;

    public GameBoardGeneratorObject ContinuationGeneratorObject;

    public GameBoardObject Generate(int levelIndex)
    {
        if (levelIndex < Levels.Count)
        {
            return Levels[levelIndex].Generate();
        }

        return ContinuationGeneratorObject.Generate(levelIndex * 37 + 7);
    }

    public GameBoardObject Generate(ISagaMapState state)
    {
        var index = state.GetLevelIndex(this);
        return Generate(index);
    }
    

}


[Serializable]
public class SagaMapEntry
{
    public string name;
    public GameBoardObject LevelObject;
    public GameBoardGeneratorObject GeneratorObject;
    public int Seed = -1;
    public bool UseGenerator;

    public GameBoardObject Generate()
    {
        return UseGenerator 
            ? GeneratorObject.Generate(Seed) 
            : LevelObject;
    }

}
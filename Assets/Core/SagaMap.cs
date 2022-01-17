using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class SagaMap : ScriptableObject
{
    public List<SagaMapEntry> Levels;

    public GameBoardGeneratorObject ContinuationGeneratorObject;

    private Dictionary<int, SagaMapEntry> indexToLevel;

    private Dictionary<int, GameBoardObject> backLog;
    [NonSerialized]
    private int pregenIndex = 0;
    [NonSerialized]
    private bool _hasInit;
    
    private void OnEnable()
    {
    }

    [ContextMenu("Display Info")]
    public void DisplayInfo()
    {
        Debug.Log("PREGEN " + pregenIndex);
    }

    void Init()
    {
        if (_hasInit) return;
        _hasInit = true;
        indexToLevel = Levels.ToDictionary(l => l.index, l => l);
        backLog = new Dictionary<int, GameBoardObject>();
        Debug.Log("ON ENABLE THE THING!!!");
    }

    public GameBoardObject Generate(int levelIndex)
    {
        Init();
        
        // if it exists in the backlog, use that.
        if (backLog.TryGetValue(levelIndex, out var preGen))
        {
            Debug.Log("Returning data via pregen");

            backLog.Remove(levelIndex);
            if (preGen != null)
            {
                return preGen;
            }
        }
        
        GameBoardGeneratorObject generator = ContinuationGeneratorObject;
        var seed = -1;

        if (indexToLevel.TryGetValue(levelIndex, out var level))
        {
            if (!level.UseGenerator)
            {
                Debug.Log("Returning data via hardcoded");
                return level.LevelObject;
            }
            seed = level.Seed;
            generator = level.GeneratorObject;
        }

        Debug.Log("Returning data via generation " + generator + " / " + seed);
        return generator.Generate(seed == -1 ? (levelIndex * 37 + 7) : seed);
    }

    public GameBoardObject Generate(ISagaMapState state)
    {
        var index = state.GetLevelIndex(this);
        pregenIndex = index + 1;
        
        
        return Generate(index);
    }

    public IEnumerator Pregenerate()
    {
        while (true)
        {
            var levelIndex = pregenIndex;

            if (backLog.Count > 10)
            {
                yield return null;
                continue;
            }

            if (backLog.TryGetValue(levelIndex, out var existing))
            {
                pregenIndex++;
                continue;
            }

            GameBoardGeneratorObject generator = ContinuationGeneratorObject;
            var seed = -1;
            if (indexToLevel.TryGetValue(levelIndex, out var level))
            {
                if (!level.UseGenerator)
                {
                    backLog.Add(levelIndex, level.LevelObject);
                    yield return null;
                    continue;
                }

                seed = level.Seed;
                generator = level.GeneratorObject;
            }

            seed = seed == -1 ? (levelIndex * 37 + 7) : seed;
            GameBoardObject board = null;
            while (board == null)
            {
                foreach (var progress in generator.GenerateRoutine(seed))
                {
                    if (progress != null)
                    {
                        board = progress;
                        backLog.Add(levelIndex, board);
                    }

                    yield return null;
                }
            }
            
            
            yield return null;
        }
        
       
    }
    

}


[Serializable]
public class SagaMapEntry
{
    [FormerlySerializedAs("name")]
    public string desc;

    public int index;
    public GameBoardObject LevelObject;
    public GameBoardGeneratorObject GeneratorObject;
    public int Seed = -1;
    public bool UseGenerator;

    public GameBoardObject Generate()
    {
        return UseGenerator 
            ? GeneratorObject.Generate(Seed == -1 ? (index * 37 + 7) : Seed) 
            : LevelObject;
    }

}
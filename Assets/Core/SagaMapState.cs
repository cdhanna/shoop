using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ISagaMapState
{
    int GetLevelIndex(SagaMap map);
    void Reset(SagaMap map);
    void NextLevel(SagaMap map);
}

public interface IStarState
{
    int Stars { get; set; }
    void ResetStars();
}


public class SagamapPlayerPrefs : ISagaMapState, IStarState
{
    public int GetLevelIndex(SagaMap map)
    {
        var key = $"sagamap_{map.name}";
        return PlayerPrefs.GetInt(key, 0);
    }

    public void Reset(SagaMap map)
    {
        var key = $"sagamap_{map.name}";
        PlayerPrefs.DeleteKey(key);
    }

    public void SaveIndex(SagaMap map, int levelIndex, int movesTaken)
    {
        
    }

    public void NextLevel(SagaMap map)
    {
        var key = $"sagamap_{map.name}";
        PlayerPrefs.SetInt(key, GetLevelIndex(map) + 1);
    }




    public int Stars
    {
        get => GetStars();
        set => SetStars(value);
    }

    public int GetStars() => PlayerPrefs.GetInt("star_count_", 4);
    public void SetStars(int stars) => PlayerPrefs.SetInt("star_count_", Mathf.Max(0, stars));
    public void ResetStars() => PlayerPrefs.DeleteKey("star_count_");

}

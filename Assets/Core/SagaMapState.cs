using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public interface ISagaMapState
{
    int GetLevelIndex(SagaMap map);
    int GetHighestLevelIndex(SagaMap map);
    int GetBestStarsForLevel(SagaMap map, int levelIndex);
    int SaveBestStarsForLevel(SagaMap map, int levelIndex, int stars);
    
    void Reset(SagaMap map);
    void PreviousLevel(SagaMap map);
    void NextLevel(SagaMap map);
}

public interface IStarState
{
    int Stars { get; set; }
    void ResetStars();
}


public class SagamapFileStorage : ISagaMapState, IStarState
{
    private readonly string _fileNameSuffix;

    [Serializable]
    public class Model
    {
        public int CurrentLevelIndex, HighestLevelIndex;
        public SerializableDictionary<int, int> StarEarnedOnLevel = new SerializableDictionary<int, int>();
    }

    [Serializable]
    public class StarModel
    {
        public int Stars;
    }

    private Dictionary<SagaMap, Model> _mapToModel = new Dictionary<SagaMap, Model>();
    private StarModel _starModel;


    public Model GetModel(SagaMap map)
    {
        if (!_mapToModel.TryGetValue(map, out var model))
        {
            var path = GetFileName(map);

            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                Debug.Log("Loading map from " + path);
                Debug.Log(text);
                model = JsonUtility.FromJson<Model>(text);
            }
            else
            {
                model = new Model();
            }
        }
        _mapToModel[map] = model;
        return model;
    }

    public void Save(SagaMap map)
    {
        var model = GetModel(map);
        var json = JsonUtility.ToJson(model);
        File.WriteAllText(GetFileName(map), json);
    }

    public StarModel GetStarModel()
    {
        var path = GetStarName();
        if (File.Exists(path))
        {
            return JsonUtility.FromJson<StarModel>(File.ReadAllText(path));
        }
        else
        {
            return new StarModel();
        }
    }

    public void SaveStarModel() => File.WriteAllText(GetStarName(), JsonUtility.ToJson(_starModel));
    public string GetFileName(SagaMap map) =>
        Path.Combine(Application.persistentDataPath, $"sagamap_{map.name}_{_fileNameSuffix}.json");
    public string GetStarName() =>
        Path.Combine(Application.persistentDataPath, $"earnings.json");

    
    public SagamapFileStorage(string fileNameSuffix = null)
    {
        _fileNameSuffix = fileNameSuffix ?? "";
        _starModel = GetStarModel();
    }
    
    public int GetLevelIndex(SagaMap map)
    {
        var model = GetModel(map);
        return model.CurrentLevelIndex;
    }

    public int GetHighestLevelIndex(SagaMap map)
    {
        var model = GetModel(map);
        return model.HighestLevelIndex;
    }

    public int GetBestStarsForLevel(SagaMap map, int levelIndex)
    {
        var model = GetModel(map);
        var stars = 0;
        model.StarEarnedOnLevel.TryGetValue(levelIndex, out stars);
        return stars;
    }

    public int SaveBestStarsForLevel(SagaMap map, int levelIndex, int stars)
    {
        var model = GetModel(map);
        var delta = stars;
        if (model.StarEarnedOnLevel.TryGetValue(levelIndex, out var existing))
        {
            stars = Math.Max(existing, stars);
            delta = Math.Max(0, stars - existing);
        }
        model.StarEarnedOnLevel[levelIndex] = stars;
        Save(map);
        return delta;

    }

    public void Reset(SagaMap map)
    {
        var model = GetModel(map);

        model.CurrentLevelIndex = 0;
        model.HighestLevelIndex = 0;
        model.StarEarnedOnLevel.Clear();
        Save(map);

    }

    public void NextLevel(SagaMap map)
    {
        var model = GetModel(map);
        model.CurrentLevelIndex++;
        model.HighestLevelIndex = Math.Max(model.CurrentLevelIndex, model.HighestLevelIndex);
        Save(map);
    }

    public void PreviousLevel(SagaMap map)
    {
        var model = GetModel(map);
        model.CurrentLevelIndex--;
        model.CurrentLevelIndex = Math.Max(model.CurrentLevelIndex, 0);
        model.HighestLevelIndex = Math.Max(model.CurrentLevelIndex, model.HighestLevelIndex);

        Save(map);
    }
    
    public int Stars
    {
        get => _starModel.Stars;
        set
        {
            _starModel.Stars = value;
            SaveStarModel();
        }
    }
    public void ResetStars()
    {
        _starModel.Stars = 0;
        SaveStarModel();
    }
}


// public class SagamapPlayerPrefs : ISagaMapState, IStarState
// {
//     public int GetLevelIndex(SagaMap map)
//     {
//         var key = $"sagamap_{map.name}";
//         return PlayerPrefs.GetInt(key, 0);
//     }
//
//     public int GetHighestLevelIndex(SagaMap map)
//     {
//         throw new System.NotImplementedException();
//     }
//
//     public int GetBestStarsForLevel(SagaMap map, int levelIndex)
//     {
//         throw new System.NotImplementedException();
//     }
//
//     public void SaveBestStarsForLevel(SagaMap map, int levelIndex, int stars)
//     {
//         throw new System.NotImplementedException();
//     }
//
//     public void Reset(SagaMap map)
//     {
//         var key = $"sagamap_{map.name}";
//         PlayerPrefs.DeleteKey(key);
//     }
//
//     public void SaveIndex(SagaMap map, int levelIndex, int movesTaken)
//     {
//         
//     }
//
//     public void NextLevel(SagaMap map)
//     {
//         var key = $"sagamap_{map.name}";
//         PlayerPrefs.SetInt(key, GetLevelIndex(map) + 1);
//     }
//
//
//
//
//     public int Stars
//     {
//         get => GetStars();
//         set => SetStars(value);
//     }
//
//     public int GetStars() => PlayerPrefs.GetInt("star_count_", 4);
//     public void SetStars(int stars) => PlayerPrefs.SetInt("star_count_", Mathf.Max(0, stars));
//     public void ResetStars() => PlayerPrefs.DeleteKey("star_count_");
//
// }

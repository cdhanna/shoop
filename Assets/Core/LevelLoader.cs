using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;

    public GameBoardObject NextBoard;

    private GameBoardGeneratorObject _generator;
    private Coroutine _backgroundGenerationRoutine;

    public List<GameBoardObject> GeneratedBacklog = new List<GameBoardObject>();
    
    public static LevelLoader GetInstance()
    {
        if (Instance != null) return Instance;
        
        var gob = new GameObject("LevelLoader");
        var inst = gob.AddComponent<LevelLoader>();
        return inst;
    }

    public static bool TryGetLevelLoader(out LevelLoader loader)
    {
        loader = Instance;
        return loader != null;
    }
    
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLevel(GameBoardObject gameBoardObject)
    {
        NextBoard = gameBoardObject;
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadRandomLevel(GameBoardGeneratorObject generatorObject)
    {
        _generator = generatorObject;
        NextBoard = generatorObject.Generate();
        StartBackgroundGeneration();
        // while (GameBoardState.FromBoard(NextBoard).IsWin)
        // {
        //     Debug.Log("Regenerating winning board...");
        //     NextBoard = generatorObject.Generate();
        // }
        SceneManager.LoadScene("SampleScene");
    }

    public IEnumerable GotoNextBackgroundGeneration()
    {
        while (GeneratedBacklog.Count == 0)
        {
            yield return null;
        }

        var next = GeneratedBacklog[0];
        GeneratedBacklog.RemoveAt(0);
        // LoadLevel(next);

        NextBoard = next;
        var op = SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Single);
        yield return op;

    }

    public void StartBackgroundGeneration(int maxCount = 10)
    {
        if (_backgroundGenerationRoutine != null) StopCoroutine(_backgroundGenerationRoutine);

        GeneratedBacklog.Clear();
        
        IEnumerator Routine()
        {

            while (true)
            {
                while (GeneratedBacklog.Count < maxCount)
                {
                    GameBoardObject board = null;
                    while (board == null)
                    {
                        foreach (var progress in _generator.GenerateRoutine())
                        {
                            if (progress != null)
                            {
                                board = progress;
                                break;
                            }
                        
                            yield return null;
                        }
                    }
                    GeneratedBacklog.Add(board);
                }

                yield return null;
            }
        }

        _backgroundGenerationRoutine = StartCoroutine(Routine());
    }

    public static void GotoMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}

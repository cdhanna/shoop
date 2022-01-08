using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BackgroundBehaviour : MonoBehaviour
{

    private static BackgroundBehaviour _instance;

    public static BackgroundBehaviour Instance => _instance;

    public List<SpriteRenderer> Clouds;

    public List<BackgroundColors> Colors;

    public int _lastColorsIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        PickColors();

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public BackgroundColors ActiveColors => Colors[_lastColorsIndex];
    

    public void PickColors(BackgroundColors colors=null)
    {
        if (colors == null)
        {
            var index = Random.Range(0, Colors.Count);
            if (index == _lastColorsIndex)
            {
                index++;
                if (index >= Colors.Count) index = 0;
            }
            colors = Colors[index];
            _lastColorsIndex = index;
        }
        foreach (var cloud in Clouds)
        {
            cloud.color = colors.CloudColors[Random.Range(0, colors.CloudColors.Length)];
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying background :(");
    }
}

[Serializable]
public class BackgroundColors
{
    [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
    public Color PrimaryUIFill;
    [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
    public Color PrimaryUIOutline;
    
    public Color[] CloudColors;
}

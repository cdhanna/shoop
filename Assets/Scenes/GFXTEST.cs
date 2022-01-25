using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GFXTEST : MonoBehaviour
{
    public TextMeshProUGUI FPSText;
    
    private float _nextFpsUpdateTime;

    // Start is called before the first frame update
    void Start()
    {
        var r = .5f;
        Screen.SetResolution((int) (Screen.width*r), (int)(Screen.height * r), true);
    }

    // Update is called once per frame
    void Update()
    {
        // if (Time.realtimeSinceStartup > _nextFpsUpdateTime)
        // {
        //     _nextFpsUpdateTime = Time.realtimeSinceStartup + 1;
        //     FPSText.text = (1f / Time.unscaledDeltaTime).ToString("00");
        // }

    }
}

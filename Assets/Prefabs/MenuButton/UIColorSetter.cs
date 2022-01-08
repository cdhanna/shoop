using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorSetter : MonoBehaviour
{

    public BackgroundBehaviour BackgroundBehaviour;
    public Image Image;
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int Color = Shader.PropertyToID("_Color");

    public Material instanceMat;

    // Start is called before the first frame update
    void Start()
    {
        if (!BackgroundBehaviour) BackgroundBehaviour = FindObjectOfType<BackgroundBehaviour>();

        instanceMat = new Material(Image.material);
        Image.material = instanceMat;
    }

    // Update is called once per frame
    void Update()
    {
        instanceMat.SetColor(Color, BackgroundBehaviour.ActiveColors.PrimaryUIFill);
        instanceMat.SetColor(OutlineColor, BackgroundBehaviour.ActiveColors.PrimaryUIOutline);
    }
}

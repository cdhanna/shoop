using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;

[ExecuteInEditMode]
public class GamePieceShaderControls : MonoBehaviour
{

    public MaterialPropertyBlock Block;
    public SpriteRenderer Renderer;

    public Texture2D NextSprite;
    
    [Range(0f, 1f)]
    public float NoisePower = .4f;

        
    [Range(0f, 1f)]
    public float TextureLerp = 0;

    
    public Color BorderColor;

    public bool ControlBorderSDF;

    [Range(0, 1f)]
    public float BorderThickness = 0f;
    
    [Range(0, 1f)]
    public float BorderInnerSmooth = 0f;


    public bool ControlSDFProperties = false;

    [Range(0, 1)]
    public float SDFThreshold;
    [Range(0, 1)]
    public float Smoothness;
    

    private Coroutine _routine, _colorRoutine, _sdfRoutine;

    private float _startSDFThresold, _startSmoothness;
    public float StartSDFThreshold => _startSDFThresold;
    public float StartSDFSmoothness => _startSmoothness;
    public Color StartBorderColor { get; private set; }
    public Color StartColor { get; private set; }
    
    
    // Start is called before the first frame update
    void Start()
    {
        _startSDFThresold = SDFThreshold;
        _startSmoothness = Smoothness;
        StartBorderColor = BorderColor;
        StartColor = Renderer.color;
    }

    // Update is called once per frame
    void Update()
    {

        if (Renderer == null) return;
        if (Block == null)
        {
            Block = new MaterialPropertyBlock();
        }
        
        Block.SetTexture("_MainTex", Renderer.sprite.texture);
        if (NextSprite != null)
            Block.SetTexture("_Main2Tex", NextSprite);
        Block.SetFloat("_TexLerp", TextureLerp);
        Block.SetColor("_OutlineColor", BorderColor);

        if (ControlBorderSDF)
        {
            Block.SetFloat("_OutlineThickness", BorderThickness);
            Block.SetFloat("_OutlineInnerSmooth", BorderInnerSmooth);
        }

        Block.SetFloat("_NoisePower", NoisePower);

        if (ControlSDFProperties)
        {
            Block.SetFloat("_SDFThreshold", SDFThreshold);
            Block.SetFloat("_Smoothness", Smoothness);
        }
        
        Renderer.SetPropertyBlock(Block);
    }

    public void SetSDFProperties(float threshold, float smoothness, float transitionTime = .3f, Action<float> cb=null)
    {
        if (_sdfRoutine != null)
            StopCoroutine(_sdfRoutine);
        
        IEnumerator Routine()
        {
            var now = Time.realtimeSinceStartup;
            var endTime = now + transitionTime;

            var startSDF = SDFThreshold;
            var startSmooth = Smoothness;

            while (Time.realtimeSinceStartup < endTime)
            {
                var t = Time.realtimeSinceStartup;
                var r = 1 - ((endTime - t) / transitionTime);

                SDFThreshold = Mathf.Lerp(startSDF, threshold, r);
                Smoothness = Mathf.Lerp(startSmooth, smoothness, r);
                cb?.Invoke(r);

                yield return null;
            }

            Smoothness = smoothness;
            SDFThreshold = threshold;
            cb?.Invoke(1);
        }

        _sdfRoutine = StartCoroutine(Routine());
    }

    public void SetTexture(Sprite sprite, float transitionTime = .3f)
    {
        if (_routine != null)
            StopCoroutine(_routine);
        
        IEnumerator Routine()
        {
            NextSprite = sprite.texture;
            TextureLerp = 0;

            var now = Time.realtimeSinceStartup;
            var endTime = now + transitionTime;

            while (Time.realtimeSinceStartup < endTime)
            {
                var t = Time.realtimeSinceStartup;
                var r = 1 - ((endTime - t) / transitionTime);
                TextureLerp = r;
                yield return null;
            }

            Renderer.sprite = sprite;
            TextureLerp = 0;
        }

        _routine = StartCoroutine(Routine());
    }


    public void SetColors(Color fillColor, Color borderColor, float transitionTime = .3f, float delay=0f)
    {
        if (_colorRoutine != null) StopCoroutine(_colorRoutine);

        IEnumerator Routine()
        {
            yield return new WaitForSecondsRealtime(delay);
            var now = Time.realtimeSinceStartup;
            var endTime = now + transitionTime;

            var startFillColor = Renderer.color;
            var startBorderColor = BorderColor;

            while (Time.realtimeSinceStartup < endTime)
            {
                var t = Time.realtimeSinceStartup;
                var r = 1 - ((endTime - t) / transitionTime);

                Renderer.color = Color.Lerp(startFillColor, fillColor, r);
                BorderColor = Color.Lerp(startBorderColor, borderColor, r);
                yield return null;

            }

            Renderer.color = fillColor;
            BorderColor = borderColor;
        }

        _colorRoutine = StartCoroutine(Routine());
    }
}

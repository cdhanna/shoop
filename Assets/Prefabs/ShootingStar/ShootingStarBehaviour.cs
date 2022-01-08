using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShootingStarBehaviour : MonoBehaviour
{
    public Camera Cam;


    private bool _first;
    private BorderSide _lastSide;
    private TweenerCore<Vector3, Vector3, VectorOptions> _tween;
    public TrailRenderer Renderer;

    private float RenderAt = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (!Cam) Cam = FindObjectOfType<Camera>();

        Renderer.emitting = false;
        _lastSide = GetRandomSide();
        var pos = GetPosition(_lastSide);
        transform.position = Cam.transform.position + (pos - Cam.transform.position) * Random.Range(.9f, 5f);
        Reposition();
        RenderAt = Time.realtimeSinceStartup + 1;

    }

    void Reposition()
    {
        if (!this) return;
        

        if (!Cam) Cam = FindObjectOfType<Camera>();
        var side = GetRandomSide();
        while (side == _lastSide || !_first)
        {
            side = GetRandomSide();
            _first = true;
        }

        _lastSide = side;
        
        var startPosition = transform.position;
        var endPosition = GetPosition(side);

        var actualEnd = startPosition + (endPosition - startPosition) * Random.Range(1.1f, 1.7f);

        var x = (startPosition - actualEnd).magnitude / 5;
        // transform.position = startPosition;
        _tween = transform.DOMove(actualEnd, x * Random.Range(.3f, 1)).SetEase(Ease.Linear).OnComplete(Reposition);
    }

    private void OnDestroy()
    {
        _tween.Kill();
    }

    // Update is called once per frame
    void Update()
    {
        if (RenderAt < Time.realtimeSinceStartup)
        {
            Renderer.emitting = true;
        }
    }

    BorderSide GetRandomSide()
    {
        return new[] {BorderSide.LOW, BorderSide.TOP, BorderSide.LEFT, BorderSide.RIGHT}[Random.Range(0, 4)];
    }

    Vector3 GetPosition(BorderSide side)
    {
        float OrthoWidth = Cam.orthographicSize * Cam.aspect;
        float OrthoHeight = Cam.orthographicSize;
        
        
        
        switch (side)
        {
            case BorderSide.LEFT:
                return new Vector3 (Cam.transform.position.x - OrthoWidth, Random.Range(Cam.transform.position.y - OrthoHeight, Cam.transform.position.y + OrthoHeight), 0.0F);
                break;
            
            case BorderSide.RIGHT:
                return new Vector3 (Cam.transform.position.x + OrthoWidth, Random.Range(Cam.transform.position.y - OrthoHeight, Cam.transform.position.y + OrthoHeight), 0.0F);
                break;
            
            case BorderSide.TOP:
                return new Vector3 (Random.Range(Cam.transform.position.x - OrthoWidth, Cam.transform.position.x + OrthoWidth), Cam.transform.position.y + OrthoHeight, 0.0F);
                break;
            
            default:
            case BorderSide.LOW:
                return new Vector3 (Random.Range(Cam.transform.position.x - OrthoWidth, Cam.transform.position.x + OrthoWidth), Cam.transform.position.y - OrthoHeight, 0.0F);
                break;
        }
    }
}

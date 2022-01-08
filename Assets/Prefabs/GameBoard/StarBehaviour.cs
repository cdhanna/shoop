using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarBehaviour : MonoBehaviour
{

    public GamePieceShaderControls Controls;

    public Transform TempTarget;
    public Transform WinTarget;
    public Transform TopTarget;
    public Transform Temp2Target;

    public Color TempColor, TempBorder;


    public Color WinBorderColor, WinFillColor;
    public bool _isVanished;
    private Coroutine _vanishRoutine;
    private Vector3 _startScale;
    private Color _startFill, _startBorder;
    private float _startSDF, _startSmooth;

    private Coroutine _scootRoutine;

    private bool _isAtTemp = false;
    private bool _isAtTemp2 = false;
    private bool _isAtTop = false;
    private bool _isAtScore = false;

    private bool _isInit;

    public bool _isWin;

    public SoundManifestObject SoundManifestObject;
    public AudioSource DialSource;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_isInit) return;

        _isInit = true;
        _startFill = Controls.Renderer.color;
        _startBorder = Controls.BorderColor;
        _startScale = new Vector3(4, 4, 1);
        _startSDF = Controls.SDFThreshold;
        _startSmooth = Controls.Smoothness;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScootTowardsTemp(float duration = .3f)
    {
        if (_isAtTemp) return;
        _isAtTemp = true;
        _isAtTemp2 = false;
        _isAtScore = false;
        _isAtTop = false;
        ScootToPosition(TempTarget, duration);
        Controls.SetColors(TempColor, TempBorder, duration);
    }
    public void ScootTowardsTemp2(float duration = .3f)
    {
        if (_isAtTemp2) return;
        _isAtTemp = false;
        _isAtTemp2 = true;
        _isAtScore = false;
        _isAtTop = false;
        ScootToPosition(Temp2Target, duration);
        Controls.SetColors(TempColor, TempBorder, duration);
    }
    public void ScootTowardsTop(float duration = .3f)
    {
        if (_isAtTop) return;
        _isAtTemp = false;
        _isAtTemp2 = false;
        _isAtScore = false;
        _isAtTop = true;
        ScootToPosition(TopTarget, duration);
        Controls.SetColors(_startFill, _startBorder, duration);

    }
    public void ScootTowardsScore(float duration = .3f, float colorDelay=.3f, bool isWin=true)
    {
        if (_isAtScore) return;
        _isAtTemp = false;
        _isAtTemp2 = false;
        _isAtScore = true;
        _isAtTop = false;
        _isWin = isWin;
        if (isWin)
        {
            Controls.SetColors(isWin ? WinFillColor : new Color(0, 0, 0, .3f), WinBorderColor, duration, colorDelay);

            IEnumerator PlaySound()
            {
                yield return new WaitForSecondsRealtime(colorDelay);
                DialSource.PlayOneShot(SoundManifestObject.ShowStarGain);

            }
            StartCoroutine(PlaySound());
        }

        ScootToPosition(WinTarget, duration);
    }

    public void ScootToPosition(Transform target, float duration = .3f)
    {
        if (_scootRoutine != null)
        {
            StopCoroutine(_scootRoutine);
        }
     
        Start();
        var startTime = Time.realtimeSinceStartup;
        var endTime = startTime + duration;
        
        Vector3 velRef = Vector3.zero;
        var startScale = transform.localScale;
        var targetScale = target.localScale; //new Vector3(startScale.x * target.localScale.x, startScale.y * target.localScale.y, 1f);

        IEnumerator Routine()
        {
            while (Time.realtimeSinceStartup < endTime)
            {
                var r = (Time.realtimeSinceStartup - startTime) / (endTime - startTime);

                transform.localPosition =
                    Vector3.SmoothDamp(transform.localPosition, target.localPosition, ref velRef, .1f);

                // angle = Mathf.SmoothDamp(angle, 360 * Mathf.Sign(target.eulerAngles.z), ref angleRef, .1f);
                // transform.eulerAngles = new Vector3(0, 0, angle);
                transform.localScale = Vector3.Lerp(startScale, targetScale, r);
                yield return null;
            }

        }
        transform.eulerAngles = new Vector3(0, 0, 0);

        _scootRoutine = StartCoroutine(Routine());
    }

    private Vector3 _vanishedScale;
    public void Vanish()
    {
        if (_isVanished) return;
        _isVanished = true;
        
        
        
        if (_vanishRoutine != null) StopCoroutine(_vanishRoutine);
        
        var startTime = Time.realtimeSinceStartup;
        var endTime = startTime + .3f;
        _vanishedScale = transform.localScale;
        IEnumerator Routine()
        {
            while (Time.realtimeSinceStartup < endTime)
            {
                var r = (Time.realtimeSinceStartup - startTime) / (endTime - startTime);

                transform.localScale = Vector3.Lerp(_startScale, _startScale + Vector3.one * 3, r);
                yield return null;
            }
            yield return new WaitForSecondsRealtime(.1f);
            DialSource.PlayOneShot(SoundManifestObject.LoseStar, .7f);
        }
        Controls.SetSDFProperties(0, 1, .6f);

        _vanishRoutine = StartCoroutine(Routine());
    }

    public void Appear()
    {
        if (!_isVanished) return;
        _isVanished = false;
        
        
        
        if (_vanishRoutine != null) StopCoroutine(_vanishRoutine);
        var startTime = Time.realtimeSinceStartup;
        var endTime = startTime + .1f;
        var startScale = transform.localScale;
        IEnumerator Routine()
        {
            while (Time.realtimeSinceStartup < endTime)
            {
                var r = (Time.realtimeSinceStartup - startTime) / (endTime - startTime);

                transform.localScale = Vector3.Lerp(startScale, _vanishedScale, r);
                yield return null;
            }
            
            yield return new WaitForSecondsRealtime(.1f);
            DialSource.PlayOneShot(SoundManifestObject.ShowStarGain);

        }
        Controls.SetSDFProperties(_startSDF, _startSmooth, .3f);
        

        
        _vanishRoutine = StartCoroutine(Routine());
    }
}

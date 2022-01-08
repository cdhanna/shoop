using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionHelperBehaviour : MonoBehaviour
{
    private static TransitionHelperBehaviour _instance;

    public static TransitionHelperBehaviour Instance => _instance;

    public float Left, Right;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);


        var cam = FindObjectOfType<Camera>();
        Right = cam.orthographicSize * cam.aspect * Right * 2;
        Left = cam.orthographicSize * cam.aspect * Left * 2;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TransitionAndDoThing(Action thing)
    {
        IEnumerable Func()
        {
            thing();
            yield break;
        }
        TransitionAndDoThing( Func() );
    }

    public void TransitionAndDoThing(IEnumerable thing)
    {
        IEnumerator LoadNext()
        {
            // disable the virtual camera
            
            // animate to the right-

            foreach (var _ in Routine_Enter(.4f))
            {
                yield return _;
            }

            var expectedDelay = .2f;

            BackgroundBehaviour.Instance.PickColors();

            var now = Time.realtimeSinceStartup;
            foreach (var _ in thing)
            {
                yield return _;
            }

            var actualDelay = Time.realtimeSinceStartup - now;
            var forcedDelay = Mathf.Max(0, expectedDelay - actualDelay);

            yield return new WaitForSeconds(forcedDelay);
            
            foreach (var _ in Routine_Exit(.4f))
            {
                yield return _;
            }
            
            // fade in stuff
        }
        StartCoroutine(LoadNext());
    }
    

    public void StartNextTransition()
    {
         TransitionAndDoThing(LevelLoader.GetInstance().GotoNextLevel());
        // IEnumerator LoadNext()
        // {
        //     // disable the virtual camera
        //     
        //     // animate to the right-
        //
        //     foreach (var _ in Routine_Enter(1f))
        //     {
        //         yield return _;
        //     }
        //
        //     var expectedDelay = .2f;
        //
        //     var now = Time.realtimeSinceStartup;
        //     foreach (var _ in LevelLoader.GetInstance().GotoNextLevel())
        //     {
        //         yield return _;
        //     }
        //
        //     var actualDelay = Time.realtimeSinceStartup - now;
        //     var forcedDelay = Mathf.Max(0, expectedDelay - actualDelay);
        //     yield return new WaitForSeconds(forcedDelay);
        //     BackgroundBehaviour.Instance.PickColors();
        //     
        //     foreach (var _ in Routine_Exit(.5f))
        //     {
        //         yield return _;
        //     }
        //     
        //     // fade in stuff
        // }
        // StartCoroutine(LoadNext());

    }

    public IEnumerable Routine_Enter(float duration)
    {
        var startTime = Time.realtimeSinceStartup;
        var endTIme = startTime + duration;
        transform.localPosition = Vector3.right * Right;

        while (Time.realtimeSinceStartup < endTIme)
        {
            var r = (Time.realtimeSinceStartup - startTime) / (duration);

            transform.localPosition = Vector3.Lerp( Vector3.right * Right, Vector3.zero, r);
            yield return null;
        }
        transform.localPosition = Vector3.zero;
        
    }
    
    public IEnumerable Routine_Exit(float duration)
    {
        var startTime = Time.realtimeSinceStartup;
        var endTIme = startTime + duration;
        transform.localPosition = Vector3.zero;

        while (Time.realtimeSinceStartup < endTIme)
        {
            var r = (Time.realtimeSinceStartup - startTime) / (duration);

            transform.localPosition = Vector3.Lerp( Vector3.zero, Vector3.left * Left, r);
            yield return null;
        }
        transform.localPosition = Vector3.left * Left;
        
    }

    private void OnDestroy()
    {
        Debug.Log("Destroying background :(");
    }
}

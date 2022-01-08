using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvasController : MonoBehaviour
{
    public CanvasGroup CanvasGroup;

    public Button Hint, Next, Undo;
    
    // Start is called before the first frame update
    void Start()
    {
        CanvasGroup.alpha = 0;
        CanvasGroup.DOFade(1, .2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeOut()
    {
        CanvasGroup.DOFade(0, .6f);

    }
}

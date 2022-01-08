using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MenuTransitionBehaviour : MonoBehaviour
{
    private RectTransform rect;

    public Vector3 homePosition;
    public Vector3 homeSize;
    
    public Vector3 settingsPosition;
    public Vector3 settingsSize;
    
    public RectTransform settingsAnchor;
    public CanvasGroup CanvasGroup;

    public float SettingsAlpha = 1, HomeAlpha = 1;

    public bool DisableSettings, DisableHome;
    
    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();

        homePosition = transform.localPosition;

        if (CanvasGroup)
        {
            CanvasGroup.alpha = HomeAlpha;
        }


        GotoHome();
        if (DisableHome)
        {
            gameObject.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GotoSettings()
    {
        gameObject.SetActive(true);
        rect.DOLocalMove(settingsAnchor.localPosition, .3f);
        CanvasGroup?.DOFade(SettingsAlpha, .6f).OnComplete(() =>
        {
            if (DisableSettings)
            gameObject.SetActive(false);
        });
    }

    public void GotoHome()
    {
        gameObject.SetActive(true);
        rect.DOLocalMove(homePosition, .3f);
        CanvasGroup?.DOFade(HomeAlpha, .4f).OnComplete(() =>
        {
            if (DisableHome)
            {
                gameObject.SetActive(false);
            }
        });

    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NextButtonBehaviour : MonoBehaviour
{
    public Button NextButton;
    public GameBoardBehaviour GameBoardBehaviour;

    public Vector3 InactivePosition;

    private Vector3 _activePosition, _inactivePosition;

    private float _colorLerp;
    private float _colorVel;
    private Vector3 _vel;
    private Color _activeColor, _inactiveColor;

    private bool _animating;
    
    // Start is called before the first frame update
    void Start()
    {
        NextButton.onClick.AddListener(OnClick);
        _activeColor = NextButton.targetGraphic.color;
        _inactiveColor = new Color(0, 0, 0, 0);

        _activePosition = transform.localPosition;
        _inactivePosition = _activePosition + InactivePosition;
        
        
        NextButton.onClick.AddListener(() =>
        {
            NextButton.targetGraphic.transform.DOPunchScale(Vector3.one *.2f, .1f);
        });
    }

    // Update is called once per frame
    void Update()
    {
        // is valid?
        var isValid = GameBoardBehaviour.IsWin && !GameBoardBehaviour.IsOver;

        NextButton.interactable = isValid & !_animating;
        
        var targetPosition = isValid
            ? _activePosition
            : _inactivePosition;

        var targetColor = isValid
            ? 1
            : 0;

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref _vel, .3f);

        _colorLerp = Mathf.SmoothDamp(_colorLerp, targetColor, ref _colorVel, .3f);
        NextButton.targetGraphic.color = Color.Lerp(_inactiveColor, _activeColor, _colorLerp);

    }

    public void OnClick()
    {
        // 1 thing happens
        var transition = TransitionHelperBehaviour.Instance;
        
        // and another thing happens

        GameBoardBehaviour.IsOver = true;

        
        var obj = GameObject.FindObjectOfType<StarCounterBehaviour>();
        if (obj && !GameBoardBehaviour.Flags.DisableStars)
        {
            this.DoRoutine(obj.TakeNewStars(), () =>
            {
                transition.StartNextTransition();
            });
        }
        else
        {
            transition.StartNextTransition();
        }

    }
}

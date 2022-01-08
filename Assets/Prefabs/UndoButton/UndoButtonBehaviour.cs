using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UndoButtonBehaviour : MonoBehaviour
{
    public Button UndoButton;
    public GameBoardBehaviour GameBoardBehaviour;

    public Image Graphic;
    
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
        UndoButton.onClick.AddListener(OnClick);
        _activeColor =Graphic.color;
        _inactiveColor = new Color(0, 0, 0, 0);

        _activePosition = transform.localPosition;
        _inactivePosition = _activePosition + InactivePosition;
        
        
        UndoButton.onClick.AddListener(() =>
        {
            Graphic.transform.DOPunchScale(Vector3.one *.2f, .1f);
        });
    }

    // Update is called once per frame
    void Update()
    {
        // is valid?
        var isValid = GameBoardBehaviour.Swaps.Count > 0 && !GameBoardBehaviour.HasHint && !GameBoardBehaviour.IsOver;

        isValid &= !GameBoardBehaviour.Flags.DisableUndo;
        isValid &= !GameBoardBehaviour.HasHint;
        
        UndoButton.interactable = isValid & !_animating;
        
        var targetPosition = isValid
            ? _activePosition
            : _inactivePosition;

        var targetColor = isValid
            ? 1
            : 0;

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref _vel, .3f);

        _colorLerp = Mathf.SmoothDamp(_colorLerp, targetColor, ref _colorVel, .3f);
        Graphic.color = Color.Lerp(_inactiveColor, _activeColor, _colorLerp);

    }

    public void OnClick()
    {
        
        GameBoardBehaviour.Undo();

        // rotate backwards 360 degrees...

        IEnumerator RotateBackwards()
        {
            //_animating = true;

            var vel = 0f;
            var startRotation = Graphic.transform.localRotation.eulerAngles.z;
            var arg = startRotation;

            while (arg < startRotation+359f)
            {
                arg = Mathf.SmoothDamp(arg, startRotation+360f, ref vel, .2f);

                var rotation = arg;
                Graphic.transform.localRotation = Quaternion.Euler(0, 0, rotation);
                yield return null;
            }
            
            Graphic.transform.localRotation = Quaternion.Euler(0, 0, startRotation);
            _animating = false;


        }

        StartCoroutine(RotateBackwards());

    }
}

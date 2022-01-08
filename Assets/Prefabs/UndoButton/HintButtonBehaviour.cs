using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HintButtonBehaviour : MonoBehaviour
{
    public Button Button;

    public GameBoardBehaviour GameBoardBehaviour;

    public StarStateProvider StarStateProvider;
    public Vector3 InactivePosition;

    private Vector3 _activePosition, _inactivePosition;

    private float _colorLerp;
    private float _colorVel;
    private Vector3 _vel;
    private Color _activeColor, _inactiveColor;

    private bool _animating;
    public Image Graphic;


    // Start is called before the first frame update
    void Start()
    {
        _activeColor =Graphic.color;
        _inactiveColor = new Color(0, 0, 0, 0);

        _activePosition = transform.localPosition;
        _inactivePosition = _activePosition + InactivePosition;
        
        Button.onClick.AddListener(() =>
        {
            Graphic.transform.DOPunchScale(Vector3.one *.2f, .1f);
        });
    }

    // Update is called once per frame
    void Update()
    {
        var hasStars = StarStateProvider.GetState().Stars >= 3 && !GameBoardBehaviour.IsWin && !GameBoardBehaviour.IsOver;
        var isValid = hasStars && !GameBoardBehaviour.HasHint;
        isValid &= !GameBoardBehaviour.Flags.DisableHInts;

        Button.interactable = isValid & !_animating;
        
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
}

using System.Collections;
using System.Collections.Generic;
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
    }

    // Update is called once per frame
    void Update()
    {
        // is valid?
        var isValid = GameBoardBehaviour.IsWin;

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

        Debug.Log("NExt up!!");


        var transition = TransitionHelperBehaviour.Instance;
        transition.StartNextTransition();
    }
}

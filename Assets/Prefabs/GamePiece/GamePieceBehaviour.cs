using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GamePieceBehaviour : MonoBehaviour
{

    public SpriteRenderer Renderer;
    public SpriteRenderer BackgroundRenderer;
    public GamePieceShaderControls ShaderControls;
    public GamePieceShaderControls SelectionCloudControls;

    public GameBoardBehaviour GameBoardBehaviour;
    public GamePieceObject PieceObject;
    public GameBoardObject BoardObject;
    public Vector2Int Location;

    public Color SelectedColor, DeselectedColor, DeselectedFill;


    private static Color _transparent = new Color(0,0,0,0);
    public CinemachineTargetGroup TargetGroup;

    public AudioSource SelectionAudioSource;
    public SoundManifestObject SoundManifestObject;
    
    private Vector3 scaleVel;
    private Vector3 startScale;
    public float ScaleBoostOnSelected = 1.2f;
    private bool _isSelected;

    public GamePieceShaderControls LockedShaderControls;
    public bool IsLocked;
    
    public event Action OnMouseClicked, OnMouseEntered, OnMouseExited, OnMouseReleased;
    
    // Start is called before the first frame update
    void Start()
    {
        startScale = transform.localScale;
        
        // select a random sound for the selection
        SelectionAudioSource.clip =
            SoundManifestObject.SelectionSounds[Random.Range(0, SoundManifestObject.SelectionSounds.Count)];


        if (PieceObject)
        {
            Set(PieceObject);
        }
    }

    private Coroutine scaleRoutine;

    // Update is called once per frame
    void Update()
    {
        // if (GameBoardBehaviour == null || PieceObject == null) return;
        //
        // var x = Mathf.SmoothStep(1f, ScaleBoostOnSelected, 
        //     Mathf.Pow(GameBoardBehaviour.HoverStack.Count / (float)GameBoardBehaviour.PieceBehaviours.Count, .5f));
        // x = .5f; // TODO: FIX THIS.
        // var largeScale = startScale * x;
        //
        // var targetScale = _isSelected ? largeScale : startScale;
        // transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVel, .2f);
    }

    IEnumerator SetScale(Vector3 start, Vector3 end, float duration)
    {
        if (end.magnitude < .01f) yield break;
        
        transform.localScale = start;
        do
        {
            transform.localScale = Vector3.SmoothDamp(transform.localScale, end, ref scaleVel, duration);
            yield return null;
        } while ((transform.localScale - end).magnitude > .03f);

        transform.localScale = end;

    }

    public void Set(GameBoardObject board)
    {
        BoardObject = board;
    }
    
    public void Set(GameBoardSlot slot)
    {
        Location = slot.Location;

        IsLocked = slot.IsLocked;
        if (IsLocked)
        {
            LockedShaderControls.SetSDFProperties(.5f, .025f, .3f);
        }
        else
        {
            LockedShaderControls.Renderer.color = new Color(0, 0, 0, 0);
        }
        
        Set(slot.PieceObject);
        DeselectInStack();
    }

    public void Set(GamePieceObject pieceObject)
    {
        if (PieceObject != null)
        {
            Renderer.transform.localScale /= PieceObject.ScaleMult;
        }
        PieceObject = pieceObject;
        Renderer.color = PieceObject.Color;
        Renderer.transform.localScale *= PieceObject.ScaleMult;


        DeselectInStack();
        ShaderControls.SetTexture(PieceObject.Sprite, .2f);
    }

    private void OnMouseDown()
    {
        OnMouseClicked?.Invoke();
    }

    private void OnMouseEnter()
    {
        OnMouseEntered?.Invoke();
    }

    private void OnMouseExit()
    {
        OnMouseExited?.Invoke();
    }

    private void OnMouseUp()
    {
        OnMouseReleased?.Invoke();
    }

    public void SelectInStack()
    {
        _isSelected = true;
        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
            
        };

        float speedModify = 1;
        #if UNITY_ANDROID
        speedModify = 1f;
        #endif
        
        scaleRoutine = StartCoroutine(SetScale(startScale, startScale * 1.1f, .2f * speedModify));
        // BackgroundRenderer.color = PieceObject.Color;
        // ShaderControls.BorderColor = SelectedColor;
        var selectionColor = GetSelectionColor();

        SelectionCloudControls.SetColors(PieceObject.Color, DeselectedColor, .3f * speedModify);

        var brighter = selectionColor;
        brighter = Color.Lerp(brighter, Color.white, .5f);
        
        
        if (IsLocked)
        {
            LockedShaderControls.SetColors(Color.black, Color.Lerp(GetSelectionColor(.8f), Color.white, .8f));
        }

        
        ShaderControls.SetColors(selectionColor, brighter, .3f * speedModify);
        SetTargetGroupWeight(1 + .07f * Mathf.Pow(GameBoardBehaviour.HoverStack.Count, .2f));

        SelectionAudioSource.Play();
    }

    private Color GetSelectionColor(float mix = .5f)
    {
        var selectionColor = PieceObject.Color;
        if (IsLocked)
        {
            var grey = selectionColor.r + selectionColor.g + selectionColor.b;
            grey /= 3;

            var greyColor = new Color(grey, grey, grey, selectionColor.a);
            selectionColor = Color.Lerp(selectionColor, greyColor, mix);

            //selectionColor = ;
        }
        return selectionColor;
    }

    public void DeselectInStack()
    {
        _isSelected = false;
        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
            
        };
        var selectionColor = GetSelectionColor();

        
        if (IsLocked)
        {
            LockedShaderControls.SetColors(Color.grey, GetSelectionColor());
        }
        else
        {
            LockedShaderControls.SetColors(_transparent, _transparent);
        }
        
        scaleRoutine = StartCoroutine(SetScale( startScale * 1.1f, startScale, .2f));
        // BackgroundRenderer.color = DeselectedColor;
        SelectionCloudControls.SetColors(DeselectedColor, DeselectedColor);
        ShaderControls.SetColors(DeselectedFill, selectionColor);
        SetTargetGroupWeight(1f);
        // ShaderControls.BorderColor = Renderer.color;

    }

    public void SetTargetGroupWeight(float weight)
    {
        if (!TargetGroup) return;
        TargetGroup.m_Targets[TargetGroup.FindMember(transform)].weight = weight;
    }
}

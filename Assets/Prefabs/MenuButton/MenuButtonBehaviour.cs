using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Button Button;
    public TextMeshProUGUI Text;
    public Image Image;

    public Color DefaultFaceColor;
    public Color HoverFaceColor;

    public bool OverrideNoisePower;
    public float NoisePower = .6f;

    public BackgroundBehaviour BackgroundBehaviour;

    public Material _instanceMat;
    public Sprite ImageLerpSprite;
    
    private int FaceColor = Shader.PropertyToID("_FaceColor");
    private int GlowColor = Shader.PropertyToID("_GlowColor");
    private int UnderlayColor = Shader.PropertyToID("_UnderlayColor");
    private int NoisePowerParam = Shader.PropertyToID("_NoisePower");
    private int TexLerp = Shader.PropertyToID("_Main2Tex");
    private int TexLerpAmount = Shader.PropertyToID("_TexLerp");

    private Color _faceColor;
    private float _texLerp;

    public SoundManifestObject SoundManifestObject;
    public AudioSource DialSource;
    public bool Pointable = true;
    
    // Start is called before the first frame update
    void Start()
    {
         BackgroundBehaviour = BackgroundBehaviour.Instance;

         if (Image)
         {
             _instanceMat = new Material(Image.material);
             Image.material = _instanceMat;
             FaceColor = Shader.PropertyToID("_Color");
             GlowColor = Shader.PropertyToID("_OutlineColor");
             UnderlayColor = Shader.PropertyToID("_UnderlayColor");
             TexLerp = Shader.PropertyToID("_Main2Tex");
             NoisePowerParam = Shader.PropertyToID("_NoisePower");
         }
         else
         {
             _instanceMat = new Material(Text.fontSharedMaterial);
             Text.fontSharedMaterial = _instanceMat;
             FaceColor = Shader.PropertyToID("_FaceColor");
             GlowColor = Shader.PropertyToID("_GlowColor");
             UnderlayColor = Shader.PropertyToID("_UnderlayColor");
             NoisePowerParam = Shader.PropertyToID("_NoisePower");
         }


         _faceColor = DefaultFaceColor;

        if (Button)
        {
            Button.onClick.AddListener(() =>
            {
                DialSource.PlayOneShot(SoundManifestObject.MenuPushButtonSound);
                transform.DOPunchScale(Vector3.one * .1f, .2f);
            });
        }
    }

    public void SetState(bool state)
    {
        if (state)
        {
            DOTween.To(() => _texLerp, c => _texLerp = c, 1, .1f);

        }
        else
        {
            DOTween.To(() => _texLerp, c => _texLerp = c, 0, .1f);

        }
    }

    // Update is called once per frame
    void Update()
    {
        BackgroundBehaviour = BackgroundBehaviour.Instance;

        _instanceMat.SetColor(UnderlayColor, BackgroundBehaviour.ActiveColors.PrimaryUIFill);
        _instanceMat.SetColor(GlowColor, BackgroundBehaviour.ActiveColors.PrimaryUIOutline);

        _instanceMat.SetColor(FaceColor, _faceColor);

        if (OverrideNoisePower)
        {
            _instanceMat.SetFloat(NoisePowerParam, NoisePower);
        }

        if (ImageLerpSprite)
        {
            _instanceMat.SetTexture(TexLerp, ImageLerpSprite.texture);
            _instanceMat.SetFloat(TexLerpAmount, _texLerp);
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Pointable) return;
        if (Button)
        {
            DialSource.PlayOneShot(SoundManifestObject.MenuHoverSound, .1f);

        }
        DOTween.To(() => _faceColor, c => _faceColor = c, HoverFaceColor, .1f);

        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Pointable) return;

        DOTween.To(() => _faceColor, c => _faceColor = c, DefaultFaceColor, .1f);

        // _instanceMat.SetColor(FaceColor, DefaultFaceColor);

    }
}

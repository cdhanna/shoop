using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MultiDigitBehaviour : MonoBehaviour
{

    public NumberAssetObject NumbersObject;

    public Color OutlineColor; 
    public Color FillColor; 
    
    public GamePieceShaderControls ControlsPrefab;
    
    private List<GamePieceShaderControls> _existingDigits = new List<GamePieceShaderControls>();

    public float Spread = .4f;
    public float TransitionTime = .3f;
    public TextAlignment Alignment;
    
    
    public int Number;
    
    
    
    private int _lastNumber;
    
    // Start is called before the first frame update
    void Start()
    {
        SetNumber(Number);
    }

    // Update is called once per frame
    void Update()
    {
        if (Number != _lastNumber)
        {
            _lastNumber = Number;
            SetNumber(Number);
        }
    }

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        SetNumber(Number);
    }

    public void SetNumber(int number)
    {
        _lastNumber = number;
        Number = number;
        // separate out into digits.
        var str = number.ToString();

        Vector3 GetPosition(int i)
        {
            var totalSize = Spread * str.Length;
            var leftPos = new Vector3(i * Spread, 0, 0);
            switch (Alignment)
            {
                case TextAlignment.Left:
                    return leftPos;
                
                case TextAlignment.Center:
                    return leftPos + (Vector3.left * totalSize * .5f);
                
                default:
                case TextAlignment.Right:
                    return new Vector3( (str.Length - i) * -Spread, 0, 0);
                    
            }
        }
        
        for (var i = 0; i < str.Length; i++)
        // for (var i = str.Length - 1; i >= 0; i--)
        {
            var digit = int.Parse(str[i].ToString());
            var sprite = NumbersObject.GetSpriteForNumber(digit);

            if (_existingDigits.Count > i)
            {
                _existingDigits[i].SetTexture(sprite, TransitionTime);
                _existingDigits[i].transform.localPosition = GetPosition(i);
            }
            else
            {
                var gob = Instantiate(ControlsPrefab, transform);
                gob.SetTexture(sprite, .01f);
                gob.transform.localPosition = GetPosition(i);
                _existingDigits.Add(gob);

            }
            
        }

        for (var i = str.Length; i < _existingDigits.Count; i++)
        {
            Destroy(_existingDigits[i].gameObject);
        }

        _existingDigits = _existingDigits.Take(str.Length).ToList();
    }
}

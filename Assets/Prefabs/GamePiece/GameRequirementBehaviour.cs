using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRequirementBehaviour : MonoBehaviour
{
    public NumberAssetObject NumbersObject;

    public GamePieceBehaviour GamePieceBehaviour;
    public GamePieceShaderControls NumberSpriteControls;
    private GameBoardBehaviour _boardBehaviour;
    private GameBoardRequirement _requirement;

    public Color IncorrectBorderColor;

    public bool IsCorrect;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_boardBehaviour || _requirement == null) return;
        
        var isCorrect = _boardBehaviour.CurrentCounts.TryGetValue(_requirement.PieceObject, out var count) && count == _requirement.RequiredCount;

        if (isCorrect && !IsCorrect)
        {
           
            NumberSpriteControls.SetColors(_requirement.PieceObject.Color, _requirement.PieceObject.Color);
            GamePieceBehaviour.ShaderControls.SetColors(_requirement.PieceObject.Color, _requirement.PieceObject.Color);
        } else if (!isCorrect && IsCorrect)
        {
            NumberSpriteControls.SetColors(Color.black, _requirement.PieceObject.Color);
            GamePieceBehaviour.ShaderControls.SetColors(Color.black, _requirement.PieceObject.Color);

        }

        IsCorrect = isCorrect;
    }

    public void SetRequirement(GameBoardRequirement requirement, GameBoardBehaviour boardBehaviour)
    {
        _requirement = requirement;
        _boardBehaviour = boardBehaviour;
        NumberSpriteControls.SetTexture( NumbersObject.GetSpriteForNumber(requirement.RequiredCount));
        GamePieceBehaviour.Set(requirement.PieceObject);
        NumberSpriteControls.SetColors(Color.black, _requirement.PieceObject.Color);

    }
}

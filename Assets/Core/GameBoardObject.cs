using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class GameBoardObject : ScriptableObject
{
    public List<GameBoardSlot> Slots;
    public List<GameBoardRequirement> Requirements;
    public int PerfectMoveCount;
    public int Seed;
    public GameFlags Flags = new GameFlags();
    
    public IEnumerable<GameBoardSlot> ValidSlots => Slots.Where(s => s != null);
    
    
    #if UNITY_EDITOR
    [ContextMenu("Save To Disk")]
    public void WriteToDisk()
    {
        // UnityEditor.EditorUtility.Save
        var path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save path", "level", "asset", "Save the object");
        if (string.IsNullOrEmpty(path)) return;
        UnityEditor.AssetDatabase.CreateAsset(this, path);
    }
    #endif
    
    public RectInt ValidBounds 
    {
        get
        {
            var xVals = ValidSlots.Select(s => s.Location.x);
            var yVals = ValidSlots.Select(s => s.Location.y);

            var xMin = xVals.Min();
            var yMin = yVals.Min();
            var xMax = xVals.Max();
            var yMax = yVals.Max();
            
            return new RectInt(xMin, yMin, (xMax - xMin) + 1, (yMax - yMin) + 1);
        }
    }

    public int GetPieceRequirement(GamePieceObject piece)
    {
        return Requirements.FirstOrDefault(p => p.PieceObject == piece)?.RequiredCount ?? 0;
    }
}

[Serializable]
public class GameFlags
{
    public bool DisableMoveCounter;
    public bool DisableStars;
    public bool DisableUndo;
    public bool DisableHInts;
    public bool DisableStarCounter;
    public bool DisableRequirements;
    public bool DisablePieces;
    public bool DisableInput;
    public GameObject ExtraPrefab;
}

[Serializable]
public class GameBoardSlot
{
    public Vector2Int Location;
    public GamePieceObject PieceObject;
    public bool IsLocked;
    public int TouchCount;
    
    public int GetCode()
    {
        return CombineHashCodes(Location.x, CombineHashCodes(Location.y, CombineHashCodes(IsLocked ? 2 : 1, CombineHashCodes(PieceObject.Code, TouchCount+1))));
    }

    
    private static int CombineHashCodes(int h1, int h2)
    {
        return (h1 << 5) + h1 ^ h2;

        // another implementation
        //unchecked
        //{
        //    var hash = 17;

        //    hash = hash * 23 + h1;
        //    hash = hash * 23 + h2;

        //    return hash;
        //}
    }
}

[Serializable]
public class GameBoardRequirement
{
    public int RequiredCount;
    public GamePieceObject PieceObject;
}
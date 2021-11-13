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
    
    public IEnumerable<GameBoardSlot> ValidSlots => Slots.Where(s => s != null);
    
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
public class GameBoardSlot
{
    public Vector2Int Location;
    public GamePieceObject PieceObject;

    public int GetCode()
    {
        return CombineHashCodes(Location.x, CombineHashCodes(Location.y, PieceObject.Code));
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
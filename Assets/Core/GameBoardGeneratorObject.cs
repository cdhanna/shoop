using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

[CreateAssetMenu]
public class GameBoardGeneratorObject : ScriptableObject
{

    public GamePieceCollectionObject PieceCollection;

    public SelectionRange CellCountRange;
    public SelectionRange PieceTypeCountRange;
    public SelectionRange MoveRange;


    public GameBoardObject Generate(int seed = -1, GameBoardObject board = null)
    {
        return GenerateRoutine(seed, board).FirstOrDefault(f => f != null);
    }

    public IEnumerable<GameBoardObject> GenerateRoutine(int seed=-1, GameBoardObject board=null)
    {
        if (seed < 0)
        {
            seed = new Random().Next(1, int.MaxValue);
        }
        
        var rand = new Random(seed);
        
        
        board =  CreateInstance<GameBoardObject>();
        board.Seed = seed;
        IEnumerable Attempt()
        {

            // step 1. generate piece locations
            var locations = GenerateLocations(rand);

            // step 2. assign types to each cell
            var slots = GenerateSlots(rand, locations);
            board.Slots = slots;
            // step 3. randomly come up with some requirements
            var reqs = GenerateRequirements(rand, slots);
            board.Requirements = reqs;
            // step 4. run BFS operation to see how many moves it takes to win
            foreach (var pendingMoveCount in GetMinMovesToWin(board))
            {
                if (pendingMoveCount?.HasValue ?? false)
                {
                    board.PerfectMoveCount = pendingMoveCount.Value;
                    break;
                }

                yield return null;
            }
        }
        var attempts = 0;
        while (board.PerfectMoveCount < MoveRange.Min || board.PerfectMoveCount > MoveRange.Max || GameBoardState.FromBoard(board).IsWin)
        {
            attempts++;
            if (attempts > 1000)
            {
                break;
            }

            foreach (var _ in Attempt())
            {
                yield return null;
            }
        }

        yield return board;
    }

    IEnumerable<PendingResult<int>> GetMinMovesToWin(GameBoardObject board)
    {
        return GameBoardAI.CountMovesToWin(board);
    }
    
    
    List<Vector2Int> GenerateLocations(Random rand)
    {
        var locations = new HashSet<Vector2Int>();

        locations.Add(new Vector2Int(0, 0));
        
        var neighbors = new Dictionary<Vector2Int, int>();

        void IncLocal(Vector2Int local)
        {
            neighbors.TryGetValue(local, out var score);
            neighbors[local] = (score + 1) * 2;
            neighbors[local] /= (local.sqrMagnitude + 1);
            neighbors[local] = Math.Max(1, neighbors[local]);
        }
        
        void CheckNeighbors()
        {
            foreach (var local in locations)
            {
                IncLocal(local + new Vector2Int(1, 0));
                IncLocal(local + new Vector2Int(-1, 0));
                IncLocal(local + new Vector2Int(0, 1));
                IncLocal(local + new Vector2Int(0, -1));
            }

            foreach (var local in locations)
            {
                neighbors.Remove(local);
            }
        }

        var cellCount = CellCountRange.RandomValue(rand);
        for (var i = 0; i < cellCount; i++)
        {
            CheckNeighbors();

            var weightSum = neighbors.Select(kvp => kvp.Value).Sum();
            var n = rand.Next(weightSum);
            var start = 0;
            var genned = false;
            foreach (var kvp in neighbors)
            {
                if (n >= start && n < (start + kvp.Value))
                {
                    locations.Add(kvp.Key);
                    genned = true;
                    break;
                }

                start += kvp.Value;
            }

            if (!genned)
            {
                Debug.LogWarning("Didn't generate a spot for " + i);
                foreach (var neighbor in neighbors)
                {
                    Debug.LogWarning($"neighbor {neighbor.Key} -> {neighbor.Value}");
                }
            }
        }
        
        
        return locations.ToList();
    }
    
    List<GameBoardSlot> GenerateSlots(Random rand, List<Vector2Int> locations)
    {
        var slots = new List<GameBoardSlot>();

        var uniqueCount = PieceTypeCountRange.RandomValue(rand);
        var randomList = PieceCollection.Pieces.ToList();
        randomList.Sort((a, b) => rand.Next(-1, 1));
        
        foreach (var local in locations)
        {
            slots.Add(new GameBoardSlot
            {
                Location = local,
                PieceObject = randomList[ rand.Next(uniqueCount) ]
            });
        }
            
        return slots;
    }

    List<GameBoardRequirement> GenerateRequirements(Random rand, List<GameBoardSlot> slots)
    {
        var uniqueTypes = new HashSet<GamePieceObject>(slots.Select(s => s.PieceObject)).ToList();
        // randomly create a weight for each type
        // for each number, use weight to assign it to a group
        var weights = uniqueTypes.Select(_ => rand.Next(1, 4)).ToList();
        var weightSum = weights.Sum();
        
        var reqDict = slots.Skip(uniqueTypes.Count).Aggregate(uniqueTypes.ToDictionary(x => x, x => 1), (agg, _) =>
        {
            // randomly pick a uniqueType
            var n = rand.Next(weightSum);

            var start = 0;
            var genned = false;
            for (var i = 0 ; i < weights.Count; i ++)
            {
                var weight = weights[i];
                if (n >= start && n <= (start + weight))
                {
                    // found!
                    var type = uniqueTypes[i];
                    agg[type] += 1;
                    genned = true;
                    break;
                }

                start += weight;
            }

            return agg;
        });

        if (reqDict.Select(k => k.Value).Sum() < slots.Count)
        {
            Debug.LogError("Count is low!");
        }

        var reqs = reqDict.Select(kvp => new GameBoardRequirement
        {
            PieceObject = kvp.Key,
            RequiredCount = kvp.Value
        }).ToList();
        
        return reqs;
    }

}


[Serializable]
public struct SelectionRange
{
    public int Min, Max;

    public int RandomValue(Random rand) => rand.Next(Min, Max);
}
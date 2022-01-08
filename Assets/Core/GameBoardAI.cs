using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PendingResult<T>
{
    public T Value { get; }
    public bool HasValue { get; }

    public PendingResult(T value)
    {
        Value = value;
        HasValue = true;
    }
}
public class GameBoardAI
{
    public static IEnumerable<GameBoardState> BFS(GameBoardObject board,
        Dictionary<GameBoardState, GameBoardState> history = null) => BFS(GameBoardState.FromBoard(board), history);
    public static IEnumerable<GameBoardState> BFS(GameBoardState board, Dictionary<GameBoardState, GameBoardState> history=null)
    {
        var toExplore = new Queue<GameBoardState>();
        toExplore.Enqueue(board);

        var visited = new HashSet<int>();
        var maxLimit = 99999;
        var limit = 0;

        history?.Clear();


        while (toExplore.Count > 0)
        {
            limit++;
            if (limit >= maxLimit)
            {
                Debug.Log("Hit max limit!!");
                break;
            }
            var curr = toExplore.Dequeue();
            var hash = curr.ComputeBoardHash();

            if (visited.Contains(hash))
            {
                continue; // skip this one.
            }
            
            yield return curr;
            visited.Add(hash);

            foreach (var next in curr.Expand())
            {
                if (history != null) history[next] = curr;
                toExplore.Enqueue(next);
            }
        }
        
    }


    public static int CountMovesToWinInstant(GameBoardObject board)
    {
        return CountMovesToWin(board).FirstOrDefault(f => f?.HasValue ?? false)?.Value ?? -1;
    }
    
    public static IEnumerable<PendingResult<int>> CountMovesToWin(GameBoardObject board)
    {
        // foreach (var state in BFS(board))
        // {
        //     if (state.IsWin)
        //     {
        //         return state.MovesTaken;
        //     }
        // }

        foreach (var state in BFS(board))
        {
            if (!state.IsWin)
            {
                yield return null;
                continue;
            }
            yield return new PendingResult<int>(state.MovesTaken);
            yield break;
        }
        yield return new PendingResult<int>(-1);
        
    }

    public static IEnumerable<GameSwapMove> Solve(GameBoardState board)
    {
        var history = new Dictionary<GameBoardState, GameBoardState>();

        if (board.IsWin)
        {
            Debug.LogError("board is already won");
        }
        
        var winState = BFS(board, history).FirstOrDefault(state => state.IsWin);
        if (winState == null)
        {
            Debug.LogError("No winning state found");
            
        }
        
        // create a trace from the winState back to the start board
        var curr = winState;
        var trace = new List<GameBoardState>();
        trace.Add(curr);

        while (curr != board && curr != null)
        {
            curr = history[curr];
            trace.Add(curr);

        }

        Debug.Log("There are " + trace.Count + " steps to victory");
        trace.Reverse();
        
        for (var i = 0 ; i < trace.Count -1; i ++)
        {
            var end = trace[i + 1];
            var start = trace[i];
            yield return GameBoardState.IdentifySwap(start, end);

        }
        
     
    }

}

public class GameSwapMove
{
    public GameBoardSlot Target;
    public HashSet<GameBoardSlot> Cluster;
}

public class GameBoardState
{
    public List<GameBoardSlot> Slots;
    public List<GameBoardRequirement> Requirements;
    public int MovesTaken;

    public int ComputeBoardHash()
    {

        var c = 1;
        for (var i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];
            c = CombineHashCodes(c, slot.GetCode());
        }

        return c;

        // return string.Join(";", Slots // around 2500ms
        //     // .OrderBy(s => $"{s.Location}".GetHashCode())
        //     .Select(s => $"{s.Location.x},{s.Location.y},{s.PieceObject.Code}")).GetHashCode();
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

    public static GameSwapMove IdentifySwap(GameBoardState start, GameBoardState dest)
    {
        // identify the differences between start and dest, and group the differences.

        var startSlots = start.Slots.ToDictionary(s => s.Location);
        var destSlots = dest.Slots.ToDictionary(s => s.Location);
        
        // ASSUMPTION: all the keys are the same in startSlots...
        var differences = startSlots.Where(kvp => kvp.Value.PieceObject != destSlots[kvp.Key].PieceObject).ToList();
        var groups = differences.GroupBy(s => s.Value.PieceObject).ToList();

        if (groups.Count != 2)
        {
            Debug.LogError("There are more differences between start and dest than one swap can do...");
           
        }
        var group1 = groups[0].ToList();
        var group2 = groups[1].ToList();

        var targetGroup = group1.Count == 1
            ? group1
            : group2;
        var clusterGroup = group1.Count == 1
            ? group2
            : group1;
        
        Debug.Log($"Target {targetGroup[0].Key}");
        Debug.Log($"Cluster \n {string.Join("\n-", clusterGroup.Select(c => c.Key))}");
        
        return new GameSwapMove
        {
            Target = targetGroup[0].Value,
            Cluster = new HashSet<GameBoardSlot>(clusterGroup.Select(s => s.Value))

        };
    }
    
    public static GameBoardState FromBoard(GameBoardObject board)
    {
        return new GameBoardState
        {
            Slots = board.Slots.ToList(),
            Requirements = board.Requirements.ToList()
        };
    }
    
    
    public bool IsWin
    {
        get
        {
            var counts = PieceCounts;
            foreach (var req in Requirements)
            {
                if (!counts.TryGetValue(req.PieceObject, out var currentCount) || currentCount != req.RequiredCount)
                    return false;
            }
            
            return true;
            // return Requirements.All(r =>
            //     counts.TryGetValue(r.PieceObject, out var currentCount) && currentCount == r.RequiredCount);
        }
    }

    Dictionary<GamePieceObject, int> PieceCounts2 => Slots
        .GroupBy(p => p.PieceObject)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Count());

    public Dictionary<GamePieceObject, int> PieceCounts
    {
        get
        {

            var counts = new Dictionary<GamePieceObject, int>();
            foreach (var req in Requirements)
            {
                counts[req.PieceObject] = 0;
            }
            
            foreach (var slot in Slots)
            {
                counts[slot.PieceObject]++;
            }

            return counts;
            // return Slots
            //     .GroupBy(p => p.PieceObject)
            //     .ToDictionary(kvp => kvp.Key, kvp => kvp.Count());
        }
    }

    public string Print()
    {
        var str = "";

        var dict = Slots.ToDictionary(s => s.Location);
        var bounds = ValidBounds;

            for (var y = bounds.yMin; y <= bounds.yMax; y++)
            {
                for (var x = bounds.xMin; x <= bounds.xMax; x++)
                {
                if (dict.TryGetValue(new Vector2Int(x, y), out var slot))
                {
                    str += slot.PieceObject.Code.ToString();
                }
                else
                {
                    str += "_";
                }
            }

            str += "\n";
        }
        
        return str;
    }
    
    public RectInt ValidBounds 
    {
        get
        {
            var xVals = Slots.Select(s => s.Location.x);
            var yVals = Slots.Select(s => s.Location.y);

            var xMin = xVals.Min();
            var yMin = yVals.Min();
            var xMax = xVals.Max();
            var yMax = yVals.Max();
            
            return new RectInt(xMin, yMin, (xMax - xMin) + 1, (yMax - yMin) + 1);
        }
    }

    
    public IEnumerable<GameBoardState> Expand()
    {
        
        /*
         * To identify all moves, we need to identify all clusters, and for each cluster, swap with each neighbor
         */

        var localToSlot = Slots.ToDictionary(s => s.Location);
        
        var seenSlots = new HashSet<GameBoardSlot>();
        var clusters = new List<HashSet<GameBoardSlot>>();

        void FloodCluster(HashSet<GameBoardSlot> cluster, GameBoardSlot start)
        {
            if (cluster.Contains(start)) return;

            var orig = start;
            cluster.Add(start);
            clusters.Add(new HashSet<GameBoardSlot>(cluster));

            var type = orig.PieceObject;
            if (localToSlot.TryGetValue(orig.Location + new Vector2Int(1, 0), out var left ) && left.PieceObject == type)
                FloodCluster(cluster, left);
            if (localToSlot.TryGetValue(orig.Location + new Vector2Int(-1, 0), out var right) && right.PieceObject == type)
                FloodCluster(cluster, right);
            if (localToSlot.TryGetValue(orig.Location + new Vector2Int(0, 1), out var top) && top.PieceObject == type)
                FloodCluster(cluster, top);
            if (localToSlot.TryGetValue(orig.Location + new Vector2Int(0, -1), out var low) && low.PieceObject == type)
                FloodCluster(cluster, low);
        }

        HashSet<GameBoardSlot> GetNeighbors(HashSet<GameBoardSlot> cluster)
        {
            var neighbors = new HashSet<GameBoardSlot>();
            var clusterPiece = cluster.First().PieceObject;

            foreach (var member in cluster)
            {
                if (localToSlot.TryGetValue(member.Location + new Vector2Int(1, 0), out var neighbor) && clusterPiece != neighbor.PieceObject && !cluster.Contains(neighbor))
                    neighbors.Add(neighbor);
                if (localToSlot.TryGetValue(member.Location + new Vector2Int(-1, 0), out neighbor) && clusterPiece != neighbor.PieceObject && !cluster.Contains(neighbor))
                    neighbors.Add(neighbor);
                if (localToSlot.TryGetValue(member.Location + new Vector2Int(0, 1), out neighbor) && clusterPiece != neighbor.PieceObject && !cluster.Contains(neighbor))
                    neighbors.Add(neighbor);
                if (localToSlot.TryGetValue(member.Location + new Vector2Int(0, -1), out neighbor) && clusterPiece != neighbor.PieceObject && !cluster.Contains(neighbor))
                    neighbors.Add(neighbor);
            }

            return neighbors;
        }
        
        foreach (var slot in Slots)
        {
            
            // new cluster!
            var cluster = new HashSet<GameBoardSlot>();
            
            FloodCluster(cluster, slot);
        }

        foreach (var cluster in clusters)
        {
            if (cluster.Count == 0) continue;
            

            var clusterPiece = cluster.First().PieceObject;
            var clusterLocations = new HashSet<Vector2Int>(cluster.Select(c => c.Location));


            var clusterMask = new bool[Slots.Count];
            for (var i = 0; i < Slots.Count; i++)
            {
               clusterMask[i] = clusterLocations.Contains(Slots[i].Location);
            }
            
            // identify all neighbors of cluster
            var neighbors = GetNeighbors(cluster);
            foreach (var neighbor in neighbors)
            {
                // we could do a swap of this cluster, with this neighbor

                var targetPiece = neighbor.PieceObject;
                var targetLocation = neighbor.Location;
       
                var nextSlots2 = new GameBoardSlot[Slots.Count];
                for (var i = 0; i < Slots.Count; i++)
                {
                    var local = Slots[i].Location;
                    var isTarget = local == targetLocation;
                    var isCluster = clusterMask[i];
                    nextSlots2[i] = new GameBoardSlot
                    {
                        Location = Slots[i].Location,
                        PieceObject = isTarget ? clusterPiece : (isCluster ? targetPiece : Slots[i].PieceObject)
                    };
                }
                
                var nextState = new GameBoardState
                {
                    Requirements = Requirements,
                    Slots = nextSlots2.ToList(),
                    MovesTaken = MovesTaken + 1
                };
                
                yield return nextState;
            }
        }
        
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameBoardBehaviour : MonoBehaviour
{

    public GamePieceBehaviour GamePieceBehaviour;
    public GameRequirementBehaviour RequirementBehaviour;
    public GameBoardObject BoardObject;
    public NumberAssetObject NumbersObject;

    public Transform PieceContainer, RequirementContainer;
    public CinemachineTargetGroup CameraTargetGroup;
    

    public bool IsBuildingStack;
    public GamePieceBehaviour SwapCheck;
    public bool SwapValid;

    public List<GamePieceBehaviour> PieceBehaviours = new List<GamePieceBehaviour>();
    
    public List<GamePieceBehaviour> HoverStack = new List<GamePieceBehaviour>();
    public GamePieceObject StackType => HoverStack.FirstOrDefault()?.PieceObject;
    
    
    public Stack<SwapMove> Swaps = new Stack<SwapMove>();
    public ExplosionBehaviour ExplosionBehaviour;

    public int MoveCount;

    public TextMeshProUGUI MoveCountText;
    public TextMeshProUGUI SeedText;

    public GamePieceShaderControls MoveCountControls, LoseCountControls;
    public Sprite ScoreWinSprite;
    public Sprite ScoreLoseSprite;
    
    public StarBehaviour Star1, Star2, Star3;


    public Dictionary<GamePieceObject, int> CurrentCounts { get; private set; }
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (LevelLoader.TryGetLevelLoader(out var loader))
        {
            BoardObject = loader.NextBoard;
            Debug.Log("Found " + loader.NextBoard.Seed);
        }

        SeedText.text = BoardObject.Seed.ToString();

        SpawnPieces();
        SpawnRequirements();

        _startScoreBorder = MoveCountControls.BorderColor;
        _startScoreFill = MoveCountControls.Renderer.color;
        _startScoreTextSDFSmooth = MoveCountControls.Smoothness;
        _startScoreTextSDFThreshold = MoveCountControls.SDFThreshold;

        _loseTextStartColor = LoseCountControls.BorderColor;
        
        SetMoveText();
    }

    // Update is called once per frame
    void Update()
    {
        var curState = GetBoardState();
        CurrentCounts = curState.PieceCounts;
    }

    public GameBoardState GetBoardState()
    {
        return new GameBoardState
        {
            MovesTaken = MoveCount,
            Requirements = BoardObject.Requirements,
            Slots = PieceBehaviours.Select(p => new GameBoardSlot
            {
                Location = p.Location,
                PieceObject = p.PieceObject
            }).ToList()
        };
    }

    void SpawnRequirements()
    {
        ClearRequirementContainer();
        for (var i = 0; i < BoardObject.Requirements.Count; i++)
        {
            var instance = Instantiate(RequirementBehaviour, RequirementContainer);
            instance.SetRequirement(BoardObject.Requirements[i], this);
            var ratio = (i+1f) / (BoardObject.Requirements.Count + 1f);
            
            instance.transform.localPosition = new Vector3(5 * (ratio - .5f), 0, 0);
        }
        var bounds = BoardObject.ValidBounds;

        // RequirementContainer.position = new Vector3(0, -.5f * (bounds.height + 2), 0);
        //RequirementContainer.position = new Vector3(0, .5f * (bounds.height + 2), 0);
    }

    void SpawnPieces()
    {
        ClearContainer();
        var bounds = BoardObject.ValidBounds;
        foreach (var slot in BoardObject.ValidSlots)
        {
            SpawnPiece(bounds, slot);
        }
    }

    void ClearRequirementContainer()
    {
        for (var i = 0; i < RequirementContainer.childCount; i++)
        {
            Destroy(RequirementContainer.GetChild(i).gameObject);
        }
    }

    void ClearContainer()
    {
        for (var i = 0; i < PieceContainer.childCount; i++)
        {
            Destroy(PieceContainer.GetChild(i).gameObject);
        }
    }

    public void GotoMenu()
    {
        LevelLoader.GotoMenu();
    }

    void SpawnPiece(RectInt bounds, GameBoardSlot slot)
    {
        if (slot.PieceObject == null) return;
            
        var instance = Instantiate(GamePieceBehaviour, PieceContainer);
        instance.Set(slot);
        instance.Set(BoardObject);
        instance.GameBoardBehaviour = this;
        PieceBehaviours.Add(instance);
        var pos = new Vector2Int(slot.Location.x - bounds.x - bounds.width / 2, slot.Location.y - bounds.y - bounds.height / 2);
        // Debug.Log($"SPAWN {pos} FROM {bounds}");
        instance.transform.localPosition = new Vector3(pos.x + .5f, pos.y, 0);

        CameraTargetGroup.AddMember(instance.transform, 1, 1);
        instance.TargetGroup = CameraTargetGroup;
        instance.OnMouseClicked += () =>
        {
            if (IsBuildingStack)
            {

                return;
            }
            
            // if the current click is in the stack, this is a deselection
            if (HoverStack.Contains(instance) || instance.PieceObject == StackType)
            {
                ClearStack();
                return;
            } 
            // if the current click is not in the stack, but there _is_ a stack, this _might_ be a swap. It's a swap if the mouse is released _quickly_ in the same square
            else if (HoverStack.Count > 0 && IsSlotNextToStack(slot))
            {
                SwapCheck = instance;
                return;
            }

            ClearStack();
            IsBuildingStack = true;
            Debug.Log("mouse down");
            TryToAddToStack(slot, instance);

        };
        instance.OnMouseReleased += () =>
        {
            if (SwapCheck != null)
            {
                PerformSwap();
            }
            
            OnMouseUp();
        };

        instance.OnMouseEntered += () =>
        {
            if (!IsBuildingStack) return;
            TryToAddToStack(slot, instance);
        };
        instance.OnMouseExited += () =>
        {
            if (SwapCheck == instance)
            {
                // we left the swap area...
                SwapCheck = null;
                ClearStack();
                IsBuildingStack = true;
                TryToAddToStack(slot, instance);
            }

        };
    }


    private float _startScoreTextSDFThreshold, _startScoreTextSDFSmooth;
    private Color _startScoreFill, _startScoreBorder;
    private int MovesLeft => BoardObject.PerfectMoveCount - MoveCount;
    private Color _loseTextStartColor;

    public Transform StarWin1, StarWin2, StarWin3;
    void SetMoveText()
    {

        var sprite = ScoreWinSprite;
        if (!IsWin)
        {
            sprite = NumbersObject.GetSpriteForNumber(Math.Max(0, MovesLeft));
            if (MovesLeft <= 0)
            {
                MoveCountControls.SetSDFProperties(0, 1, .3f);
                MoveCountControls.SetColors(new Color(0,0,0,0), new Color(0,0,0,0), .5f);
            }
            else
            {
                MoveCountControls.SetSDFProperties(_startScoreTextSDFThreshold, _startScoreTextSDFSmooth, .2f);
                MoveCountControls.SetColors(_startScoreFill, _startScoreBorder, .1f);
            }

            switch (MovesLeft)
            {
                case int m when m > 0:
                    Star1.Appear();
                    Star2.Appear();
                    Star3.Appear();
                
                    Star1.ScootTowardsTop();
                    Star2.ScootTowardsTop();
                    Star3.ScootTowardsTop();
                    
                    LoseCountControls.SetColors(new Color(0,0,0,0), new Color(0,0,0,0));
                    LoseCountControls.SetSDFProperties(1, 1);
                    break;
                case 0:
                    Star1.Vanish();
                    Star2.Appear();
                    Star3.Appear();
                    Star1.ScootTowardsTemp();
                    Star2.ScootTowardsTemp();
                    Star3.ScootTowardsTemp();
                    LoseCountControls.SetColors(new Color(0,0,0,0), new Color(0,0,0,0));
                    LoseCountControls.SetSDFProperties(1, 1);
                    break;
                case -1:
                    Star1.Vanish();
                    Star2.Vanish();
                    Star3.Appear();
                    Star1.ScootTowardsTemp2();
                    Star2.ScootTowardsTemp2();
                    Star3.ScootTowardsTemp2();
                    LoseCountControls.SetColors(new Color(0,0,0,0), new Color(0,0,0,0));
                    LoseCountControls.SetSDFProperties(1, 1);
                    break;
                case -2:
                    Star3.Vanish();
                    Star2.Vanish();
                    Star1.Vanish();
                    Star1.ScootTowardsTemp2();
                    Star2.ScootTowardsTemp2();
                    Star3.ScootTowardsTemp2();
                    LoseCountControls.SetColors(LoseCountControls.StartColor, _loseTextStartColor);
                    LoseCountControls.SetSDFProperties(LoseCountControls.StartSDFThreshold, LoseCountControls.StartSDFSmoothness);

                    break;
            }
            MoveCountControls.SetTexture(sprite);
            // if (MovesLeft > 0)
            // {
            //     Star1.Appear();
            //     
            //     Star2.ScootTowardsTop();
            //     Star3.ScootTowardsTop();
            // }
            // if (MovesLeft == 0)
            // {
            //     Star1.Vanish();
            //     Star2.Appear();
            //     Star2.ScootTowardsTemp();
            //     Star3.ScootTowardsTemp();
            // }
            //
            // if (MovesLeft == -1)
            // {
            //     Star2.Vanish();
            //     Star2.ScootTowardsTemp2();
            //     Star3.ScootTowardsTemp2();
            //     Star3.Appear();
            // }
            //
            //
            // if (MovesLeft == -2)
            // {
            //     Star3.Vanish();
            //     LoseCountControls.SetColors(LoseCountControls.StartColor, _loseTextStartColor);
            //     LoseCountControls.SetSDFProperties(LoseCountControls.StartSDFThreshold, LoseCountControls.StartSDFSmoothness);
            //
            // }
            // else
            // {
            //     LoseCountControls.SetColors(new Color(0,0,0,0), new Color(0,0,0,0));
            //     LoseCountControls.SetSDFProperties(1, 1);
            // }
            // if (MovesLeft > -2)
            // {
            //     
            // }
        }
        else 
        {
            MoveCountControls.SetSDFProperties(_startScoreTextSDFThreshold, _startScoreTextSDFSmooth, .2f);
            MoveCountControls.SetColors(_startScoreFill, _startScoreBorder, .1f);

            
            // if star 2 is available, thats the "leftmost" star for some stupid reason.
            switch (MovesLeft)
            {
                case 0:
                    Star3.Appear();
                    Star2.Appear();
                    Star1.Appear();
                    Star3.WinTarget = StarWin3;
                    Star2.WinTarget = StarWin1;
                    Star1.WinTarget = StarWin2;
                    Star3.ScootTowardsScore(.3f, 1);
                    Star2.ScootTowardsScore(.3f, .5f);
                    Star1.ScootTowardsScore(.3f, .75f);
                    break;
                case -1:
                    Star3.Appear();
                    Star2.Appear();
                    Star1.Appear();
                    Star3.WinTarget = StarWin2;
                    Star2.WinTarget = StarWin1;
                    Star1.WinTarget = StarWin3;
                    Star3.ScootTowardsScore(.3f, .75f);
                    Star2.ScootTowardsScore(.3f, .5f);
                    Star1.ScootTowardsScore(.3f, 1f, false);
                    break;
                case -2:
                    Star3.Appear();
                    Star2.Appear();
                    Star1.Appear();
                    Star3.WinTarget = StarWin1;
                    Star2.WinTarget = StarWin2;
                    Star1.WinTarget = StarWin3;
                    Star3.ScootTowardsScore(.3f, .5f);
                    Star2.ScootTowardsScore(.3f, .75f, false);
                    Star1.ScootTowardsScore(.3f, 1f, false);
                    break;
            }



            sprite = ScoreWinSprite;
            MoveCountControls.SetTexture(sprite);
            LoseCountControls.SetColors(new Color(0,0,0,0), new Color(0,0,0,0));
            LoseCountControls.SetSDFProperties(1, 1);
        } 




    }

    public void Undo()
    {
        if (Swaps.Count == 0) return;
        var swap = Swaps.Pop();
        swap.Backwards();
        
        SwapCheck = null;
        ClearStack();
        MoveCount--;
        SetMoveText();
    }

    void PerformSwap()
    {
        var move = new SwapMove(this, HoverStack, SwapCheck) {ExplosionBehaviour = ExplosionBehaviour};
        move.Forwards(() =>
        {
            MoveCount++;
            SetMoveText();
        });

        Swaps.Push(move);
        SwapCheck = null;
        ClearStack();

    }

    public void PerformSwap(GameSwapMove gameMove)
    {
        var move = new SwapMove(this, GetPiecesForSlots(gameMove.Cluster.ToArray()), GetPiecesForSlots(gameMove.Target).First())
        {
            ExplosionBehaviour = ExplosionBehaviour
        };
        move.Forwards(() =>
        {
            MoveCount++;
            SetMoveText();
        });
  
        Swaps.Push(move);
        SwapCheck = null;
        ClearStack();


    }

    public List<GamePieceBehaviour> GetPiecesForSlots(params GameBoardSlot[] slots)
    {
        return PieceBehaviours.Where(p => slots.Any(s => s.Location == p.Location)).ToList();
    }

    void TryToAddToStack(GameBoardSlot slot, GamePieceBehaviour gamePieceBehaviour)
    {

        if (MovesLeft == -2 || IsWin) return; 
        
        
        if (HoverStack.Contains(gamePieceBehaviour))
        {
            return; // we can't re-add this piece to the stack.
        }
        
        if (StackType == null)
        {
            AddToStack(gamePieceBehaviour);
            return; // if this is the first piece, then we are all good to rock.
        }


        if (!IsSlotNextToStack(slot))
        {
            return; // if no piece is within one block, we can't add this.
        }
        
        
        // can't add this, but doesn't mean the build is over.
            
        if (StackType == gamePieceBehaviour.PieceObject)
        {
            AddToStack(gamePieceBehaviour);
            return;
        }
    }

    bool IsSlotNextToStack(GameBoardSlot slot) => HoverStack.Any(s => (s.Location - slot.Location).sqrMagnitude <= 1);
    void AddToStack(GamePieceBehaviour gamePieceBehaviour)
    {
        HoverStack.Add(gamePieceBehaviour);
        gamePieceBehaviour.SelectInStack();
    }

    public bool IsWin
    {
        get
        {
            return GetBoardState().IsWin;
            // var counts = GetPieceCounts;
            // return BoardObject.Requirements.All(r =>
            //     counts.TryGetValue(r.PieceObject, out var currentCount) && currentCount == r.RequiredCount);
        }
    }

    Dictionary<GamePieceObject, int> GetPieceCounts => PieceBehaviours
            .GroupBy(p => p.PieceObject)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Count());
    
    
    void ClearStack()
    {
        foreach (var s in HoverStack)
        {
            s.DeselectInStack();
        }
        SwapCheck = null;
        HoverStack.Clear();
    }

    private void OnMouseUp()
    {
        if (IsBuildingStack)
        {
            // finish selection
            IsBuildingStack = false;
        }
    }

    private void OnMouseDown()
    {
        ClearStack();
    }
}

[Serializable]
public class SwapMove
{
    public GamePieceObject GroupType, TargetType;
    public List<GamePieceBehaviour> Group;
    public GamePieceBehaviour Target;
    public GameBoardBehaviour Board;

    public ExplosionBehaviour ExplosionBehaviour;

    public SwapMove()
    {
    }

    public SwapMove(GameBoardBehaviour board, List<GamePieceBehaviour> group, GamePieceBehaviour target)
    {
        Board = board;
        Group = group.ToList();
        Target = target;
        GroupType = group.First().PieceObject;
        TargetType = target.PieceObject;
    }

    public void Forwards(Action cb=null)
    {
        ExplosionBehaviour explosion = null;

        
        explosion = Object.Instantiate(ExplosionBehaviour, Board.transform);
        explosion.SetColor(GroupType.Color);
        explosion.transform.localPosition = Target.transform.localPosition;
        explosion.Blowup();
        CameraShakeBehaviour.TryShake(.4f, .2f);
        Target.Set(GroupType);

        Board.CameraTargetGroup.m_Targets[Board.CameraTargetGroup.FindMember(Target.transform)].radius = 5;
        IEnumerator Routine()
        {
            yield return new WaitForSecondsRealtime(.03f);    

            Board.CameraTargetGroup.m_Targets[Board.CameraTargetGroup.FindMember(Target.transform)].radius = 1;

            yield return new WaitForSecondsRealtime(.10f);    
            

            CameraShakeBehaviour.TryShake(.2f, .1f);

            foreach (var piece in Group)
            {
                piece.Set(TargetType);
                // explosion = Object.Instantiate(ExplosionBehaviour, Board.transform);
                // explosion.SetColor(TargetType.Color);
                // explosion.transform.localPosition = piece.transform.localPosition;
                // explosion.Intensity = .3f;
                // explosion.Blowup();
            }


            cb?.Invoke();
        }

        Board.StartCoroutine(Routine());
        


        
    }

    public void Backwards()
    {
        foreach (var piece in Group)
        {
            piece.Set(GroupType);
        }
        
        var explosion = Object.Instantiate(ExplosionBehaviour, Board.transform);
        explosion.SetColor(TargetType.Color);
        explosion.SetFlip(true);
        explosion.transform.localPosition = Target.transform.localPosition;
        explosion.Blowup();       
        CameraShakeBehaviour.TryShake(.5f, .1f);

        Target.Set(TargetType);
    }
    
}
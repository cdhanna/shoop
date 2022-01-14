using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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

    public GamePieceShaderControls HintControl;

    public Dictionary<GamePieceObject, int> CurrentCounts { get; private set; }

    private Coroutine _hintRoutine;

    public SoundManifestObject SoundManifestObject;

    public AudioSource DialAudioSource;
    
    PostProcessVolume m_Volume;
    public ChromaticAberration m_Aberration;

    public Bloom m_Bloom;

    public GameObject StarScoreContainer;
    public GameObject StarCountContainer;

    public GameCanvasController Menu;

    public event Action OnNextInput; 
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (LevelLoader.TryGetLevelLoader(out var loader))
        {
            BoardObject = loader.NextBoard;
            Debug.Log("Found " + loader.NextBoard.Seed);
        }

        if (BoardObject?.Flags?.ExtraPrefab)
        {
            Instantiate(BoardObject.Flags.ExtraPrefab);
        }
        

        Flags = new GameFlags
        {
            DisablePieces = BoardObject.Flags.DisablePieces,
            DisableRequirements = BoardObject.Flags.DisableRequirements,
            DisableStars = BoardObject.Flags.DisableStars,
            DisableUndo = BoardObject.Flags.DisableUndo,
            DisableHInts = BoardObject.Flags.DisableHInts,
            DisableMoveCounter = BoardObject.Flags.DisableMoveCounter,
            DisableStarCounter = BoardObject.Flags.DisableStarCounter,
            DisableInput = BoardObject.Flags.DisableInput
        };
        
        
        m_Aberration = ScriptableObject.CreateInstance<ChromaticAberration>();
        // m_Aberration.enabled.Override(true);
        m_Aberration.intensity.Override(.15f);

        m_Bloom = ScriptableObject.CreateInstance<Bloom>();
        // m_Bloom.enabled.Override(true);

        m_Volume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, m_Aberration, m_Bloom);

        SeedText.text = BoardObject.Seed.ToString();

        SpawnPieces();
        SpawnRequirements();

        _startScoreBorder = MoveCountControls.BorderColor;
        _startScoreFill = MoveCountControls.Renderer.color;
        _startScoreTextSDFSmooth = MoveCountControls.Smoothness;
        _startScoreTextSDFThreshold = MoveCountControls.SDFThreshold;

        _loseTextStartColor = LoseCountControls.BorderColor;
        
        StarSpendUI.color = new Color(0,0,0,0);
        SetMoveText();
        
    }
    
    private GameFlags _flags = new GameFlags();
    public GameFlags Flags = new GameFlags();

    public TextMeshProUGUI ContinueText;
    private int _lastInputListenerText;
    private Sequence _inputSequence;
    private Sequence _inputSequenceShow;

    private bool _wasWon;
    private float _winAt;

    public bool IsWinForAMoment => IsWinForDuration();
    public bool IsWinForDuration(float forDuration = .3f)
    {
        return _wasWon && (Time.realtimeSinceStartup > _winAt + forDuration);
    }
    
    // Update is called once per frame
    void Update()
    {
        var curState = GetBoardState();
        CurrentCounts = curState.PieceCounts;


        void HandleWinTracker()
        {
            if (IsWin && !_wasWon)
            {
                // start time!
                _winAt = Time.realtimeSinceStartup;
            }

            _wasWon = IsWin;
        }
        HandleWinTracker();

        void HandleListenerUI()
        {
            var listenerCount = OnNextInput?.GetInvocationList().Length ?? 0;
            if (listenerCount != _lastInputListenerText)
            {
                if (listenerCount > 0)
                {
                    // don't show it for the first .3 seconds...
                    _inputSequenceShow?.Kill();
                    _inputSequence?.Kill();

                    _inputSequenceShow = DOTween.Sequence().AppendInterval(.4f).Append(ContinueText.DOFade(.2f, .2f));
                }
                else
                {
                    // debounce this...
                    _inputSequence?.Kill();
                    _inputSequence = DOTween.Sequence().AppendInterval(.4f).Append(ContinueText.DOFade(0, .2f));
                }

                _lastInputListenerText = listenerCount;
            }
        }


        HandleListenerUI();
        
        var b = false;
        CheckDisable(ref _flags.DisableRequirements, ref Flags.DisableRequirements, RequirementContainer.gameObject);
        // CheckDisable(ref b, ref Flags.DisableMoveCounter, MoveCountControls.gameObject);
        CheckDisable(ref _flags.DisableMoveCounter, ref Flags.DisableMoveCounter, LoseCountControls.gameObject, MoveCountControls.gameObject);
        CheckDisable(ref _flags.DisableMoveCounter, ref Flags.DisableMoveCounter, StarScoreContainer);
        CheckDisable(ref _flags.DisableStarCounter, ref Flags.DisableStarCounter, StarCountContainer);
        CheckDisable(ref _flags.DisableStars, ref Flags.DisableStars, StarScoreContainer);
        CheckDisable(ref _flags.DisablePieces, ref Flags.DisablePieces, PieceContainer.gameObject);
        
    }

    void TriggerInputListener()
    {
        OnNextInput?.Invoke();
        OnNextInput = null;
    }


    void CheckDisable(ref bool state, ref bool actual, params GameObject[] roots)
    {
        CheckDisable(ref state, ref actual, .2f, roots);
    }

    private Dictionary<GameObject, Vector3> _disableOriginalScales = new Dictionary<GameObject, Vector3>();
    
    void CheckDisable(ref bool state, ref bool actual, float duartion, params GameObject[] roots)
    {
        if (state != actual)
        {
            if (actual)
            {
                foreach (var root in roots)
                {
                    _disableOriginalScales[root] = root.transform.localScale;
                    root.transform.DOScale(Vector3.zero, .2f);
                }
            }
            else
            {
                foreach (var root in roots)
                    root.transform.DOScale(_disableOriginalScales[root], .2f);
            }

            state = actual;
        }
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

    public void SpawnRequirements()
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

        var dud = false;
        // CheckDisable(ref dud, ref Flags.DisableRequirements, 0, RequirementContainer.gameObject);

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
        TransitionHelperBehaviour.Instance.TransitionAndDoThing(LevelLoader.GotoMenu);
        
    }

    public bool HasHint => (_hintRoutine != null) || _hintMove != null;
    

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
            TriggerInputListener();
            
            if (HasHint)
            {
                PerformSwap(_hintMove);
                return;
            }
            
            if (IsBuildingStack)
            {
                return;
            }
            
            // if the current click is in the stack, this is a deselection
            if (HoverStack.Contains(instance) || instance.PieceObject == StackType)
            {
                ClearStack(false);
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
            if (MovesLeft <= 0 || Flags.DisableMoveCounter)
            {
                MoveCountControls.SetSDFProperties(0, 1, .3f);
                MoveCountControls.SetColors(new Color(0,0,0,0), new Color(0,0,0,0), .5f);
            }
            else
            {
                MoveCountControls.SetSDFProperties(_startScoreTextSDFThreshold, _startScoreTextSDFSmooth, .2f);
                MoveCountControls.SetColors(_startScoreFill, _startScoreBorder, .1f);
            }

            if (Flags.DisableMoveCounter) return;
            
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
        }
        else 
        {
            if (!Flags.DisableMoveCounter)
            {


                MoveCountControls.SetSDFProperties(_startScoreTextSDFThreshold, _startScoreTextSDFSmooth, .2f);
                MoveCountControls.SetColors(_startScoreFill, _startScoreBorder, .1f);
            }

            if (!Flags.DisableStars)
            {
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
            }


            if (!Flags.DisableMoveCounter)
            {
                sprite = ScoreWinSprite;
                MoveCountControls.SetTexture(sprite);
                LoseCountControls.SetColors(new Color(0, 0, 0, 0), new Color(0, 0, 0, 0));
                LoseCountControls.SetSDFProperties(1, 1);
            }
        } 




    }

    public void Undo()
    {
        if (Swaps.Count == 0) return;
        var swap = Swaps.Pop();
        swap.Backwards();
        DialAudioSource.PlayOneShot(SoundManifestObject.UndoSound, 1.5f);

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
            _hintMove = null;
            MoveCount++;
            SetMoveText();
        });

        if (HintControl.transform.localScale.x > .1f)
        {
            DialAudioSource.PlayOneShot(SoundManifestObject.HintAcceptSound);
        }
        else
        {
            DialAudioSource.PlayOneShot(SoundManifestObject.AcceptSound);
            
        }
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
            _hintMove = null;

            MoveCount++;
            SetMoveText();
        });
        DialAudioSource.PlayOneShot(SoundManifestObject.HintAcceptSound);
  
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

        if (Flags.DisableInput) return;
        if ( !Flags.DisableStars && (MovesLeft == -2 || IsWin)) return; 
        
        
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

    public StarStateProvider StarStateProvider;
    public SpriteRenderer StarSpendUI;
    public Color StarSpendColor;
    private SpriteRenderer _lastHint;
    private GameSwapMove _hintMove;
    
    public void StartHintShow()
    {
        if (_hintRoutine != null)
        {
            
            StopCoroutine(_hintRoutine);
            _hintRoutine = null;
        }

        
        var targetScale = new Vector3(8.4f, 8.4f, 1f);
        

        IEnumerator Show()
        {
            
            // subtract 3 
            StarStateProvider.GetState().Stars -= 3;

            var startPos = StarSpendUI.transform.position.y;
            var startColor = StarSpendUI.color;
            var dingCount = 0;
            
            void Ding(int repeats=0)
            {
                StarSpendUI.color = StarSpendColor;
                
                StarSpendUI.DOFade(.9f, .168f).SetEase(Ease.OutBounce);
                // StarSpendUI.DOColor(new Color(StarC, 0, 0, .6), .16f).SetEase(Ease.OutBounce);
                StarSpendUI.transform.DOBlendableLocalRotateBy(new Vector3(0, 0, Random.Range(-30, 30)), .15f);
                StarSpendUI.transform.parent.DOPunchScale(Vector3.one * .05f, .12f);
                StarSpendUI.transform.DOMoveY(startPos - .6f, .17f).SetEase(Ease.OutBounce).OnComplete(
                    () =>
                    {
                        StarSpendUI.transform.position = new Vector3(StarSpendUI.transform.position.x, startPos, 0);
                        StarSpendUI.color = new Color(0,0,0,0);

                        if (repeats > 0)
                        {
                            Ding(repeats - 1);
                        }
                    });
            }

            Ding(2);
    
            yield return new WaitForSecondsRealtime(.7f); // TODO: We can splice this with the actual move calculation to smooth out lag.

            var boardState = GetBoardState();
            var moves = GameBoardAI.Solve(boardState).ToList();
            
            var move = GameBoardAI.Solve(GetBoardState()).FirstOrDefault();
            _hintMove = move;
            //GameBoardBehaviour.PerformSwap(move);

            foreach (var clusterPiece in move.Cluster)
            {
                var slot = this.BoardObject.Slots.FirstOrDefault(s => s.Location == clusterPiece.Location);
                var piece = PieceBehaviours.FirstOrDefault(p => p.Location == clusterPiece.Location);

                TryToAddToStack(slot, piece);

                yield return new WaitForSecondsRealtime(.35f);
            }

            
            var targetPiece = PieceBehaviours.First(p => p.Location == move.Target.Location);
            
            HintControl.SetColors(new Color(0,0,0,.6f), targetPiece.PieceObject.Color, .01f );
            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + .5f;
            HintControl.transform.position = targetPiece.transform.position;
            yield return new WaitForSecondsRealtime(.02f);
            HintControl.transform.localScale = Vector3.one * 200;
  
            //
            // var explosion = Object.Instantiate(ExplosionBehaviour, transform);
            // explosion.SetColor(targetPiece.PieceObject.Color);
            // explosion.transform.position = targetPiece.transform.position;
            // // explosion.transform.localScale *= .85f;
            // explosion.Blowup();

            targetPiece.Renderer.sortingOrder = HintControl.Renderer.sortingOrder + 1;
            _lastHint = targetPiece.Renderer;
            DialAudioSource.PlayOneShot(SoundManifestObject.HintStartSound);

            while (Time.realtimeSinceStartup < endTime)
            {
                var r = (Time.realtimeSinceStartup - startTime) / (endTime - startTime);
                HintControl.transform.localScale = Vector3.Lerp(Vector3.one * 200, targetScale, r);
                
                yield return null;
            }

            HintControl.transform.localScale = targetScale;
            // CameraShakeBehaviour.TryShake(.3f, .4f);

            
            yield return null;
            _hintRoutine = null;

        }

        _hintRoutine = StartCoroutine(Show());
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

    public bool IsOver { get; set; }


    private float clearedStackAt;

    public bool HasEmptyStackForAwhile => (Time.realtimeSinceStartup - clearedStackAt) > .2f;
    
    void ClearStack(bool quiet=true)
    {
        foreach (var s in HoverStack)
        {
            s.DeselectInStack();
        }

        if (HoverStack.Count > 0 && !quiet)
        {
            DialAudioSource.PlayOneShot(SoundManifestObject.GetRandomDeselectSound(), .6f);
        }

        if (HoverStack.Count > 0)
        {
            clearedStackAt = Time.realtimeSinceStartup;
        }
        SwapCheck = null;
        HoverStack.Clear();
        HintControl.transform.localScale = Vector3.zero;
        if (_lastHint)
            _lastHint.sortingOrder = 0;
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
        TriggerInputListener();

        if (HasHint)
        {
            PerformSwap(_hintMove);
            return;
        }
        ClearStack(false);

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


       // Board.m_Aberration.intensity.Override(1);
        DOTween.To(() => Board.m_Aberration.intensity.value, v => Board.m_Aberration.intensity.Override(v), 1, .15f)
            .SetEase(Ease.OutBounce).OnComplete(
                () =>
                {
                    DOTween.To(() => Board.m_Aberration.intensity.value, v => Board.m_Aberration.intensity.Override(v),
                        .15f, .15f).SetEase(Ease.InBounce);
                });
        
        DOTween.To(() => Board.m_Bloom.intensity.value, v => Board.m_Bloom.intensity.Override(v), 4, .15f)
            .SetEase(Ease.OutBounce).OnComplete(
                () =>
                {
                    DOTween.To(() => Board.m_Bloom.intensity.value, v => Board.m_Bloom.intensity.Override(v),
                        3f, .2f).SetEase(Ease.InBounce);
                });
        
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
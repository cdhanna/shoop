using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Tutorial1Behaviour : MonoBehaviour
{

    public GameBoardBehaviour BoardBehaviour;
    
    public TextMeshProUGUI WelcomeText, GamePiecesText;
    public TextMeshProUGUI YouCanSwapText, TapAndDrag;
    public TextMeshProUGUI TapToSwap, BlamoText;
    public TextMeshProUGUI Req1, Req2;
    public TextMeshProUGUI TryItOut;
    public TextMeshProUGUI Winner;


    public GameBoardObject NextLevel;
    
    public bool IsReadyToShowTapToSwap;

    public bool ShowedReadyToShowTapToSwap;

    public bool IsReadyToShowBlamo, ShowedBlamo;

    public bool IsReadyToShowWin, ShowedWin;
    private Sequence _seq;

    // Start is called before the first frame update
    void Start()
    {
        WelcomeText.alpha = 0;
        GamePiecesText.alpha = 0;
        YouCanSwapText.alpha = 0;
        TapAndDrag.alpha = 0;
        TapToSwap.alpha = 0;
        BlamoText.alpha = 0;
        Req1.alpha = 0;
        Req2.alpha = 0;
        TryItOut.alpha = 0;
        Winner.alpha = 0;

        BoardBehaviour = FindObjectOfType<GameBoardBehaviour>();

        StartCoroutine(Stage1());

        // _seq = DOTween.Sequence()
        //     .AppendInterval(.7f)
        //     .Append(WelcomeText.DOFade(1, .4f))
        //     .AppendInterval(1.7f)
        //     .Append(WelcomeText.DOFade(0, .4f))
        //     .AppendInterval(.3f)
        //     .Append(GamePiecesText.DOFade(1, .4f).OnComplete(() =>
        //     {
        //         BoardBehaviour.Flags.DisablePieces = false;
        //     }))
        //     .AppendInterval(1.5f)
        //     .Append(GamePiecesText.DOFade(0, .1f))
        //     .Append(YouCanSwapText.DOFade(1, .2f))
        //     .AppendInterval(1.0f)
        //     .Append(TapAndDrag.DOFade(1, .2f).OnComplete(() =>
        //     {
        //         IsReadyToShowTapToSwap = true;
        //         BoardBehaviour.Flags.DisableInput = false;
        //     }));
        //
    }

    IEnumerator Stage1()
    {
        BoardBehaviour.BoardObject.Requirements[0].RequiredCount = 7;
        BoardBehaviour.BoardObject.Requirements[1].RequiredCount = 7;
        BoardBehaviour.SpawnRequirements();
        yield return new WaitForSecondsSkippable(BoardBehaviour, .7f);

        yield return WelcomeText.DOFade(1, .4f).WaitForCompletion();
        
        yield return new WaitForSecondsSkippable(BoardBehaviour, 1.7f);

        yield return WelcomeText.DOFade(0, .4f).WaitForCompletion();
        
        yield return new WaitForSecondsSkippable(BoardBehaviour, .3f);
        
        yield return GamePiecesText.DOFade(1, .4f).WaitForCompletion();
        BoardBehaviour.Flags.DisablePieces = false;
        
        yield return new WaitForSecondsSkippable(BoardBehaviour, 5f);

        yield return GamePiecesText.DOFade(0, .1f).WaitForCompletion();
        yield return YouCanSwapText.DOFade(1, .2f).WaitForCompletion();

        yield return new WaitForSecondsSkippable(BoardBehaviour, 5f);

        yield return TapAndDrag.DOFade(1, .2f).WaitForCompletion();
        IsReadyToShowTapToSwap = true;
        BoardBehaviour.Flags.DisableInput = false;
    }



    IEnumerator Stage2()
    {
        yield return new WaitForSecondsSkippable(BoardBehaviour, 1.5f);

        yield return TapAndDrag.DOFade(0, .1f).WaitForCompletion();
        yield return YouCanSwapText.DOFade(0, .1f).WaitForCompletion();

        yield return TapToSwap.DOFade(1, .2f).WaitForCompletion();
        IsReadyToShowBlamo = true;

    }

    IEnumerator Stage3()
    {
        yield return new WaitForSecondsSkippable(BoardBehaviour, .2f);

        yield return TapToSwap.DOFade(0, .1f).WaitForCompletion();
        yield return BlamoText.DOFade(1, .2f).WaitForCompletion();
        yield return BlamoText.transform.DOPunchScale(Vector3.one * .2f, .2f).WaitForCompletion();
        
        yield return new WaitForSecondsSkippable(BoardBehaviour, 1f);
        yield return BlamoText.DOFade(0, .4f).WaitForCompletion();
        
        BoardBehaviour.BoardObject.Requirements[0].RequiredCount = (BoardBehaviour.PieceBehaviours.Count(p => p.PieceObject == BoardBehaviour.BoardObject.Requirements[0].PieceObject) + Random.Range(1, 2)) % 5 + 1;
        BoardBehaviour.BoardObject.Requirements[1].RequiredCount = BoardBehaviour.PieceBehaviours.Count - BoardBehaviour.BoardObject.Requirements[0].RequiredCount;
        BoardBehaviour.SpawnRequirements();
        BoardBehaviour.Flags.DisableRequirements = false;
        IsReadyToShowWin = true;
        
        yield return new WaitForSecondsSkippable(BoardBehaviour, .5f);

        yield return Req1.DOFade(1, .2f).WaitForCompletion();
        yield return new WaitForSecondsSkippable(BoardBehaviour, .8f);
        yield return Req2.DOFade(1, .2f).WaitForCompletion();
        yield return new WaitForSecondsSkippable(BoardBehaviour, 5f);

        yield return Req1.DOFade(0, .1f).WaitForCompletion();
        yield return Req2.DOFade(0, .1f).WaitForCompletion();

        yield return new WaitForSeconds(.4f);

        yield return TryItOut.DOFade(1, .2f).WaitForCompletion();
    }

    IEnumerator Stage4()
    {
        yield return TryItOut.DOFade(0, .1f).WaitForCompletion();

        yield return new WaitForSeconds(.2f);
        yield return Winner.DOFade(1, .3f).WaitForCompletion();
        Winner.transform.DOPunchScale(Vector3.one * .2f, .2f);

    }

    // Update is called once per frame
    void Update()
    {

        
        if (IsReadyToShowTapToSwap && !ShowedReadyToShowTapToSwap && BoardBehaviour.HoverStack.Count > 0)
        {
            ShowedReadyToShowTapToSwap = true;
            StartCoroutine(Stage2());
            // DOTween.Sequence()
            //     .AppendInterval(1.5f)
            //     .Append(TapAndDrag.DOFade(0, .1f))
            //     .Append(YouCanSwapText.DOFade(0, .1f))
            //     .Append(TapToSwap.DOFade(1, .2f))
            //     .AppendCallback(() => IsReadyToShowBlamo = true);
            //     ;
        }

        if (IsReadyToShowBlamo && !ShowedBlamo && BoardBehaviour.Swaps.Count > 0)
        {
            ShowedBlamo = true;
            StartCoroutine(Stage3());
            //
            // DOTween.Sequence()
            //     .AppendInterval(.2f)
            //     .Append(TapToSwap.DOFade(0, .1f))
            //     .Append(BlamoText.DOFade(1, .2f))
            //     .Append(BlamoText.transform.DOPunchScale(Vector3.one * .2f, .2f))
            //     .AppendInterval(1f)
            //     .Append(BlamoText.DOFade(0, .4f))
            //
            //     .AppendCallback(() =>
            //     {
            //         BoardBehaviour.BoardObject.Requirements[0].RequiredCount = (BoardBehaviour.PieceBehaviours.Count(p => p.PieceObject == BoardBehaviour.BoardObject.Requirements[0].PieceObject) + Random.Range(1, 2)) % 5 + 1;
            //         BoardBehaviour.BoardObject.Requirements[1].RequiredCount = BoardBehaviour.PieceBehaviours.Count - BoardBehaviour.BoardObject.Requirements[0].RequiredCount;
            //         
            //         
            //         BoardBehaviour.SpawnRequirements();
            //         
            //         BoardBehaviour.Flags.DisableRequirements = false;
            //         IsReadyToShowWin = true;
            //     })
            //     .AppendInterval(.5f)
            //     .Append(Req1.DOFade(1, .2f))
            //     .AppendInterval(.8f)
            //     .Append(Req2.DOFade(1, .2f))
            //     .AppendInterval(2.8f)
            //     .Append(Req1.DOFade(0, .1f))
            //     .Append(Req2.DOFade(0, .1f))
            //     .AppendInterval(.4f)
            //     .Append(TryItOut.DOFade(1, .2f))
            //     
            //     ;
        }

        if (IsReadyToShowWin && BoardBehaviour.IsWinForAMoment && !ShowedWin)
        {
            ShowedWin = true;
            BoardBehaviour.Flags.DisableInput = true;
            LevelLoader.GetInstance().NextBoard = NextLevel;
            StartCoroutine(Stage4());
            // DOTween.Sequence()
            //     .Append(Req1.DOFade(0, .1f))
            //     .Append(Req2.DOFade(0, .1f))
            //     .Append(TryItOut.DOFade(0, .1f))
            //
            //     .AppendInterval(.2f)
            //
            //     .Append(Winner.DOFade(1, .3f))
            //     .Append(Winner.transform.DOPunchScale(Vector3.one * .2f, .2f))
            //
            //     ;
        }
    }
}

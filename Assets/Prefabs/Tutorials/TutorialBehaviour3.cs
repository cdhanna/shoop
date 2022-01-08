using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialBehaviour3 : MonoBehaviour
{
    
    public GameBoardBehaviour BoardBehaviour;
    
    public TextMeshProUGUI MoveCounterText1, MoveCounterText2, Stars1, Stars2, Lose1, Lose2, SolveAway, Win1, Win2;

    public bool ShowedUndo;
    public bool ShowedWin;


    public GameBoardObject NextLevel;
    
    // Start is called before the first frame update
    void Start()
    {
        MoveCounterText1.alpha = 0;
        MoveCounterText2.alpha = 0;
        Stars1.alpha = 0;
        Stars2.alpha = 0;
        Lose1.alpha = 0;
        Lose2.alpha = 0;
        SolveAway.alpha = 0;
        Win1.alpha = 0;
        Win2.alpha = 0;
        BoardBehaviour = FindObjectOfType<GameBoardBehaviour>();
        StartCoroutine(Stage1());
        //
        // DOTween.Sequence()
        //     .AppendInterval(.5f)
        //     .Append(MoveCounterText1.DOFade(1, .4f))
        //     .AppendInterval(1.5f)
        //     .Append(MoveCounterText1.DOFade(0, .4f))
        //     .AppendInterval(.2f)
        //     .Append(MoveCounterText2.DOFade(1, .4f))
        //
        //     
        //     .AppendInterval(2.5f)
        //     .Append(MoveCounterText2.DOFade(0, .2f))
        //
        //     .AppendCallback(() =>
        //     {
        //         BoardBehaviour.Flags.DisableInput = true;
        //         BoardBehaviour.Flags.DisableStars = false;
        //     })
        //     .AppendInterval(.5f)
        //     .Append(Stars1.DOFade(1, .4f))
        //     .AppendInterval(1.7f)
        //     .Append(Stars2.DOFade(1, .4f))
        //
        //     .AppendInterval(2.4f)
        //     
        //     .Append(Stars1.DOFade(0, .4f))
        //     .Append(Stars2.DOFade(0, .4f))
        //
        //     .AppendInterval(.5f)
        //
        //     .Append(Lose1.DOFade(1, .4f))
        //     .AppendInterval(1.7f)
        //     .Append(Lose2.DOFade(1, .4f))
        //     .AppendInterval(2.4f)
        //     
        //     .Append(Lose1.DOFade(0, .4f))
        //     .Append(Lose2.DOFade(0, .4f))
        //     .AppendInterval(.5f)
        //     .AppendCallback(() =>
        //     {
        //         BoardBehaviour.Flags.DisablePieces = false;
        //         BoardBehaviour.Flags.DisableRequirements = false;
        //         BoardBehaviour.Flags.DisableInput = false;
        //
        //     })
        //     .Append(SolveAway.DOFade(1, .4f))
        //     .AppendInterval(2.5f)
        //     .Append(SolveAway.DOFade(0, .7f))
        //
        //     ;

    }

    IEnumerator Stage1()
    {
        
        yield return new WaitForSecondsRealtime(.5f);
            
        yield return MoveCounterText1.DOFade(1, .4f).WaitForCompletion();
            
        yield return new WaitForSecondsSkippable(BoardBehaviour, 3);

        MoveCounterText1.DOFade(0, .3f).WaitForCompletion();
        yield return new WaitForSecondsRealtime(.2f);

        yield return MoveCounterText2.DOFade(1, .3f).WaitForCompletion();
        
        yield return new WaitForSecondsSkippable(BoardBehaviour, 15);

        yield return MoveCounterText2.DOFade(0, .3f).WaitForCompletion();
        yield return new WaitForSecondsSkippable(BoardBehaviour,.5f);

        BoardBehaviour.Flags.DisableInput = true;
        BoardBehaviour.Flags.DisableStars = false;
        
        yield return new WaitForSecondsSkippable(BoardBehaviour,.5f);
        BoardBehaviour.Flags.DisableMoveCounter = true;

        yield return Stars1.DOFade(1, .4f).WaitForCompletion();
        yield return new WaitForSecondsSkippable(BoardBehaviour, 2);
        yield return Stars2.DOFade(1, .4f).WaitForCompletion();

        yield return new WaitForSecondsSkippable(BoardBehaviour, 15);

        yield return Stars1.DOFade(0, .2f).WaitForCompletion();
        yield return Stars2.DOFade(0, .2f).WaitForCompletion();

        yield return new WaitForSecondsRealtime(.3f);

        yield return Lose1.DOFade(1, .3f).WaitForCompletion();
        yield return new WaitForSecondsSkippable(BoardBehaviour, 2);
        yield return Lose2.DOFade(1, .3f).WaitForCompletion();
        yield return new WaitForSecondsSkippable(BoardBehaviour, 15);

        yield return Lose1.DOFade(0, .2f).WaitForCompletion();
        yield return Lose2.DOFade(0, .2f).WaitForCompletion();
        BoardBehaviour.Flags.DisableMoveCounter = false;

        yield return new WaitForSecondsSkippable(BoardBehaviour,.5f);

        BoardBehaviour.Flags.DisablePieces = false;
        BoardBehaviour.Flags.DisableRequirements = false;
        BoardBehaviour.Flags.DisableInput = false;

        yield return SolveAway.DOFade(1, .4f).WaitForCompletion();
        yield return new WaitForSecondsRealtime(1.5f);
        yield return SolveAway.DOFade(0, .4f).WaitForCompletion();

        while (!BoardBehaviour.IsWinForAMoment) yield return null;
        
        BoardBehaviour.Flags.DisableStarCounter = false;
        BoardBehaviour.Flags.DisableUndo = true;

        LevelLoader.GetInstance().NextBoard = NextLevel;
        yield return new WaitForSecondsRealtime(.25f);
        yield return Win1.DOFade(1, .2f).WaitForCompletion();
        yield return new WaitForSecondsRealtime(.25f);
        yield return Win2.DOFade(1, .1f).WaitForCompletion();


        //     .Append()
        //     .AppendInterval(.2f)
        //     .Append(MoveCounterText2.DOFade(1, .4f))
        //
        //     
        //     .AppendInterval(2.5f)
        //     .Append(MoveCounterText2.DOFade(0, .2f))
        //
        //     .AppendCallback(() =>
        //     {
        //         BoardBehaviour.Flags.DisableInput = true;
        //         BoardBehaviour.Flags.DisableStars = false;
        //     })
        //     .AppendInterval(.5f)
        //     .Append(Stars1.DOFade(1, .4f))
        //     .AppendInterval(1.7f)
        //     .Append(Stars2.DOFade(1, .4f))
        //
        //     .AppendInterval(2.4f)
        //     
        //     .Append(Stars1.DOFade(0, .4f))
        //     .Append(Stars2.DOFade(0, .4f))
        //
        //     .AppendInterval(.5f)
        //
        //     .Append(Lose1.DOFade(1, .4f))
        //     .AppendInterval(1.7f)
        //     .Append(Lose2.DOFade(1, .4f))
        //     .AppendInterval(2.4f)
        //     
        //     .Append(Lose1.DOFade(0, .4f))
        //     .Append(Lose2.DOFade(0, .4f))
        //     .AppendInterval(.5f)
        //     .AppendCallback(() =>
        //     {
        //         BoardBehaviour.Flags.DisablePieces = false;
        //         BoardBehaviour.Flags.DisableRequirements = false;
        //         BoardBehaviour.Flags.DisableInput = false;
        //
        //     })
        //     .Append(SolveAway.DOFade(1, .4f))
        //     .AppendInterval(2.5f)
        //     .Append(SolveAway.DOFade(0, .7f))
    }

    // Update is called once per frame
    void Update()
    {
    }
}

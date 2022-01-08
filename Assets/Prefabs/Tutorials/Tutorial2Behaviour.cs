using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Tutorial2Behaviour : MonoBehaviour
{
    
    public GameBoardBehaviour BoardBehaviour;
    
    public TextMeshProUGUI WelcomeText, UndoText, WinText, UndoConfirm1, UndoConfirm2;

    public bool ShowedUndo;
    public bool ShowedWin;


    public GameBoardObject NextLevel;
    
    // Start is called before the first frame update
    void Start()
    {
        BoardBehaviour = FindObjectOfType<GameBoardBehaviour>();

        WelcomeText.alpha = 0;
        UndoText.alpha = 0;
        WinText.alpha = 0;
        UndoConfirm1.alpha = 0;
        UndoConfirm2.alpha = 0;

        StartCoroutine(Stage1());
        // DOTween.Sequence()
        //     .AppendInterval(.5f)
        //     .Append(WelcomeText.DOFade(1, .2f));
    }

    private bool _canShowConfirm;
    private bool _showedConfirm;
    
    IEnumerator Stage1()
    {
        yield return new WaitForSecondsSkippable(BoardBehaviour, .5f);
        yield return WelcomeText.DOFade(1, .2f).WaitForCompletion();

        while (BoardBehaviour.HoverStack.Count == 0) yield return null;
        
        yield return WelcomeText.DOFade(0, .2f).WaitForCompletion();
        
        yield return new WaitForSeconds(1.4f);
        yield return UndoText.DOFade(1, .4f).WaitForCompletion();

        _canShowConfirm = true;
        while (!BoardBehaviour.IsWinForAMoment) yield return null;
        
        BoardBehaviour.Flags.DisableUndo = true;
        LevelLoader.GetInstance().NextBoard = NextLevel;
        yield return UndoConfirm1.DOFade(0, .1f).WaitForCompletion();
        yield return UndoConfirm1.DOFade(0, .1f).WaitForCompletion();
        yield return UndoText.DOFade(0, .1f).WaitForCompletion();
        yield return WelcomeText.DOFade(0, .1f).WaitForCompletion();
        yield return WinText.DOFade(1, .3f).WaitForCompletion();
        WinText.transform.DOPunchScale(Vector3.one * .1f, .3f);
    }

    // Update is called once per frame
    void Update()
    {
        if (_canShowConfirm && BoardBehaviour.Swaps.Count == 0 && !_showedConfirm)
        {
            _showedConfirm = true;
            DOTween.Sequence()
                .Append(UndoText.DOFade(0, .1f))
                .Append(WelcomeText.DOFade(0, .1f))
                .AppendInterval(.4f)
                .Append(UndoConfirm1.DOFade(1, .4f))
                .AppendInterval(.3f)
                .Append(UndoConfirm2.DOFade(1, .4f))
                .AppendInterval(4.4f)
                .Append(UndoConfirm1.DOFade(0, .1f))
                .Append(UndoConfirm2.DOFade(0, .1f))
                ;
        }
        // if (!ShowedUndo && BoardBehaviour.HoverStack.Count > 0)
        // {
        //     ShowedUndo = true;
        //     DOTween.Sequence()
        //         .Append(WelcomeText.DOFade(0, .2f))
        //         .AppendInterval(.5f)
        //         .Append(UndoText.DOFade(1, .4f));
        //
        //         ;
        //
        // }
        //
        // if (!ShowedWin && BoardBehaviour.IsWin)
        // {
        //     ShowedWin = true;
        //     BoardBehaviour.Flags.DisableUndo = true;
        //     LevelLoader.GetInstance().NextBoard = NextLevel;
        //     DOTween.Sequence()
        //         .Append(UndoText.DOFade(0, .1f))
        //         .Append(WelcomeText.DOFade(0, .1f))
        //         .Append(WinText.DOFade(1, .3f))
        //         .Append(WinText.transform.DOPunchScale(Vector3.one * .1f, .3f))
        //         
        //         ;
        //
        // }
    }
}

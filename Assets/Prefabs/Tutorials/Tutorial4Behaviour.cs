using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Tutorial4Behaviour : MonoBehaviour
{
    public GameBoardBehaviour BoardBehaviour;
    public TextMeshProUGUI Hint1, ClickAnywhere, EarnMore;

    // Start is called before the first frame update

    public bool IsReadyForHint;
    public bool Showed, ShowedAccepter;
    void Start()
    {
        BoardBehaviour = FindObjectOfType<GameBoardBehaviour>();
        Hint1.alpha = 0;
        ClickAnywhere.alpha = 0;
        EarnMore.alpha = 0;

        BoardBehaviour.Flags.DisableInput = true;

        var state = FindObjectOfType<StarCounterBehaviour>().StarStateProvider.GetState();
        state.Stars = Mathf.Max(5, state.Stars);
        
        DOTween.Sequence()
            .AppendInterval(.3f)
            .Append(Hint1.DOFade(1, .4f))
            .AppendCallback(() =>
            {
                IsReadyForHint = true;
            });

    }

    // Update is called once per frame
    void Update()
    {
        if (!ShowedAccepter && BoardBehaviour.HasHint && IsReadyForHint)
        {
            ShowedAccepter = true;
            BoardBehaviour.Flags.DisableInput = false;
            DOTween.Sequence()
                .AppendInterval(.1f)
                .Append(Hint1.DOFade(0, .4f))
                .Append(ClickAnywhere.DOFade(1, .4f));
        }
        
        if (!Showed && BoardBehaviour.Swaps.Count > 0 && IsReadyForHint)
        {
            Showed = true;
            BoardBehaviour.Flags.DisableInput = false;
            DOTween.Sequence()
                .AppendInterval(.1f)
                .Append(ClickAnywhere.DOFade(0, .4f))
                .AppendInterval(.5f)
                .Append(EarnMore.DOFade(1, .4f))
                .AppendInterval(3.5f)
                .Append(EarnMore.DOFade(0, .4f))

                ;
        }   
    }
}

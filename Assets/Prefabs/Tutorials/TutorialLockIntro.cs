using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialLockIntro : MonoBehaviour
{

    public TextMeshProUGUI Hello1, Hello2, Warning1, Warning2, Lock1, Lock2;

    public GameBoardBehaviour BoardBehaviour;
    
    // Start is called before the first frame update
    void Start()
    {

        BoardBehaviour = FindObjectOfType<GameBoardBehaviour>();
        Hello1.alpha = 0;
        Hello2.alpha = 0;
        Warning1.alpha = 0;
        Warning2.alpha = 0;
        Lock1.alpha = 0;
        Lock2.alpha = 0;


        StartCoroutine(Show());
    }

    IEnumerator Show()
    {
        yield return new WaitForSecondsRealtime(.3f);

        Hello1.DOFade(1, .3f);
        yield return new WaitForSecondsSkippable(BoardBehaviour, 2f);

        Hello1.DOFade(0, .3f);
        yield return new WaitForSecondsRealtime(.1f);
        yield return Hello2.DOFade(1, .3f).WaitForCompletion();
        
        yield return new WaitForSecondsSkippable(BoardBehaviour, 2f);
        Hello2.DOFade(0, .3f);
        yield return new WaitForSecondsRealtime(.1f);
        yield return Warning1.DOFade(1, .3f).WaitForCompletion();
        yield return new WaitForSecondsSkippable(BoardBehaviour, 1.4f);

        Warning1.DOFade(0, .3f);
        yield return new WaitForSecondsRealtime(.1f);
        yield return Warning2.DOFade(1, .3f).WaitForCompletion();
        yield return new WaitForSecondsSkippable(BoardBehaviour, 3f);
        Warning2.DOFade(0, .3f);
        yield return new WaitForSecondsRealtime(.5f);

        BoardBehaviour.Flags.DisablePieces = false;
        yield return Lock1.DOFade(1, .3f).WaitForCompletion();
        BoardBehaviour.Flags.DisableRequirements = false;
        
        yield return new WaitForSecondsSkippable(BoardBehaviour, 3f);
        Lock1.DOFade(0, .3f);

        yield return new WaitForSecondsRealtime(.5f);


        BoardBehaviour.Flags.DisableInput = false;

        while (!BoardBehaviour.IsWinForAMoment) yield return null;
        Lock2.DOFade(1, .3f);

    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;

public class WaitForSecondsSkippable : CustomYieldInstruction
{
    private readonly GameBoardBehaviour _board;
    private WaitForSecondsRealtime _waiter;
    private bool skipped;
    public WaitForSecondsSkippable(GameBoardBehaviour board, float time)
    {
        _board = board;
        _waiter = new WaitForSecondsRealtime(time);
        board.OnNextInput += Listener;
    }

    void Listener()
    {
        skipped = true;
    }

    public override bool keepWaiting
    {
        get
        {
            var wait = !skipped && _waiter.keepWaiting;
            if (!wait)
            {
                _board.OnNextInput -= Listener;
            }
            return wait;
        }
    }
}

public static class DotweenExts
{
    public static YieldInstruction WaitForCompletionSkippable(this Tween t, GameBoardBehaviour board, float scale=1)
    {

        void Listener()
        {
            
            t.timeScale = t.Duration() * scale;
        }

        board.OnNextInput += Listener;
        t.OnComplete(() => board.OnNextInput -= Listener);
        
        if (t.active)
            return (YieldInstruction) DOTween.instance.StartCoroutine(WaitForCompletion(t));
        if (Debugger.logPriority > 0)
            Debugger.LogInvalidTween(t);
        return (YieldInstruction) null;
    }
    
    internal static IEnumerator WaitForCompletion(Tween t)
    {
        while (t.active && !t.IsComplete())
            yield return (object) null;
    }
}


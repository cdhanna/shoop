using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class GameObjectExtensions
{

    
    public static Coroutine DoRoutineList(this MonoBehaviour self, params CustomYieldInstruction[] instructions)
    {
        IEnumerator Func()
        {
            foreach (var ins in instructions)
            {
                yield return ins;
            }
        }

        return DoRoutine(self, Func());
    }
    
    public static Coroutine DoRoutine(this MonoBehaviour self, Func<IEnumerator> routine, Action cb = null)
    {
        return DoRoutine(self, routine(), cb);
    }
    
    public static Coroutine DoRoutine(this MonoBehaviour self, IEnumerator routine, Action cb=null)
    {
        
        IEnumerator Func()
        {
            while (routine.MoveNext())
            {
                yield return routine.Current;
            }
            cb?.Invoke();
        }
        
        return self.StartCoroutine(Func());
    }

    public static T GetRandom<T>(this List<T> rand)
    {
        return rand[Random.Range(0, rand.Count)];
    } 
}

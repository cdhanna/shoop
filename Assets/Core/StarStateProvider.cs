using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStarStateProvider
{
    IStarState GetState();
}

public class StarStateProvider : MonoBehaviour, IStarStateProvider
{
    static IStarState State = new SagamapPlayerPrefs();
    
    public IStarState GetState()
    {
        return State;
    }
}

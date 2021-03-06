using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStarStateProvider
{
    IStarState GetState();
}

public class StarStateProvider : MonoBehaviour, IStarStateProvider
{
    public static IStarState State
    {
        get
        {
            if (_storage == null) _storage = new SagamapFileStorage();
            return _storage;
        }
    }

    public static ISagaMapState SagaState
    {
        get
        {
            if (_storage == null) _storage = new SagamapFileStorage();
            return _storage;
        }
    }

    public static SagamapFileStorage _storage;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void OnReset()
    {
        _storage = new SagamapFileStorage();
    }
    
    public IStarState GetState()
    {
        return State;
    }
}

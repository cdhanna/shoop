using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStarStateProvider
{
    IStarState GetState();
}

public class StarStateProvider : MonoBehaviour, IStarStateProvider
{
    public static IStarState State => _storage;
    public static ISagaMapState SagaState => _storage;
    
    public static SagamapFileStorage _storage = new SagamapFileStorage();


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

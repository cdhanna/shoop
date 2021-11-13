using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraShakeBehaviour : MonoBehaviour
{

    public static void TryShake(float intensity, float transition = .3f)
    {
        FindObjectOfType<CameraShakeBehaviour>()?.Shake(intensity,transition); // TODO: cache this reference...
    }
    
    public CinemachineVirtualCamera Vcam;

    private CinemachineBasicMultiChannelPerlin _noiseComponent;

    private Coroutine _routine;
    
    // Start is called before the first frame update
    void Start()
    {
        _noiseComponent = Vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float intensity, float transition = .3f)
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
        
        IEnumerator Routine()
        {
            _noiseComponent.m_AmplitudeGain = intensity;

            var start = Time.realtimeSinceStartup;
            var end = start + transition;
            while (Time.realtimeSinceStartup < end)
            {
                var r = (Time.realtimeSinceStartup - start) / (end - start);
                _noiseComponent.m_AmplitudeGain = Mathf.Lerp(intensity, 0, r);
                yield return null;
            }

            _noiseComponent.m_AmplitudeGain = 0;
        }

        _routine = StartCoroutine(Routine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

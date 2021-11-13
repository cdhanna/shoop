using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningDecorBeahviour : MonoBehaviour
{

    public float MinSpeed = 1;
    public float MaxSpeed = 2;

    public bool UseRandom;
    
    public float ActualSpeed;
    public int Direction = 1;
    
    // Start is called before the first frame update
    void Start()
    {
        
        transform.eulerAngles = new Vector3(0, 0, Random.value * 360);

        if (!UseRandom) return;
        ActualSpeed = Random.Range(MinSpeed, MaxSpeed);
        Direction = Random.value > .5f ? 1 : -1;
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z + ActualSpeed * Time.deltaTime * Direction);
    }
}

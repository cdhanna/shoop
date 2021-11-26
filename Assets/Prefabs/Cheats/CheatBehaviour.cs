using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CheatBehaviour : MonoBehaviour
{

    public GameBoardBehaviour GameBoardBehaviour;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("Help!");

            var sw = new Stopwatch();
            sw.Start();
            var move = GameBoardAI.Solve(GameBoardBehaviour.GetBoardState()).FirstOrDefault();
            sw.Stop();
            Debug.Log("Found move in " + sw.ElapsedMilliseconds);
            GameBoardBehaviour.PerformSwap(move);

        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            GameBoardBehaviour.StartHintShow();
        }

    }
}

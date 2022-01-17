using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class GeneralLevelHelloGroup
{
    public List<MenuButtonBehaviour> Texts;
    public float InterDelay = .3f;
    public float GroupDelay;
    public float Duration;
}

public class GeneralLevelHelloBehaviour : MonoBehaviour
{
    public List<GeneralLevelHelloGroup> Groups;
    public int Index;

    public GameBoardBehaviour Board;
    
    // Start is called before the first frame update
    void Start()
    {
        Board = FindObjectOfType<GameBoardBehaviour>();
        foreach (var group in Groups)
        {
            foreach (var text in group.Texts)
            {
                text.Text.alpha = 0;
            }
        }
        
        StartCoroutine(Show());
    }

    IEnumerator Show()
    {
        while (Index < Groups.Count)
        {
            if (Board.IsWin || Board.IsOver) break;
            
            var group = Groups[Index];
            yield return new WaitForSecondsRealtime(group.GroupDelay);
            
            foreach (var text in group.Texts)
            {
                text.Text.DOFade(1, .4f);
                yield return new WaitForSecondsRealtime(group.InterDelay);

            }
            yield return new WaitForSecondsRealtime(group.Duration);
            foreach (var text in group.Texts)
            {
                text.Text.DOFade(0, .4f);
                yield return new WaitForSecondsRealtime(group.InterDelay);

            }

            Index++;
        }
        
        foreach (var group in Groups)
        {
            foreach (var text in group.Texts)
            {
                text.Text.alpha = 0;
            }
        }

    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}

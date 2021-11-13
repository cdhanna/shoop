using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DumbMenuController : MonoBehaviour
{

    public List<GameBoardObject> Levels;

    public List<GameBoardGeneratorObject> Generators;
    
    public Button ButtonPrefab;
    public RectTransform ButtonContainer;
    
    
    
    // Start is called before the first frame update
    void Start()
    {

        for (var i = 0; i < ButtonContainer.childCount; i++)
        {
            Destroy(ButtonContainer.GetChild(i).gameObject);
        }

        foreach (var lvl in Levels)
        {
            var button = Instantiate(ButtonPrefab, ButtonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = lvl.name;
            button.onClick.AddListener(() =>
            {
                LevelLoader.GetInstance().LoadLevel(lvl);
            });
        }

        foreach (var gen in Generators)
        {
            var button = Instantiate(ButtonPrefab, ButtonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = $"{gen.name} Random";
            button.onClick.AddListener(() =>
            {
                LevelLoader.GetInstance().LoadRandomLevel(gen);
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

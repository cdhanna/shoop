using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DumbMenuController : MonoBehaviour
{

    public List<GameBoardObject> Levels;

    public List<GameBoardGeneratorObject> Generators;
    public List<SagaMap> SagaMaps;
    
    public Button ButtonPrefab;
    public RectTransform ButtonContainer;

    public SagaMap Sagamap;
    public TextMeshProUGUI PlayText;
    public Button PlayButton;
    public Button SettingsButton;
    public Button SettingsBackButotn;
    
    public CanvasGroup CanvasGroup;

    public GameObject DebugMenu;

    public bool isHome = true;
    public List<MenuTransitionBehaviour> TransitionBehaviours;
    
    
    // Start is called before the first frame update
    void Start()
    {
        CanvasGroup.alpha = 0;
        CanvasGroup.DOFade(1, .4f);

        PlayButton.onClick.AddListener(() =>
        {

            CanvasGroup.DOFade(0, .6f);
            TransitionHelperBehaviour.Instance.TransitionAndDoThing(() => LevelLoader.GetInstance().LoadSagamap(Sagamap));
            // LevelLoader.GetInstance().LoadSagamap(Sagamap);
        });
        
        SettingsButton.onClick.AddListener(() =>
        {
            isHome = !isHome;
            if (isHome)
            {
                TransitionBehaviours.ForEach(t => t.GotoHome());
            }
            else
            {
                TransitionBehaviours.ForEach(t => t.GotoSettings());
            }
            
        });
        SettingsBackButotn.onClick.AddListener(() =>
        {
            isHome = !isHome;
            if (isHome)
            {
                TransitionBehaviours.ForEach(t => t.GotoHome());
            }
            else
            {
                TransitionBehaviours.ForEach(t => t.GotoSettings());
            }
        });
        
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

        foreach (var map in SagaMaps)
        {
            var button = Instantiate(ButtonPrefab, ButtonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = $"{map.name} Saga";
            button.onClick.AddListener(() =>
            {
                LevelLoader.GetInstance().LoadSagamap(map);
            });
            var button2 = Instantiate(ButtonPrefab, ButtonContainer);
            button2.GetComponentInChildren<TextMeshProUGUI>().text = $"{map.name} Saga (CLEAR STATE)";
            button2.onClick.AddListener(() =>
            {
                LevelLoader.GetInstance().ClearSagamap(map);
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            DebugMenu.SetActive(true);
        }
    }
}

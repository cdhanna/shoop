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

    public TextMeshProUGUI LevelDetailText;
    
    public Button PlayButton;
    public Button SettingsButton;
    public Button SettingsBackButotn;

    public MenuButtonBehaviour PreviousLevelButton, NextLevelButton;
    public MenuButtonBehaviour[] Stars;
    public HorizontalLayoutGroup LevelLayoutGroup;
    
    public CanvasGroup CanvasGroup;

    public GameObject DebugMenu;

    public bool isHome = true;
    public List<MenuTransitionBehaviour> TransitionBehaviours;
    
    
    // Start is called before the first frame update
    void Start()
    {
        CanvasGroup.alpha = 0;
        CanvasGroup.DOFade(1, .4f);
        // CanvasGroup.
        Canvas.ForceUpdateCanvases();
        UpdateLevelText();
        
        PreviousLevelButton.Button.onClick.AddListener(() =>
        {
            StarStateProvider.SagaState.PreviousLevel(Sagamap);
            UpdateLevelText();
        });
        
        NextLevelButton.Button.onClick.AddListener(() =>
        {
            StarStateProvider.SagaState.NextLevel(Sagamap);
            UpdateLevelText();
        });
        
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

    public void UpdateLevelText()
    {
        
        var levelIndex = StarStateProvider.SagaState.GetLevelIndex(Sagamap);
        var highest = StarStateProvider.SagaState.GetHighestLevelIndex(Sagamap);


        if (levelIndex == highest)
        {
            foreach (var star in Stars)
            {
                star.Image.DOFade(0, .2f);
            }
        }
        else
        {
            var starCount = StarStateProvider.SagaState.GetBestStarsForLevel(Sagamap, levelIndex);
            for (var i = 0; i < Stars.Length; i++)
            {
                var isLit = starCount >= (i + 1);
                Stars[i].Image.DOFade(isLit ? 1 : .3f, .2f);
            }
        }
        

        if (levelIndex == 0)
        {
            // cannot go previous
            PreviousLevelButton.Button.interactable = false;
            PreviousLevelButton.Image.DOFade(0, .2f);
            // DOTween.To(() => PreviousLevelButton.NoisePower, v => PreviousLevelButton.NoisePower = v, 0, .2f);
        } else if (!PreviousLevelButton.Button.interactable)
        {
            PreviousLevelButton.Button.interactable = true;
            PreviousLevelButton.Image.DOFade(1, .2f);

            // DOTween.To(() => PreviousLevelButton.NoisePower, v => PreviousLevelButton.NoisePower = v, .5f, .2f);

        }

        if (highest == levelIndex)
        {
            NextLevelButton.Button.interactable = false;
            // DOTween.To(() => NextLevelButton.NoisePower, v => NextLevelButton.NoisePower = v, 0, .2f);
            NextLevelButton.Image.DOFade(0, .2f);
        } else if (!NextLevelButton.Button.interactable)
        {
            NextLevelButton.Button.interactable = true;
            // DOTween.To(() => NextLevelButton.NoisePower, v => NextLevelButton.NoisePower = v, .5f, .2f);
            NextLevelButton.Image.DOFade(1, .2f);

        }
        
        var text = (levelIndex + 1).ToString("000");
        LevelDetailText.text = text;
    }
    
}

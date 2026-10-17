//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;

public class TitleScreen : ScreenBase
{
    [SerializeField] private Button _titleButton;
    [FormerlySerializedAs("_testButton")]
    [SerializeField] private Button _deckBuildButton;
    private TransitionService _transitionService;

    [VContainer.Inject]
    public void Construct(TransitionService transitionService) 
    {
        _transitionService = transitionService;
    }

    private void Start()
    {
        _titleButton.onClick.AddListener(() =>
        {
            //_transitionService.BattleStart().Forget();
            _transitionService.StageSelectScreen().Forget();
        });

        _deckBuildButton.onClick.AddListener(() =>
        {
            _transitionService.DeckBuildScreen().Forget();
        });
    }
    public void PopExit()
    {
       
    }

    public void PushEnter()
    {
   
    }

    private void OnDestroy()
    {
        AssetLoader.Release(ResourceKey.Prefabs.TitleScreen);
    }
}

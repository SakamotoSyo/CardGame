//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class TitleScreen : ScreenBase
{
    [SerializeField] private Button _titleButton;
    [SerializeField] private Button _testButton;
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
            _transitionService.MapSelectScreen().Forget();
        });

        _testButton.onClick.AddListener(() =>
        {
            _transitionService.TestScreen().Forget();
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

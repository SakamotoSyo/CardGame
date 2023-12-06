//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class TitleScreen : MonoBehaviour, IScreenLifeTimeCycle
{
    [SerializeField] private Button _titleButton; 
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
            _transitionService.BattleStart().Forget();
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

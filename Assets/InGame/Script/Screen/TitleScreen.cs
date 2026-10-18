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
            Transition(_transitionService.StageSelectScreen).Forget();
        });

        _deckBuildButton.onClick.AddListener(() =>
        {
            Transition(_transitionService.DeckBuildScreen).Forget();
        });
    }

    /// <summary>
    /// 連打で遷移が重ならないよう、遷移中はボタンを押せなくする。失敗したら押せるように戻す
    /// </summary>
    private async UniTask Transition(System.Func<UniTask> transition)
    {
        if (!_titleButton.interactable || !_deckBuildButton.interactable) return;
        SetButtonsInteractable(false);
        try
        {
            await transition();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            if (this != null) SetButtonsInteractable(true);
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        _titleButton.interactable = interactable;
        _deckBuildButton.interactable = interactable;
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

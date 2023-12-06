//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using VContainer;

public sealed class TransitionService : IInitializable
{
    private ScreenContainer _mainContainer => ScreenContainer.Find("MainContainer");

    [Inject]
    public TransitionService()
    {
    }

    public void Initialize()
    {

    }

    public async UniTask BattleStart()
    {
        _mainContainer.Pop(1);
        await _mainContainer.Push(ResourceKey.Prefabs.MainBattleScreen);
        //var obj = AssetLoader.AssetSynchronousLoad<GameObject>();
    }

    public async UniTask TitleScreen() 
    {
        await _mainContainer.Push(ResourceKey.Prefabs.TitleScreen);
    }
}

//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using VContainer;

public sealed class TransitionService : IInitializable
{
    [Inject]
    public TransitionService()
    {
    }

    public void Initialize()
    {

    }

    public void BattleStart()
    {
        var obj = AssetLoader.AssetSynchronousLoad<GameObject>(ResourceKey.Prefabs.MainBattleScreen);
        Object.Instantiate(obj, MainUi.MainUiTransform);
        AssetLoader.Release(ResourceKey.Prefabs.MainBattleScreen);
    }
}

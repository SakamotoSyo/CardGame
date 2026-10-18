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
        await ReplaceScreen(ResourceKey.Prefabs.MainBattleScreen);
        //var obj = AssetLoader.AssetSynchronousLoad<GameObject>();
    }

    public async UniTask TitleScreen()
    {
        await _mainContainer.Push(ResourceKey.Prefabs.TitleScreen);
    }

    public async UniTask StageSelectScreen()
    {
        await ReplaceScreen(ResourceKey.Prefabs.StageSelectScreen);
    }

    public async UniTask BackToTitleScreen()
    {
        await ReplaceScreen(ResourceKey.Prefabs.TitleScreen);
    }

    public async UniTask MapSelectScreen()
    {
        await ReplaceScreen(ResourceKey.Prefabs.MapSelectScreen);
    }

    public async UniTask DeckBuildScreen()
    {
        await ReplaceScreen(ResourceKey.Prefabs.DeckBuildScreen);
    }

    public async UniTask ShopScreen()
    {
        await ReplaceScreen(ResourceKey.Prefabs.ShopScreen);
    }

    /// <summary>
    /// 今の画面を閉じて次の画面を開く
    /// 次の画面の準備ができてから閉じるので、途中で失敗しても今の画面は残り、呼び出し元で立て直せる
    /// </summary>
    private async UniTask ReplaceScreen(string resourceKey)
    {
        await _mainContainer.Replace(resourceKey);
    }
}

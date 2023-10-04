//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using VContainer;

/// <summary>
/// バトルの始まりを管理するスクリプト
/// </summary>
public sealed class ApplicationCompositionRoot : LifetimeScope
{
    [SerializeField] private MainUi _mainUi;
    protected override void Configure(IContainerBuilder builder)
    {
        //Actor
        builder.Register<EnemyStatus>(Lifetime.Singleton);
        builder.Register<IPlayerStatus, PlayerStatus>(Lifetime.Singleton);
        builder.Register<ActorGenerator>(Lifetime.Singleton);

        builder.Register<PlayCardFieldPresenter>(Lifetime.Singleton);

        builder.Register<TestScript>(Lifetime.Singleton);
        builder.Register<TransitionService>(Lifetime.Singleton);

        builder.RegisterComponent(_mainUi);

        builder.RegisterEntryPoint<EntryPoint>(Lifetime.Singleton);

    }

    private void Start()
    {
        //SetUp().Forget();

    }

    private async UniTask SetUp() 
    {
        await MasterData.Instance.Setup();
    }
}

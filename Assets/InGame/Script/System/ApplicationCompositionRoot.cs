//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using VContainer;

/// <summary>
/// 
/// </summary>
public sealed class ApplicationCompositionRoot : LifetimeScope
{
    [SerializeField] private MainUi _mainUi;
    [SerializeField] private FadeScript _fadeScript;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IDataLoader, MasterData>(Lifetime.Singleton);

        ///Ripository
        builder.Register<IEnemyDataRepository, EnemyDataRepositoryMock>(Lifetime.Singleton);
        builder.Register<ICardDataRepository, CardDataRepository>(Lifetime.Singleton);
        builder.Register<IEffectDataRepository, EffectDataRepository>(Lifetime.Singleton);

        //Actor
        builder.Register<IPlayerStatus, PlayerStatus>(Lifetime.Singleton);
        builder.Register<ActorGenerator>(Lifetime.Singleton);

        builder.Register<TransitionService>(Lifetime.Singleton);

        builder.RegisterComponent(_mainUi);
       // builder.RegisterComponent(_fadeScript);

        builder.RegisterEntryPoint<EntryPoint>(Lifetime.Singleton);
    }
}

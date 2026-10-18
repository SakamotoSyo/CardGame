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
    [Tooltip("デッキのルール。デッキ編集画面と戦闘の両方で使う")]
    [SerializeField] private DeckBuildSettings _deckBuildSettings;
    [Tooltip("ガチャの値段・排出率とランで手に入るガチャ石。ショップと戦闘の両方で使う")]
    [SerializeField] private GachaSettings _gachaSettings;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IDataLoader, MasterData>(Lifetime.Singleton);
        builder.RegisterInstance(_deckBuildSettings);
        builder.RegisterInstance(_gachaSettings);

        ///Ripository
        builder.Register<IEnemyDataRepository, EnemyDataRepositoryMock>(Lifetime.Singleton);
        builder.Register<ICardDataRepository, CardDataRepository>(Lifetime.Singleton);
        builder.Register<IEffectDataRepository, EffectDataRepository>(Lifetime.Singleton);
        builder.Register<IDungeonRunRepository, DungeonRunRepository>(Lifetime.Singleton);
        builder.Register<IDeckRepository, DeckRepository>(Lifetime.Singleton);
        builder.Register<IWalletRepository, WalletRepository>(Lifetime.Singleton);
        builder.Register<ICardCollectionRepository, CardCollectionRepository>(Lifetime.Singleton);

        //Actor
        builder.Register<IPlayerStatus, PlayerStatus>(Lifetime.Singleton);
        builder.Register<ActorGenerator>(Lifetime.Singleton);

        builder.Register<TransitionService>(Lifetime.Singleton);

        builder.RegisterComponent(_mainUi);
       // builder.RegisterComponent(_fadeScript);

        builder.RegisterEntryPoint<EntryPoint>(Lifetime.Singleton);
    }
}

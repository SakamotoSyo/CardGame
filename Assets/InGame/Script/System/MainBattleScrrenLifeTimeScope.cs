//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainBattleScrrenLifeTimeScope : LifetimeScope
{
    [SerializeField] private ActorGenerator _actorGenerator;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<BattleEnviroment>(Lifetime.Scoped);
        builder.RegisterComponent(_actorGenerator);
        builder.Register<IBattleTurnController, BattleTurnController>(Lifetime.Scoped);
        builder.Register<TestScript>(Lifetime.Scoped);
    }
}

//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainBattleScrrenLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<CardEnviroment>(Lifetime.Scoped);
        builder.Register<TestScript>(Lifetime.Scoped);
    }
}

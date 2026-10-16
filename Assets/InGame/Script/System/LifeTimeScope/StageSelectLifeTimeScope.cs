//日本語対応
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class StageSelectLifeTimeScope : LifetimeScope
{
    [SerializeField] private StageMapSettings _stageMapSettings;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_stageMapSettings);
        builder.Register<StageMapGenerator>(Lifetime.Scoped);
    }
}

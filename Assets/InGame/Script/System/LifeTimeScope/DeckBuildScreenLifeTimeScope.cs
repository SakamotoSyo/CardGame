//日本語対応
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class DeckBuildScreenLifeTimeScope : LifetimeScope
{
    [SerializeField] private DeckBuildSettings _deckBuildSettings;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_deckBuildSettings);
    }
}

//日本語対応
using VContainer;
using VContainer.Unity;

/// <summary>
/// デッキのルール(DeckBuildSettings)は戦闘でも使うのでApplicationCompositionRootで登録している
/// </summary>
public class DeckBuildScreenLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
    }
}

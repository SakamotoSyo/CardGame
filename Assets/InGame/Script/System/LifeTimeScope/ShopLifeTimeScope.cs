//日本語対応
using VContainer;
using VContainer.Unity;

/// <summary>
/// ガチャの設定(GachaSettings)は戦闘の報酬でも使うのでApplicationCompositionRootで登録している
/// </summary>
public class ShopLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<GachaService>(Lifetime.Scoped);
    }
}

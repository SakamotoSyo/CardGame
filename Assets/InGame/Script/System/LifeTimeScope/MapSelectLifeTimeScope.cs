//日本語対応
using VContainer;
using VContainer.Unity;

public class MapSelectLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IDungeonMapDataRepository, DungeonMapDataRepositoryMock>(Lifetime.Singleton);
        builder.Register<IMapSelectPosRepository, MapSelectPosRepositoryMock>(Lifetime.Singleton);
    }
}

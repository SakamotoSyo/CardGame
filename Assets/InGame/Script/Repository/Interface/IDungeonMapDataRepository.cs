//日本語対応
using Cysharp.Threading.Tasks;

public interface IDungeonMapDataRepository
{
    public UniTask LoadData();

    public DungeonMapData GetValue();

    public void Update(DungeonMapData dungeonData);
}

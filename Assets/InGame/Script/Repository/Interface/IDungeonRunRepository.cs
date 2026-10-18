//日本語対応
using Cysharp.Threading.Tasks;

/// <summary>
/// 進行中のラン(日数・現在地・マップ)を保存・読み込みする
/// </summary>
public interface IDungeonRunRepository
{
    /// <summary>進行中のラン。無ければnull</summary>
    public DungeonRunData Current { get; }

    public UniTask LoadData();
    /// <returns>保存に成功したらtrue。進行中のランが無いときもtrue</returns>
    public UniTask<bool> Save();
    public void StartNewRun(DungeonRunData runData);

    /// <summary>ゲームオーバー時などにランを破棄する</summary>
    public UniTask DeleteRun();
}

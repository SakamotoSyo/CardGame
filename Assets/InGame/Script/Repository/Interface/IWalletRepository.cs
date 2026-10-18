//日本語対応
using Cysharp.Threading.Tasks;

/// <summary>
/// ガチャ石を保存・読み込みする
/// </summary>
public interface IWalletRepository
{
    public int GachaStone { get; }
    /// <summary>ガチャ石の所持数が変わったら、変わった後の数で呼ばれる</summary>
    public event System.Action<int> OnGachaStoneChanged;
    /// <summary>まだタイトルで表示していない、直前に終わったランの報酬。無ければnull</summary>
    public RunReward PendingRunReward { get; }

    public UniTask LoadData();
    /// <returns>保存に成功したらtrue。失敗したときは所持数も変わらない</returns>
    public UniTask<bool> AddStone(int amount);
    /// <returns>足りていて保存にも成功したらtrue。それ以外は所持数も変わらない</returns>
    public UniTask<bool> TrySpendStone(int amount);
    /// <summary>ランの報酬を受け取り、タイトルで表示できるよう覚えておく</summary>
    public UniTask<bool> ReceiveRunReward(RunReward reward);
    /// <summary>報酬を表示し終えたら呼ぶ</summary>
    public void ClearPendingRunReward();
}

//日本語対応
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// ガチャ石をローカルのJsonに保存する
/// </summary>
public class WalletRepository : IWalletRepository
{
    public const string FileName = "WalletData.json";

    /// <summary>増やす・減らすが重なっても数がずれないよう、変更と保存を1つずつ実行する</summary>
    private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
    /// <summary>読み込み中に再度呼ばれても、同じ読み込みの完了を待つ</summary>
    private UniTask? _loadTask;
    private WalletData _data = new WalletData();

    public int GachaStone => _data.GachaStone;
    public event System.Action<int> OnGachaStoneChanged;
    public RunReward PendingRunReward { get; private set; }

    public async UniTask LoadData()
    {
        _loadTask ??= LoadInternal().Preserve();
        await _loadTask.Value;
    }

    private async UniTask LoadInternal()
    {
        _data = await LocalData.LoadAsyncData<WalletData>(FileName) ?? new WalletData();
    }

    public async UniTask<bool> AddStone(int amount)
    {
        if (amount < 0) return false;
        return await Change(amount);
    }

    public async UniTask<bool> TrySpendStone(int amount)
    {
        if (amount < 0) return false;
        return await Change(-amount);
    }

    public async UniTask<bool> ReceiveRunReward(RunReward reward)
    {
        if (!await AddStone(reward.GachaStone)) return false;
        PendingRunReward = reward;
        return true;
    }

    public void ClearPendingRunReward()
    {
        PendingRunReward = null;
    }

    /// <summary>
    /// 所持数を変えて保存する。足りないときや保存に失敗したときは元のまま
    /// </summary>
    private async UniTask<bool> Change(int amount)
    {
        await LoadData();
        await _fileLock.WaitAsync();
        try
        {
            var next = _data.GachaStone + amount;
            if (next < 0) return false;
            var saveData = new WalletData { GachaStone = next };
            if (!await LocalData.SaveAsyncData(FileName, saveData)) return false;
            _data = saveData;
        }
        finally
        {
            _fileLock.Release();
        }
        //通知先で再び増減しても待ち合わせで止まらないよう、ロックを外してから知らせる
        OnGachaStoneChanged?.Invoke(_data.GachaStone);
        return true;
    }
}

//日本語対応
using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ガチャ石をローカルのJsonに保存する
/// </summary>
public class WalletRepository : IWalletRepository
{
    public const string FileName = "WalletData.json";
    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    /// <summary>増やす・減らすが重なっても数がずれないよう、変更と保存を1つずつ実行する</summary>
    private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
    /// <summary>読み込み中に再度呼ばれても、同じ読み込みの完了を待つ</summary>
    private UniTask? _loadTask;
    private WalletData _data = new WalletData();

    public int GachaStone => _data.GachaStone;
    public event System.Action<int> OnGachaStoneChanged;
    public RunReward PendingRunReward { get; private set; }

    /// <exception cref="IOException">保存済みのファイルを読み込めなかったとき。次に呼ばれたら読み込み直す</exception>
    public async UniTask LoadData()
    {
        _loadTask ??= LoadInternal().Preserve();
        try
        {
            await _loadTask.Value;
        }
        catch
        {
            _loadTask = null;
            throw;
        }
    }

    private async UniTask LoadInternal()
    {
        var data = await LocalData.LoadAsyncData<WalletData>(FileName);
        //ファイルはあるのに読めないときに0個として扱うと、次の保存で持っていたガチャ石を上書きして失ってしまう
        if (data == null && File.Exists(FilePath)) throw new IOException($"{FileName}を読み込めませんでした");
        _data = data ?? new WalletData();
    }

    public async UniTask<bool> AddStone(int amount)
    {
        if (amount < 0) return false;
        return await Change(amount) == ChangeResult.Changed;
    }

    public async UniTask<bool> TrySpendStone(int amount)
    {
        if (amount < 0) return false;
        return await Change(-amount) == ChangeResult.Changed;
    }

    public async UniTask<bool> ReceiveRunReward(RunReward reward)
    {
        var result = reward.GachaStone >= 0 ? await Change(reward.GachaStone, reward.RunId) : ChangeResult.Failed;
        //すでに受け取っていたら石は増えていないので、タイトルで「+N」と見せない
        if (result == ChangeResult.AlreadyRewarded) return true;
        reward.IsReceived = result == ChangeResult.Changed;
        PendingRunReward = reward;
        return reward.IsReceived;
    }

    public void SetUnreceivedRunReward(RunReward reward)
    {
        reward.IsReceived = false;
        PendingRunReward = reward;
    }

    public void ClearPendingRunReward()
    {
        PendingRunReward = null;
    }

    /// <summary>
    /// 所持数を変えて保存する。足りないときや読み込み・保存に失敗したときは元のまま
    /// </summary>
    /// <param name="rewardedRunId">ランの報酬として受け取るときのランのId。同じランの報酬は2回受け取らない</param>
    private async UniTask<ChangeResult> Change(int amount, string rewardedRunId = null)
    {
        try
        {
            await LoadData();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return ChangeResult.Failed;
        }

        await _fileLock.WaitAsync();
        try
        {
            //ランの破棄に失敗して同じランでもう一度負けても、報酬を重ねて渡さない
            if (!string.IsNullOrEmpty(rewardedRunId) && rewardedRunId == _data.LastRewardedRunId) return ChangeResult.AlreadyRewarded;
            var next = _data.GachaStone + amount;
            if (next < 0) return ChangeResult.Failed;
            var saveData = new WalletData
            {
                GachaStone = next,
                LastRewardedRunId = string.IsNullOrEmpty(rewardedRunId) ? _data.LastRewardedRunId : rewardedRunId,
            };
            if (!await LocalData.SaveAsyncData(FileName, saveData)) return ChangeResult.Failed;
            _data = saveData;
        }
        finally
        {
            _fileLock.Release();
        }
        //通知先で再び増減しても待ち合わせで止まらないよう、ロックを外してから知らせる
        OnGachaStoneChanged?.Invoke(_data.GachaStone);
        return ChangeResult.Changed;
    }

    private enum ChangeResult
    {
        Changed,
        /// <summary>同じランの報酬をすでに受け取っていたので変えなかった</summary>
        AlreadyRewarded,
        Failed,
    }
}

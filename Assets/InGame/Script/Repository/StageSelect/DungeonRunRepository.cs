//日本語対応
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ランの進行状況をローカルのJsonに保存する
/// </summary>
public class DungeonRunRepository : IDungeonRunRepository
{
    public const string FileName = "DungeonRunData.json";
    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    /// <summary>同じファイルへの書き込みが重なってJsonが壊れないよう、保存と削除を1つずつ実行する</summary>
    private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
    /// <summary>読み込み中に再度呼ばれても、同じ読み込みの完了を待つ</summary>
    private UniTask? _loadTask;

    public DungeonRunData Current { get; private set; }

    public async UniTask LoadData()
    {
        _loadTask ??= LoadInternal().Preserve();
        await _loadTask.Value;
    }

    private async UniTask LoadInternal()
    {
        Current = await LocalData.LoadAsyncData<DungeonRunData>(FileName);
    }

    public async UniTask<bool> Save()
    {
        if (Current == null) return true;
        await _fileLock.WaitAsync();
        try
        {
            //待っている間に破棄されていたら保存しない
            if (Current == null) return true;
            return await LocalData.SaveAsyncData(FileName, Current);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public void StartNewRun(DungeonRunData runData)
    {
        Current = runData;
    }

    public async UniTask DeleteRun()
    {
        Current = null;
        await _fileLock.WaitAsync();
        try
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}

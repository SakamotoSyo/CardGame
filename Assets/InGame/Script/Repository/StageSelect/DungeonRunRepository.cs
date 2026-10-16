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

    /// <summary>同じファイルへの書き込みが重なってJsonが壊れないよう、保存と削除を1つずつ実行する</summary>
    private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
    private bool _isLoaded;

    public DungeonRunData Current { get; private set; }

    public async UniTask LoadData()
    {
        if (_isLoaded) return;
        _isLoaded = true;
        Current = await LocalData.LoadAsyncData<DungeonRunData>(FileName);
    }

    public async UniTask Save()
    {
        if (Current == null) return;
        await _fileLock.WaitAsync();
        try
        {
            //待っている間に破棄されていたら保存しない
            if (Current == null) return;
            await LocalData.SaveAsyncData(FileName, Current);
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
            var path = Path.Combine(Application.persistentDataPath, FileName);
            if (File.Exists(path)) File.Delete(path);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}

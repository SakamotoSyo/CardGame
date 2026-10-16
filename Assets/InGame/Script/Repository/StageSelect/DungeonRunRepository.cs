//日本語対応
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ランの進行状況をローカルのJsonに保存する
/// </summary>
public class DungeonRunRepository : IDungeonRunRepository
{
    public const string FileName = "DungeonRunData.json";

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
        await LocalData.SaveAsyncData(FileName, Current);
    }

    public void StartNewRun(DungeonRunData runData)
    {
        Current = runData;
    }

    public UniTask DeleteRun()
    {
        Current = null;
        var path = Path.Combine(Application.persistentDataPath, FileName);
        if (File.Exists(path)) File.Delete(path);
        return UniTask.CompletedTask;
    }
}

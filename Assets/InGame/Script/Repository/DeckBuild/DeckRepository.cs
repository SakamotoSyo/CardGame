//日本語対応
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// デッキをローカルのJsonに保存する
/// </summary>
public class DeckRepository : IDeckRepository
{
    public const string FileName = "DeckData.json";

    /// <summary>同じファイルへの書き込みが重なってJsonが壊れないよう、保存を1つずつ実行する</summary>
    private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
    /// <summary>読み込み中に再度呼ばれても、同じ読み込みの完了を待つ</summary>
    private UniTask? _loadTask;

    public DeckData Current { get; private set; }

    public async UniTask LoadData()
    {
        _loadTask ??= LoadInternal().Preserve();
        await _loadTask.Value;
    }

    private async UniTask LoadInternal()
    {
        //保存より前に読み込みが終わっていなかった場合に、古い内容で上書きしないようにする
        var loaded = await LocalData.LoadAsyncData<DeckData>(FileName);
        Current ??= loaded;
    }

    public async UniTask Save(DeckData deckData)
    {
        //呼び出し元が後からリストを書き換えても保存内容が変わらないよう複製して持つ
        Current = new DeckData { CardIds = new List<int>(deckData.CardIds) };
        var saveData = Current;
        await _fileLock.WaitAsync();
        try
        {
            await LocalData.SaveAsyncData(FileName, saveData);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}

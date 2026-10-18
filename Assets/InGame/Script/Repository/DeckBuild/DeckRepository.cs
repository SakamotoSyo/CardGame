//日本語対応
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

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

    private readonly ICardDataRepository _cardDataRepository;
    private readonly DeckBuildSettings _settings;

    public DeckData Current { get; private set; }

    public DeckRepository(ICardDataRepository cardDataRepository, DeckBuildSettings settings)
    {
        _cardDataRepository = cardDataRepository;
        _settings = settings;
    }

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

    public async UniTask<bool> Save(DeckData deckData)
    {
        //呼び出し元が後からリストを書き換えても保存内容が変わらないよう複製して持つ
        var saveData = new DeckData { CardIds = new List<int>(deckData.CardIds) };
        await _fileLock.WaitAsync();
        try
        {
            //ファイルに書けなかったときは、メモリ上のデッキもファイルと同じ内容のままにする
            if (!await LocalData.SaveAsyncData(FileName, saveData)) return false;
            Current = saveData;
            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public List<int> GetSavedCardIds()
    {
        return Current?.CardIds == null ? null : ExistingCards(Current.CardIds);
    }

    public async UniTask<List<int>> LoadInitialCardIds()
    {
        //AssetLoaderのキャッシュは他の画面と共有なので使わず、自分のハンドルで読んで写したらすぐ解放する
        var handle = Addressables.LoadAssetAsync<InitialPlayerStatus>(ResourceKey.MasterData.PlayerInitStatus);
        try
        {
            var initialStatus = await handle;
            return ExistingCards(initialStatus.CardId);
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    public bool IsPlayable(IReadOnlyList<int> cardIds)
    {
        if (cardIds == null) return false;
        if (cardIds.Count < _settings.MinDeckCount || _settings.MaxDeckCount < cardIds.Count) return false;
        return cardIds.GroupBy(id => id).All(g => g.Count() <= _settings.MaxSameCardCount);
    }

    public List<int> ResolveBattleDeck(IReadOnlyList<int> initialCardIds)
    {
        var saved = GetSavedCardIds();
        if (saved != null && IsPlayable(saved)) return saved;

        if (saved != null)
        {
            UnityEngine.Debug.LogWarning("保存したデッキがルールを満たしていないため、初期デッキを使います");
        }
        return ExistingCards(initialCardIds);
    }

    /// <summary>マスタから消えたカードを除く</summary>
    private List<int> ExistingCards(IEnumerable<int> cardIds)
    {
        var result = new List<int>();
        foreach (var id in cardIds)
        {
            if (_cardDataRepository.FindById(id) == null)
            {
                UnityEngine.Debug.LogWarning($"Id:{id}のカードがマスタにないためデッキから外します");
                continue;
            }
            result.Add(id);
        }
        return result;
    }
}

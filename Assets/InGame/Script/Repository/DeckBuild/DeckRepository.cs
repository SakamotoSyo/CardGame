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
    private readonly ICardCollectionRepository _collectionRepository;
    private readonly DeckBuildSettings _settings;

    public DeckData Current { get; private set; }

    public DeckRepository(ICardDataRepository cardDataRepository,
        ICardCollectionRepository collectionRepository,
        DeckBuildSettings settings)
    {
        _cardDataRepository = cardDataRepository;
        _collectionRepository = collectionRepository;
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
        return ExistingCards(await LoadRawInitialCardIds());
    }

    /// <summary>
    /// 初期デッキ(InitialPlayerStatusの札)をそのまま読む。マスタにないカードも含む
    /// </summary>
    public static async UniTask<List<int>> LoadRawInitialCardIds()
    {
        //AssetLoaderのキャッシュは他の画面と共有なので使わず、自分のハンドルで読んで写したらすぐ解放する
        var handle = Addressables.LoadAssetAsync<InitialPlayerStatus>(ResourceKey.MasterData.PlayerInitStatus);
        try
        {
            var initialStatus = await handle;
            return new List<int>(initialStatus.CardId);
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    public bool IsPlayable(IReadOnlyList<int> cardIds)
    {
        return MeetsRules(cardIds) && IsOwned(cardIds);
    }

    /// <summary>枚数と同じカードの枚数がルールを満たしていればtrue</summary>
    private bool MeetsRules(IReadOnlyList<int> cardIds)
    {
        if (cardIds == null) return false;
        if (cardIds.Count < _settings.MinDeckCount || _settings.MaxDeckCount < cardIds.Count) return false;
        return cardIds.GroupBy(id => id).All(g => g.Count() <= _settings.MaxSameCardCount);
    }

    /// <summary>どのカードも所持している枚数以内ならtrue</summary>
    private bool IsOwned(IReadOnlyList<int> cardIds)
    {
        return cardIds.GroupBy(id => id).All(g => g.Count() <= _collectionRepository.GetCount(g.Key));
    }

    public async UniTask<List<int>> ResolveBattleDeck()
    {
        await LoadData();
        var saved = GetSavedCardIds();
        var isCollectionLoaded = true;
        try
        {
            await _collectionRepository.LoadData();
        }
        catch (System.Exception e)
        {
            //所持カードを読めなくても戦闘はできるよう、そのときはルールだけで判定する
            UnityEngine.Debug.LogException(e);
            isCollectionLoaded = false;
        }
        if (saved != null && (isCollectionLoaded ? IsPlayable(saved) : MeetsRules(saved))) return saved;

        if (saved != null)
        {
            UnityEngine.Debug.LogWarning("保存したデッキがルールを満たしていないため、初期デッキを使います");
        }
        var initial = await LoadInitialCardIds();
        //初期デッキは他に代わりが無いのでそのまま使うが、ルールを満たしていなければ設定の見直しを促す
        if (!MeetsRules(initial))
        {
            UnityEngine.Debug.LogError($"初期デッキがデッキのルールを満たしていません(現在{initial.Count}枚)。InitialPlayerStatusの札かDeckBuildSettingsを見直してください");
        }
        return initial;
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

//日本語対応
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 所持カードをローカルのJsonに保存する
/// </summary>
public class CardCollectionRepository : ICardCollectionRepository
{
    public const string FileName = "CardCollectionData.json";
    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    /// <summary>同じファイルへの書き込みが重なってJsonが壊れないよう、変更と保存を1つずつ実行する</summary>
    private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
    /// <summary>読み込み中に再度呼ばれても、同じ読み込みの完了を待つ</summary>
    private UniTask? _loadTask;
    private readonly Dictionary<int, int> _counts = new Dictionary<int, int>();

    private readonly IDeckRepository _deckRepository;

    public CardCollectionRepository(IDeckRepository deckRepository)
    {
        _deckRepository = deckRepository;
    }

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
        var data = await LocalData.LoadAsyncData<CardCollectionData>(FileName);
        //ファイルはあるのに読めないときに作り直すと、ガチャで手に入れたカードを上書きして失ってしまう
        if (data == null && File.Exists(FilePath)) throw new IOException($"{FileName}を読み込めませんでした");
        if (data != null)
        {
            foreach (var card in data.Cards)
            {
                if (card.Count > 0) _counts[card.Id] = card.Count;
            }
            return;
        }

        //所持カードの仕組みより前から遊んでいても組んだデッキが使えなくならないよう、
        //初期デッキと保存済みのデッキのうち多い方の枚数を所持済みにする
        await _deckRepository.LoadData();
        AddMax(await _deckRepository.LoadInitialCardIds());
        AddMax(_deckRepository.GetSavedCardIds());
        await LocalData.SaveAsyncData(FileName, ToData(_counts));
    }

    private void AddMax(IEnumerable<int> cardIds)
    {
        if (cardIds == null) return;
        foreach (var group in cardIds.GroupBy(id => id))
        {
            _counts[group.Key] = System.Math.Max(GetCount(group.Key), group.Count());
        }
    }

    public int GetCount(int cardId)
    {
        return _counts.TryGetValue(cardId, out var count) ? count : 0;
    }

    public async UniTask<bool> AddCards(IReadOnlyList<int> cardIds)
    {
        try
        {
            await LoadData();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
        await _fileLock.WaitAsync();
        try
        {
            //保存に失敗したときに所持数を戻せるよう、複製に足してから保存する
            var next = new Dictionary<int, int>(_counts);
            foreach (var id in cardIds)
            {
                next[id] = (next.TryGetValue(id, out var count) ? count : 0) + 1;
            }
            if (!await LocalData.SaveAsyncData(FileName, ToData(next))) return false;

            _counts.Clear();
            foreach (var pair in next) _counts[pair.Key] = pair.Value;
            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private static CardCollectionData ToData(Dictionary<int, int> counts)
    {
        return new CardCollectionData
        {
            Cards = counts.OrderBy(p => p.Key).Select(p => new OwnedCard { Id = p.Key, Count = p.Value }).ToList()
        };
    }
}

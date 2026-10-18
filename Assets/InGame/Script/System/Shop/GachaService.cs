//日本語対応
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

/// <summary>
/// ガチャを引く。ガチャ石を払い、出たカードを所持カードに加える
/// </summary>
public sealed class GachaService
{
    private readonly ICardDataRepository _cardDataRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ICardCollectionRepository _collectionRepository;
    private readonly GachaSettings _settings;

    [Inject]
    public GachaService(ICardDataRepository cardDataRepository,
        IWalletRepository walletRepository,
        ICardCollectionRepository collectionRepository,
        GachaSettings settings)
    {
        _cardDataRepository = cardDataRepository;
        _walletRepository = walletRepository;
        _collectionRepository = collectionRepository;
        _settings = settings;
    }

    /// <summary>
    /// レアリティごとの出る確率(%)。マスタにカードが無いレアリティは含めない
    /// </summary>
    public IReadOnlyList<(int Rare, float Percent)> GetRarityRates()
    {
        var rares = _cardDataRepository.GetAll().Select(c => c.Rare).Distinct().OrderBy(r => r).ToList();
        var total = rares.Sum(r => _settings.GetRarityWeight(r));
        if (total <= 0f) return new List<(int, float)>();
        return rares.Select(r => (r, _settings.GetRarityWeight(r) / total * 100f)).ToList();
    }

    /// <param name="count">引く回数</param>
    /// <param name="cost">払うガチャ石</param>
    /// <returns>出たカード。石が足りないときや保存に失敗したときはnull</returns>
    public async UniTask<List<GachaResult>> Draw(int count, int cost)
    {
        var cards = _cardDataRepository.GetAll();
        if (count <= 0 || cards.Count == 0) return null;
        await _collectionRepository.LoadData();

        if (!await _walletRepository.TrySpendStone(cost)) return null;

        var drawn = new List<CardData>();
        for (int i = 0; i < count; i++) drawn.Add(Pick(cards));

        //初めて手に入れたカードは、同じガチャで2枚目以降が出てもNEWにしない
        var known = new HashSet<int>(cards.Where(c => _collectionRepository.GetCount(c.Id) > 0).Select(c => c.Id));
        var results = new List<GachaResult>();
        foreach (var card in drawn)
        {
            results.Add(new GachaResult(card, known.Add(card.Id)));
        }

        if (!await _collectionRepository.AddCards(drawn.Select(c => c.Id).ToList()))
        {
            //カードを渡せなかったので、払ったガチャ石を返す
            Debug.LogError("所持カードを保存できなかったため、ガチャ石を返します");
            if (!await _walletRepository.AddStone(cost))
            {
                Debug.LogError($"ガチャ石{cost}個を返せませんでした");
            }
            return null;
        }
        return results;
    }

    /// <summary>
    /// レアリティの重みで抽選し、同じレアリティの中からは同じ確率で選ぶ
    /// </summary>
    private CardData Pick(IReadOnlyList<CardData> cards)
    {
        //重みが0のレアリティは出さない
        var groups = cards.GroupBy(c => c.Rare).Where(g => _settings.GetRarityWeight(g.Key) > 0f).ToList();
        if (groups.Count == 0) return RandomOf(cards);

        var value = Random.value * groups.Sum(g => _settings.GetRarityWeight(g.Key));
        foreach (var group in groups)
        {
            var weight = _settings.GetRarityWeight(group.Key);
            if (value < weight) return RandomOf(group.ToList());
            value -= weight;
        }
        //Random.valueが1のときはここに来るので、最後のレアリティにする
        return RandomOf(groups[groups.Count - 1].ToList());
    }

    private static CardData RandomOf(IReadOnlyList<CardData> cards)
    {
        return cards[Random.Range(0, cards.Count)];
    }
}

public sealed class GachaResult
{
    public CardData Card { get; }
    /// <summary>初めて手に入れたカードならtrue</summary>
    public bool IsNew { get; }

    public GachaResult(CardData card, bool isNew)
    {
        Card = card;
        IsNew = isNew;
    }
}

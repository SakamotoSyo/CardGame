//日本語対応
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class CardDataRepository : ICardDataRepository
{
    private Dictionary<int, CardData> _cardData = new Dictionary<int, CardData>();

    private IEffectDataRepository _effectMasterTable;

    [Inject]
    public CardDataRepository(IEffectDataRepository effectDataRepository) 
    {
        _effectMasterTable = effectDataRepository;
    }

    public async UniTask LoadData(IDataLoader loader)
    {
        var cards = await loader.LoadMasterData<CardDataTable>("Card");

        foreach (var d in cards.Data)
        {
            CardData card = new CardData();
            card.Id = d.Id;
            card.Name = d.Name;
            card.Rare = d.Rare;
            card.Description = d.Description;
            card.Cost = d.Cost;

            card.Effect = _effectMasterTable.EffectTable.FindById(d.EffectId.ToString()).EffectList;
            var effectPowerArray = d.EffectPower.Split(',');
            for (int i = 0; i < effectPowerArray.Length; i++) 
            {
                card.Effect[i].EffectPower = int.Parse(effectPowerArray[i]);
            }

            _cardData.Add(d.Id, card);
        }
    }

    public CardData FindById(int id)
    {
        return !_cardData.TryGetValue(id, out var cardData) ? null : cardData;
    }
}

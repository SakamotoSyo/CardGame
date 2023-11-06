//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

public interface IPlayerStatus
{
    public IReadOnlyReactiveProperty<float> Cost { get;}
    public IReadOnlyReactiveProperty<float> Gold { get;}
    public IReactiveProperty<float> Defence { get; }
    public IReactiveProperty<float> MaxHp { get; }
    public IReactiveProperty<float> CurrentHp { get; }
    public IReactiveProperty<float> Attack { get; }
    public IReadOnlyReactiveCollection<CardData> HandCardList { get; }
    public IReadOnlyReactiveCollection<CardData> DeckCardList { get; }
    public IReadOnlyReactiveCollection<CardData> GraveyardCards { get; }

    public UniTask SetUp();

    public void DrawCard(float num);
    public bool UseCost(float num);
}

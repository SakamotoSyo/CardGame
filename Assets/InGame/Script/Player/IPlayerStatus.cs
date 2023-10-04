//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

public interface IPlayerStatus
{
    public IReactiveProperty<float> Cost { get;}
    public IReactiveProperty<float> Gold { get;}
    public IReactiveProperty<float> Defence { get; }
    public IReactiveProperty<float> MaxHp { get; }
    public IReactiveProperty<float> CurrentHp { get; }
    public IReactiveProperty<float> Attack { get; }
    public IReactiveCollection<CardData> HandCardList { get; }
    public IReactiveCollection<CardData> DeckCardList { get; }
    public IReactiveCollection<CardData> GraveyardCards { get; }

    public UniTask SetUp();

    public void DrawCard(float num);
    //public void Dispose();


}

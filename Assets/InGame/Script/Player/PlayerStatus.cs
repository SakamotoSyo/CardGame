using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.AddressableAssets;

public sealed class PlayerStatus : StatusModelBase, IPlayerStatus
{
    public float MaxCost => _maxCost;
    private float _maxCost;
    [Tooltip("カードに使用の際に必要なコスト")]
    public IReactiveProperty<float> Cost => _cost;
    private readonly ReactiveProperty<float> _cost = new();
    public IReactiveProperty<float> Gold => _gold;
    private readonly ReactiveProperty<float> _gold = new();

    public IReactiveCollection<CardData> HandCardList => _handCardList;
    [Tooltip("手札のカードリスト")]
    private readonly ReactiveCollection<CardData> _handCardList = new();

    public IReactiveCollection<CardData> DeckCardList => _deckCardList;
    [Tooltip("山札のカードリスト")]
    private readonly ReactiveCollection<CardData> _deckCardList = new();

    public IReactiveCollection<CardData> GraveyardCards => _graveyardCards;
    [Tooltip("捨て札を貯めておくList")]
    private readonly ReactiveCollection<CardData> _graveyardCards = new();

    public async UniTask SetUp()
    {
        var saveData = await LocalData.LoadAsyncData<PlayerStatus>("PlayerStatus");

        //セーブデータがなかった場合初期札を読み込む
        if (saveData != null)
        {
            LoadPlayerData(saveData);
        }
        else
        {
            Debug.Log("ステータスロード中");
            var data = await Addressables.LoadAssetAsync<InitialPlayerStatus>(ResourceKey.MasterData.PlayerInitStatus);
            SetInitPlayerStatus(data);
        }
    }

    public void DeckInit()
    {

    }

    /// <summary>
    /// カードをドローする
    /// </summary>
    public void DrawCard(float num = 1)
    {
        for (int i = 0; i < num; i++)
        {
            if (_deckCardList.Count == 0 && _graveyardCards.Count != 0)
            {
                //Drawするカードがなくなった時捨て札を山札に戻す
                for (int j = 0; j < _graveyardCards.Count; j++)
                {
                    _deckCardList.Add(_graveyardCards[j]);
                }
                _graveyardCards.Clear();
                _handCardList.Add(_deckCardList[0]);
            }
            else if (_deckCardList.Count == 0 && _graveyardCards.Count == 0)
            {
                Debug.LogWarning("山札に戻すカードはありません");
            }
            else
            {
                _handCardList.Add(_deckCardList[0]);
                _deckCardList.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// 手札をすべて捨てる
    /// </summary>
    public void DiscardAllHandCards()
    {
        for (int i = 0; i < _handCardList.Count; i++)
        {
            _graveyardCards.Add(_handCardList[i]);
        }
        _handCardList.Clear();
    }

    /// <summary>
    /// カードを捨て札に加える処理
    /// </summary>
    /// <param name="cardBase"></param>
    public void GraveyardCardsAdd(CardData cardBase)
    {
        _graveyardCards.Add(cardBase);
        _handCardList.Remove(cardBase);
    }

    //TODO:この関数いらないかも
    /// <summary>
    /// コストを使用する
    /// </summary>
    /// <param name="useCost"></param>
    public bool UseCost(float useCost)
    {
        if (useCost <= _cost.Value)
        {
            _cost.Value -= useCost;
            return true;
        }
        return false;
    }

    public void ResetCost()
    {
        _cost.Value = _maxCost;
    }

    public bool UseGold(float useGold)
    {
        if (useGold <= _gold.Value)
        {
            _gold.Value -= useGold;
            return true;
        }

        return false;
    }

    protected override void DisposeInternal()
    {
        base.DisposeInternal();
        _cost.Dispose();
        _gold.Dispose();
        _handCardList.Dispose();
        _deckCardList.Dispose();
        _graveyardCards.Dispose();
    }

    /// <summary>
    /// 初期ステータスをセットする関数
    /// </summary>
    /// <param name="playerStatus"></param>
    public void SetInitPlayerStatus(InitialPlayerStatus playerStatus)
    {
        _maxHp.Value = playerStatus.MaxHp;
        _currentHp.Value = playerStatus.MaxHp;
        _maxCost = playerStatus.MaxCost;
        _cost.Value = _maxCost;

        for (int i = 0; i < playerStatus.CardId.Count; i++)
        {
            //カードの読み込み
            var loadData = MasterData.Instance.CardData[playerStatus.CardId[i]];
            var cardData = new CardData();
            cardData.ReflectsLoadCardData(loadData);
            var effectData = MasterData.Instance.EffectMasterTable.FindById(playerStatus.CardId[i].ToString());
            cardData.Effect = effectData.EffectList[0];
            _deckCardList.Add(cardData);
        }

    }

    /// <summary>
    /// セーブデータをロードしてステータスにセットする
    /// </summary>
    /// <param name="playerData"></param>
    public void LoadPlayerData(PlayerStatus playerData)
    {
        _maxHp.Value = playerData.MaxHp.Value;
        _currentHp.Value = playerData.CurrentHp.Value;
        _cost.Value = playerData.Cost.Value;
        _maxCost = playerData.MaxCost;
        _gold.Value = playerData.Gold.Value;
        _deckCardList.CopyTo(playerData.DeckCardList.ToArray(), 0);
    }
}

using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.AddressableAssets;
using VContainer;

public sealed class PlayerStatus : StatusModelBase, IPlayerStatus
{
    public float MaxCost => _maxCost;
    private float _maxCost;
    [Tooltip("カードに使用の際に必要なコスト")]
    public IReadOnlyReactiveProperty<float> Cost => _cost;
    private readonly ReactiveProperty<float> _cost = new();
    public IReadOnlyReactiveProperty<float> Gold => _gold;
    private readonly ReactiveProperty<float> _gold = new();

    public IReadOnlyReactiveCollection<CardData> HandCardList => _handCardList;
    [Tooltip("手札のカードリスト")]
    private readonly ReactiveCollection<CardData> _handCardList = new();

    public IReadOnlyReactiveCollection<CardData> DeckCardList => _deckCardList;
    [Tooltip("山札のカードリスト")]
    private readonly ReactiveCollection<CardData> _deckCardList = new();

    public IReadOnlyReactiveCollection<CardData> GraveyardCards => _graveyardCards;
    [Tooltip("捨て札を貯めておくList")]
    private readonly ReactiveCollection<CardData> _graveyardCards = new();

    private ICardDataRepository _cardDataRepository;
    private IDeckRepository _deckRepository;

    [Inject]
    public PlayerStatus(ICardDataRepository cardDataRepository, IDeckRepository deckRepository) 
    {
        _cardDataRepository = cardDataRepository;
        _deckRepository = deckRepository;
    }

    public async UniTask SetUp()
    {
        //Singletonなので、前の戦闘の札が残らないよう空にしてから読み込む
        _handCardList.Clear();
        _deckCardList.Clear();
        _graveyardCards.Clear();

        var saveData = await LocalData.LoadAsyncData<PlayerStatus>("PlayerStatus");

        //セーブデータがなかった場合初期札を読み込む
        if (saveData != null)
        {
            LoadPlayerData(saveData);
        }
        else
        {
            Debug.Log("ステータスロード中");
            var handle = Addressables.LoadAssetAsync<InitialPlayerStatus>(ResourceKey.MasterData.PlayerInitStatus);
            try
            {
                var data = await handle;
                //デッキ編集画面で保存したデッキがあれば初期札の代わりに使う
                await _deckRepository.LoadData();
                SetInitPlayerStatus(data, _deckRepository.Current?.CardIds);
            }
            finally
            {
                //値はステータスに写し終えているので、読み込んだアセットはすぐ解放する
                Addressables.Release(handle);
            }
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

        var count = _handCardList.Count;
        for (int i = 0; i < count; i++) 
        {
            _handCardList.RemoveAt(0);
        }
        
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
    /// <param name="deckCardIds">保存したデッキ。nullなら初期ステータスの札を使う</param>
    public void SetInitPlayerStatus(InitialPlayerStatus playerStatus, IReadOnlyList<int> deckCardIds = null)
    {
        _maxHp.Value = playerStatus.MaxHp;
        _currentHp.Value = playerStatus.MaxHp;
        _maxCost = playerStatus.MaxCost;
        _cost.Value = _maxCost;

        AddDeckCards(deckCardIds ?? playerStatus.CardId);

        //保存したデッキのカードがすべてマスタから消えていたら、山札が空にならないよう初期ステータスの札を使う
        if (_deckCardList.Count == 0 && deckCardIds != null)
        {
            Debug.LogWarning("保存したデッキに使えるカードがないため、初期の札を使います");
            AddDeckCards(playerStatus.CardId);
        }
    }

    private void AddDeckCards(IReadOnlyList<int> cardIds)
    {
        for (int i = 0; i < cardIds.Count; i++)
        {
            //カードの読み込み
            var loadData = _cardDataRepository.FindById(cardIds[i]);
            //マスタから消えたカードは山札に入れない
            if (loadData == null)
            {
                Debug.LogWarning($"Id:{cardIds[i]}のカードがマスタにないため山札に入れません");
                continue;
            }
            _deckCardList.Add(loadData);
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

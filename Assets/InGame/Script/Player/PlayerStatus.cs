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
        //JsonUtilityはコンストラクタを通さずに作るので、ReactivePropertyがnullのままになる。読めなかったものとして初期ステータスを使う
        if (saveData != null && saveData.MaxHp == null)
        {
            Debug.LogWarning("PlayerStatusのセーブデータを読み込めないため、初期ステータスを使います");
            saveData = null;
        }

        Debug.Log("ステータスロード中");
        var handle = Addressables.LoadAssetAsync<InitialPlayerStatus>(ResourceKey.MasterData.PlayerInitStatus);
        try
        {
            var data = await handle;
            await _deckRepository.LoadData();

            //セーブデータがなかった場合初期ステータスを読み込む
            if (saveData != null) LoadPlayerData(saveData);
            else SetInitPlayerStatus(data);

            //セーブに山札が無ければ、デッキ編集画面で保存したデッキ(使えなければ初期札)を山札にする
            if (_deckCardList.Count == 0)
            {
                AddDeckCards(_deckRepository.ResolveBattleDeck(data.CardId));
            }
        }
        finally
        {
            //値はステータスに写し終えているので、読み込んだアセットはすぐ解放する
            Addressables.Release(handle);
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
    public void SetInitPlayerStatus(InitialPlayerStatus playerStatus)
    {
        _maxHp.Value = playerStatus.MaxHp;
        _currentHp.Value = playerStatus.MaxHp;
        _maxCost = playerStatus.MaxCost;
        _cost.Value = _maxCost;
    }

    private void AddDeckCards(IEnumerable<int> cardIds)
    {
        foreach (var id in cardIds)
        {
            //カードの読み込み
            var loadData = _cardDataRepository.FindById(id);
            //マスタから消えたカードは山札に入れない
            if (loadData == null)
            {
                Debug.LogWarning($"Id:{id}のカードがマスタにないため山札に入れません");
                continue;
            }
            //同じカードが複数枚あっても別の1枚として扱えるよう複製して入れる
            _deckCardList.Add(loadData.Clone());
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
        //Jsonから戻したカードには効果が入っていないので、Idからマスタのカードを引き直す
        if (playerData.DeckCardList != null) AddDeckCards(playerData.DeckCardList.Where(c => c != null).Select(c => c.Id));
    }
}

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public sealed class PlayerView : ActorViewBase
{
    [SerializeField] private Text _costText;
    [SerializeField] private Text _discardedText;
    [SerializeField] private Text _deckText;
    [SerializeField] private GameObject _cardParentObj;
    [SerializeField] private Animator _costEffectAnim;
    [SerializeField] private Text _costEffectText;
    [SerializeField] private Button _turnEndButton;
    [SerializeField] private GridLayoutGroup _layoutGroup;

    private List<CardController> _handController = new();
    private GameObject _cardPrefab;
    private IBattleTurnController _battleTurnController;

    public async UniTask SetUp(IBattleTurnController battleTurnController) 
    {
        _cardPrefab = await AssetLoader.LoadAssetAsync<GameObject>(ResourceKey.Prefabs.BattleCard);
        _battleTurnController = battleTurnController;
        _turnEndButton.onClick.AddListener(NextTurn);
    }

    public void DrawView(CardData card)
    {
        var obj = Instantiate(_cardPrefab, _cardParentObj.transform);
        var cardController = obj.GetComponent<CardController>();
        _handController.Add(cardController);
        cardController.SetCardData(card);

        _layoutGroup.CalculateLayoutInputVertical();
        _layoutGroup.CalculateLayoutInputHorizontal();
        _layoutGroup.SetLayoutHorizontal();
        _layoutGroup.SetLayoutVertical();
        var cardView = obj.GetComponent<CardView>();
        cardView.DrawAnim();
    }

    public void ThrowHandCard(CardData cardData) 
    {
        for (int i = 0; i < _handController.Count; i++) 
        {
            if (_handController[i].CardData.Value == cardData) 
            {
                _handController[i].ThrowCard();
                _handController.Remove(_handController[i]);
                break;
            }
        }
    }

    public void GraveyardCardsView(int count)
    {
        _discardedText.text = count.ToString();
    }

    public void DeckCardView(int count)
    {
        _deckText.text = count.ToString();
    }

    public async UniTask SetCostText(float cost)
    {
        var token = this.GetCancellationTokenOnDestroy();
        _costText.text = cost.ToString();
        if (int.Parse(_costText.text) < cost)
        {
            _costEffectText.enabled = true;
            _costEffectText.text = (cost - int.Parse(_costText.text)).ToString("0");
            _costText.text = cost.ToString();
            _costEffectAnim.SetTrigger("CostEffect");
            await UniTask.WaitUntil(() => _costEffectAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f, cancellationToken: token);
            _costEffectText.enabled = false;
        }
        else
        {
            _costText.text = cost.ToString();
        }
    }

    private void NextTurn() 
    {
        if (_battleTurnController.CurrentState.ToString() == "PlayerTurnState") 
        {
            _battleTurnController.ChangeState(BattleTurnController.TurnStateType.SelectNextActorTransitionState);
        }
    }
}

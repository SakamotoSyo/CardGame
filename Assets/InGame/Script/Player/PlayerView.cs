using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public sealed class PlayerView : ActorViewBase
{
    [SerializeField] private Text _costText;
    [SerializeField] private Text _discardedText;
    [SerializeField] private Text _deckText;
    [SerializeField] private GameObject _cardParentObj;
    [SerializeField] private Animator _costEffectAnim;
    [SerializeField] private Text _costEffectText;
    [SerializeField] private MyLayoutGroup _layoutGroup;

    public void DrawView(CardData card)
    {
        var addressableHandle = Addressables.LoadAssetAsync<GameObject>(ResourceKey.Prefabs.Card);
        var cardPrefab = addressableHandle.WaitForCompletion();
        var obj = Instantiate(cardPrefab, _cardParentObj.transform.position, Quaternion.identity, _cardParentObj.transform);
        Addressables.Release(addressableHandle);

        var cardController = obj.GetComponent<CardController>();
        cardController.SetCardBaseClass(card);

        var cardView = obj.GetComponent<CardView>();
        _layoutGroup.AddChild(obj.transform);
        cardView.DrawAnim(obj.transform);
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
        _costText.text = cost.ToString();
        if (int.Parse(_costText.text) < cost)
        {
            var token = this.GetCancellationTokenOnDestroy();
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
}

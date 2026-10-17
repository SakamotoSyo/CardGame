//日本語対応
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デッキ編集画面
/// 左の所持カードをタップするとデッキに追加し、右のデッキのカードをタップすると外す。保存したデッキは次の戦闘から山札になる
/// </summary>
public class DeckBuildScreen : ScreenBase
{
    [Header("一覧")]
    [Tooltip("所持カードを並べる場所")]
    [SerializeField] private RectTransform _ownedContent;
    [Tooltip("デッキのカードを並べる場所")]
    [SerializeField] private RectTransform _deckContent;
    [SerializeField] private DeckCardView _cardTemplate;

    [Header("ヘッダー")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Text _deckCountText;
    [SerializeField] private Text _hintText;
    [SerializeField] private Color _countNormalColor = Color.white;
    [SerializeField] private Color _countErrorColor = new Color(1f, 0.45f, 0.4f);

    [Header("演出")]
    [SerializeField] private Image _fadeImage;

    [Header("保存せずに戻るときの確認")]
    [SerializeField] private CanvasGroup _popup;
    [SerializeField] private Text _popupBodyText;
    [SerializeField] private Button _popupYesButton;
    [SerializeField] private Button _popupNoButton;

    private readonly Dictionary<int, DeckCardView> _ownedViews = new Dictionary<int, DeckCardView>();
    private readonly Dictionary<int, DeckCardView> _deckViews = new Dictionary<int, DeckCardView>();
    /// <summary>編集中のデッキ(カードのId)</summary>
    private readonly List<int> _deck = new List<int>();
    /// <summary>最後に保存したデッキ。編集中のデッキと違えば未保存</summary>
    private readonly List<int> _savedDeck = new List<int>();

    private TransitionService _transitionService;
    private ICardDataRepository _cardDataRepository;
    private IDeckRepository _deckRepository;
    private DeckBuildSettings _settings;
    private bool _isBusy = true;
    private UniTaskCompletionSource<bool> _popupClosed;

    private bool IsDirty => !_deck.SequenceEqual(_savedDeck);

    [VContainer.Inject]
    public void Construct(TransitionService transitionService,
        ICardDataRepository cardDataRepository,
        IDeckRepository deckRepository,
        DeckBuildSettings settings)
    {
        _transitionService = transitionService;
        _cardDataRepository = cardDataRepository;
        _deckRepository = deckRepository;
        _settings = settings;
    }

    private void Start()
    {
        _popup.gameObject.SetActive(false);
        _cardTemplate.gameObject.SetActive(false);
        _fadeImage.color = Color.black;
        _fadeImage.raycastTarget = true;
        _backButton.onClick.AddListener(() => BackToTitle().Forget());
        _saveButton.onClick.AddListener(() => Save().Forget());
        _popupYesButton.onClick.AddListener(() => _popupClosed?.TrySetResult(true));
        _popupNoButton.onClick.AddListener(() => _popupClosed?.TrySetResult(false));
        SetUp().Forget();
    }

    private async UniTask SetUp()
    {
        try
        {
            await LoadDeck();
            CreateViews();
            Refresh();
        }
        catch (System.Exception e)
        {
            //読み込みに失敗しても暗転したまま操作できなくならないよう、タイトルへ戻れる状態にする
            Debug.LogException(e);
            _hintText.text = "デッキの読み込みに失敗しました。タイトルへ戻ってください";
            _saveButton.interactable = false;
        }

        await _fadeImage.DOFade(0f, 0.4f).SetLink(gameObject).ToUniTask();
        _fadeImage.raycastTarget = false;
        _isBusy = false;
    }

    /// <summary>
    /// 保存したデッキを読み込む。まだ保存していなければ初期デッキから始める
    /// </summary>
    private async UniTask LoadDeck()
    {
        await _deckRepository.LoadData();
        IEnumerable<int> cardIds = _deckRepository.Current?.CardIds;
        if (cardIds == null)
        {
            var initialStatus = await AssetLoader.LoadAssetAsync<InitialPlayerStatus>(ResourceKey.MasterData.PlayerInitStatus);
            cardIds = initialStatus.CardId;
        }

        //マスタから消えたカードは外しておく
        _deck.AddRange(cardIds.Where(id => _cardDataRepository.FindById(id) != null));
        _deck.Sort();
        _savedDeck.AddRange(_deck);
    }

    private void CreateViews()
    {
        foreach (var card in _cardDataRepository.GetAll())
        {
            _ownedViews.Add(card.Id, CreateView(card, _ownedContent, AddCard));
            _deckViews.Add(card.Id, CreateView(card, _deckContent, RemoveCard));
        }
    }

    private DeckCardView CreateView(CardData card, RectTransform parent, System.Action<DeckCardView> onClick)
    {
        var view = Instantiate(_cardTemplate, parent);
        view.gameObject.SetActive(true);
        view.name = $"Card_{card.Id}";
        view.SetCard(card);
        view.OnClick += onClick;
        return view;
    }

    private void AddCard(DeckCardView view)
    {
        if (_isBusy || !CanAdd(view.Card.Id)) return;
        _deck.Add(view.Card.Id);
        _deck.Sort();
        Refresh();
        _deckViews[view.Card.Id].Punch();
    }

    private void RemoveCard(DeckCardView view)
    {
        if (_isBusy || !_deck.Remove(view.Card.Id)) return;
        Refresh();
    }

    private bool CanAdd(int cardId)
    {
        return _deck.Count < _settings.MaxDeckCount && CountOf(cardId) < _settings.MaxSameCardCount;
    }

    private int CountOf(int cardId)
    {
        return _deck.Count(id => id == cardId);
    }

    private void Refresh()
    {
        foreach (var pair in _ownedViews)
        {
            var count = CountOf(pair.Key);
            pair.Value.SetState($"{count}/{_settings.MaxSameCardCount}", CanAdd(pair.Key));
        }
        foreach (var pair in _deckViews)
        {
            var count = CountOf(pair.Key);
            pair.Value.gameObject.SetActive(count > 0);
            pair.Value.SetState($"×{count}", true);
        }

        var isEnough = _deck.Count >= _settings.MinDeckCount;
        _deckCountText.text = $"デッキ {_deck.Count} / {_settings.MaxDeckCount}枚";
        _deckCountText.color = isEnough ? _countNormalColor : _countErrorColor;
        _saveButton.interactable = isEnough && IsDirty;

        if (!isEnough)
        {
            _hintText.text = $"あと{_settings.MinDeckCount - _deck.Count}枚入れると保存できます(最低{_settings.MinDeckCount}枚)";
        }
        else if (_deck.Count >= _settings.MaxDeckCount)
        {
            _hintText.text = "デッキの上限です。カードを外すと追加できます";
        }
        else
        {
            _hintText.text = $"左のカードをタップでデッキに追加、右のカードをタップで外す(同じカードは{_settings.MaxSameCardCount}枚まで)";
        }
    }

    private async UniTask Save()
    {
        if (_isBusy || _deck.Count < _settings.MinDeckCount) return;
        _isBusy = true;
        try
        {
            await _deckRepository.Save(new DeckData { CardIds = new List<int>(_deck) });
            _savedDeck.Clear();
            _savedDeck.AddRange(_deck);
            Refresh();
            _hintText.text = "デッキを保存しました。次の戦闘から使われます";
            _deckCountText.rectTransform.DOKill(true);
            _deckCountText.rectTransform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.8f).SetLink(gameObject);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async UniTask BackToTitle()
    {
        if (_isBusy) return;
        _isBusy = true;

        if (IsDirty)
        {
            var isDiscard = await ShowPopup("デッキが保存されていません。\n保存せずにタイトルへ戻りますか？");
            if (!isDiscard)
            {
                _isBusy = false;
                return;
            }
        }

        _fadeImage.raycastTarget = true;
        await _fadeImage.DOFade(1f, 0.3f).SetLink(gameObject).ToUniTask();
        await _transitionService.BackToTitleScreen();
    }

    /// <returns>「はい」を押したらtrue</returns>
    private async UniTask<bool> ShowPopup(string body)
    {
        _popupBodyText.text = body;
        _popup.gameObject.SetActive(true);
        _popup.alpha = 0f;
        _popup.transform.localScale = Vector3.one * 0.9f;
        DOTween.Sequence()
            .Join(_popup.DOFade(1f, 0.2f))
            .Join(_popup.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
            .SetLink(gameObject);

        _popupClosed = new UniTaskCompletionSource<bool>();
        var result = await _popupClosed.Task;
        await _popup.DOFade(0f, 0.15f).SetLink(gameObject).ToUniTask();
        _popup.gameObject.SetActive(false);
        return result;
    }

    private void OnDestroy()
    {
        AssetLoader.Release(ResourceKey.MasterData.PlayerInitStatus);
        AssetLoader.Release(ResourceKey.Prefabs.DeckBuildScreen);
    }
}

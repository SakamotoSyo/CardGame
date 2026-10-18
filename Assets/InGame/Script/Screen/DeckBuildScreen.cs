//日本語対応
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// <summary>
/// デッキ編集画面
/// 左のカード一覧からデッキに追加し、右のデッキ編成一覧で枚数を増減する。保存したデッキは次の戦闘から山札になる
/// </summary>
public class DeckBuildScreen : ScreenBase
{
    [Header("カード一覧")]
    [SerializeField] private RectTransform _tileContent;
    [SerializeField] private DeckCardTileView _tileTemplate;
    [Tooltip("絞り込みでカードが1枚もないときに出す")]
    [SerializeField] private GameObject _emptyText;

    [Header("絞り込み")]
    [SerializeField] private InputField _keywordInput;
    [Tooltip("先頭が「全て」、続けてコスト0から順。最後のボタンはそれ以上のコスト")]
    [SerializeField] private Button[] _costFilterButtons;
    [SerializeField] private Color _filterSelectedColor = new Color(0.85f, 0.6f, 0.15f);
    [SerializeField] private Color _filterNormalColor = new Color(0.25f, 0.2f, 0.25f);

    [Header("デッキ編成")]
    [SerializeField] private RectTransform _rowContent;
    [SerializeField] private DeckListRowView _rowTemplate;
    [SerializeField] private DeckCostCurveView _costCurve;
    [SerializeField] private Text _deckCountText;
    [SerializeField] private Text _deckRuleText;
    [Tooltip("分類ごとの枚数")]
    [SerializeField] private Text _typeText;
    [SerializeField] private GameObject _deckEmptyText;
    [SerializeField] private Color _countNormalColor = Color.white;
    [SerializeField] private Color _countErrorColor = new Color(1f, 0.45f, 0.4f);

    [Header("ヘッダー")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _clearButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Text _hintText;

    [Header("カード詳細")]
    [SerializeField] private DeckCardDetailView _detail;

    [Header("演出")]
    [SerializeField] private Image _fadeImage;

    [Header("確認のポップアップ")]
    [SerializeField] private CanvasGroup _popup;
    [SerializeField] private Text _popupBodyText;
    [SerializeField] private Button _popupYesButton;
    [SerializeField] private Text _popupYesText;
    [SerializeField] private Button _popupNoButton;
    [SerializeField] private Text _popupNoText;

    private readonly Dictionary<int, DeckCardTileView> _tiles = new Dictionary<int, DeckCardTileView>();
    private readonly Dictionary<int, DeckListRowView> _rows = new Dictionary<int, DeckListRowView>();
    /// <summary>キーワード検索に使う、名前とタグを除いた説明文。入力のたびに作り直さないよう一度だけ作る</summary>
    private readonly Dictionary<int, string> _searchTexts = new Dictionary<int, string>();
    /// <summary>編集中のデッキ(カードのId)。コスト順に並べる</summary>
    private readonly List<int> _deck = new List<int>();
    /// <summary>最後に保存したデッキ。編集中のデッキと違えば未保存</summary>
    private readonly List<int> _savedDeck = new List<int>();

    private TransitionService _transitionService;
    private ICardDataRepository _cardDataRepository;
    private IDeckRepository _deckRepository;
    private DeckBuildSettings _settings;
    private bool _isBusy = true;
    /// <summary>デッキと一覧を作り終えたらtrue。読み込みに失敗したときは編集させない</summary>
    private bool _isLoaded;
    /// <summary>絞り込むコストの列。-1なら全て</summary>
    private int _costFilter = -1;
    private UniTaskCompletionSource<bool> _popupClosed;

    private bool CanEdit => _isLoaded && !_isBusy;
    private bool IsDirty => _isLoaded && !_deck.SequenceEqual(_savedDeck);

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
        _detail.gameObject.SetActive(false);
        _tileTemplate.gameObject.SetActive(false);
        _rowTemplate.gameObject.SetActive(false);
        _fadeImage.color = Color.black;
        _fadeImage.raycastTarget = true;
        ValidateCostColumns();
        BindView();
        SetUp().Forget();
    }

    /// <summary>
    /// コストの列数は設定(MaxCostColumn)に合わせる。ボタンや棒の数がずれていたら絞り込みとグラフが食い違うので知らせる
    /// </summary>
    private void ValidateCostColumns()
    {
        var columnCount = _settings.MaxCostColumn + 1;
        if (_costFilterButtons.Length != columnCount + 1)
        {
            Debug.LogError($"コストの絞り込みボタンは「全て」+{columnCount}個必要です(現在{_costFilterButtons.Length}個)");
        }
        if (_costCurve.ColumnCount != columnCount)
        {
            Debug.LogError($"コスト分布の棒は{columnCount}本必要です(現在{_costCurve.ColumnCount}本)");
        }
    }

    private void BindView()
    {
        _backButton.onClick.AddListener(() => BackToTitle().Forget());
        _saveButton.onClick.AddListener(() => Save().Forget());
        _clearButton.onClick.AddListener(() => ClearDeck().Forget());
        _popupYesButton.onClick.AddListener(() => _popupClosed?.TrySetResult(true));
        _popupNoButton.onClick.AddListener(() => _popupClosed?.TrySetResult(false));
        _keywordInput.onValueChanged.AddListener(_ => ApplyFilter());
        for (int i = 0; i < _costFilterButtons.Length; i++)
        {
            var cost = i - 1;
            _costFilterButtons[i].onClick.AddListener(() =>
            {
                _costFilter = cost;
                ApplyFilter();
            });
        }
        _detail.OnPlus += AddCard;
        _detail.OnMinus += RemoveCard;
    }

    private async UniTask SetUp()
    {
        try
        {
            await LoadDeck();
            CreateViews();
            _isLoaded = true;
            Refresh();
            ApplyFilter();
        }
        catch (System.Exception e)
        {
            //読み込みに失敗しても暗転したまま操作できなくならないよう、タイトルへ戻れる状態にする
            //一覧が途中までしか作られていないこともあるので、編集はさせない(_isLoadedはfalseのまま)
            _isLoaded = false;
            Debug.LogException(e);
            _hintText.text = "デッキの読み込みに失敗しました。タイトルへ戻ってください";
            _saveButton.interactable = false;
            _clearButton.interactable = false;
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
            //AssetLoaderのキャッシュは他の画面と共有なので使わず、自分のハンドルで読んで写したらすぐ解放する
            var handle = Addressables.LoadAssetAsync<InitialPlayerStatus>(ResourceKey.MasterData.PlayerInitStatus);
            try
            {
                var initialStatus = await handle;
                cardIds = initialStatus.CardId.ToList();
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        //マスタから消えたカードは外しておく
        _deck.AddRange(cardIds.Where(id => _cardDataRepository.FindById(id) != null));
        SortDeck();
        _savedDeck.AddRange(_deck);
    }

    /// <summary>
    /// 一覧はどちらもコストの低い順、同じコストならId順に並べる
    /// </summary>
    private IEnumerable<CardData> SortedCards()
    {
        return _cardDataRepository.GetAll().OrderBy(c => c.Cost).ThenBy(c => c.Id);
    }

    private void SortDeck()
    {
        _deck.Sort((a, b) =>
        {
            var cardA = _cardDataRepository.FindById(a);
            var cardB = _cardDataRepository.FindById(b);
            var cost = cardA.Cost.CompareTo(cardB.Cost);
            return cost != 0 ? cost : a.CompareTo(b);
        });
    }

    private void CreateViews()
    {
        foreach (var card in SortedCards())
        {
            var tile = Instantiate(_tileTemplate, _tileContent);
            tile.gameObject.SetActive(true);
            tile.name = $"Tile_{card.Id}";
            tile.SetCard(card, _settings);
            tile.OnAdd += t => AddCard(t.Card);
            tile.OnDetail += t => OpenDetail(t.Card);
            _tiles.Add(card.Id, tile);
            _searchTexts.Add(card.Id, $"{card.Name}\n{Regex.Replace(card.Description ?? "", "<.*?>", "")}");

            var row = Instantiate(_rowTemplate, _rowContent);
            row.name = $"Row_{card.Id}";
            row.SetCard(card, _settings);
            row.OnPlus += r => AddCard(r.Card);
            row.OnMinus += r => RemoveCard(r.Card);
            row.OnRemove += r => RemoveAllCopies(r.Card);
            row.OnDetail += r => OpenDetail(r.Card);
            _rows.Add(card.Id, row);
        }
    }

    private void AddCard(CardData card)
    {
        if (!CanEdit || card == null) return;
        if (!CanAdd(card.Id))
        {
            _hintText.text = _deck.Count >= _settings.MaxDeckCount
                ? $"デッキは{_settings.MaxDeckCount}枚までです"
                : $"同じカードは{_settings.MaxSameCardCount}枚までです";
            return;
        }
        _deck.Add(card.Id);
        SortDeck();
        Refresh();
        _tiles[card.Id].Punch();
        _rows[card.Id].Flash();
    }

    private void RemoveCard(CardData card)
    {
        if (!CanEdit || card == null || !_deck.Remove(card.Id)) return;
        Refresh();
        if (CountOf(card.Id) > 0) _rows[card.Id].Flash();
    }

    private void RemoveAllCopies(CardData card)
    {
        if (!CanEdit || card == null) return;
        if (_deck.RemoveAll(id => id == card.Id) == 0) return;
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

    private int CostColumn(int cost)
    {
        return Mathf.Clamp(cost, 0, _settings.MaxCostColumn);
    }

    private void Refresh()
    {
        foreach (var pair in _tiles)
        {
            pair.Value.SetState(CountOf(pair.Key), _settings.MaxSameCardCount, CanAdd(pair.Key));
        }
        foreach (var pair in _rows)
        {
            var count = CountOf(pair.Key);
            pair.Value.gameObject.SetActive(count > 0);
            pair.Value.SetCount(count, CanAdd(pair.Key));
        }
        _deckEmptyText.SetActive(_deck.Count == 0);

        //デッキのステータス
        var curve = new int[_settings.MaxCostColumn + 1];
        var types = new Dictionary<EffectType, int>();
        foreach (var id in _deck)
        {
            var card = _cardDataRepository.FindById(id);
            curve[CostColumn(card.Cost)]++;
            var type = card.EffectType;
            types[type] = types.TryGetValue(type, out var n) ? n + 1 : 1;
        }
        _costCurve.SetCounts(curve);
        _typeText.text = string.Join("　", new[] { EffectType.Attack, EffectType.Defence, EffectType.Special }
            .Select(t => $"{DeckBuildSettings.GetEffectTypeName(t)} {(types.TryGetValue(t, out var n) ? n : 0)}"));

        var isEnough = _deck.Count >= _settings.MinDeckCount;
        _deckCountText.text = $"{_deck.Count}<size=30> / {_settings.MaxDeckCount}</size>";
        _deckCountText.color = isEnough ? _countNormalColor : _countErrorColor;
        _deckRuleText.text = $"{_settings.MinDeckCount}〜{_settings.MaxDeckCount}枚・同じカードは{_settings.MaxSameCardCount}枚まで";
        _saveButton.interactable = isEnough && IsDirty;
        _clearButton.interactable = _deck.Count > 0;

        if (!isEnough)
        {
            _hintText.text = $"あと{_settings.MinDeckCount - _deck.Count}枚入れると保存できます";
        }
        else if (_deck.Count >= _settings.MaxDeckCount)
        {
            _hintText.text = "デッキの上限です。カードを外すと追加できます";
        }
        else
        {
            _hintText.text = "カードをタップしてデッキに追加。虫眼鏡で詳細を表示";
        }

        if (_detail.IsOpen) RefreshDetail();
    }

    private void ApplyFilter()
    {
        var keyword = _keywordInput.text.Trim();
        var visibleCount = 0;
        foreach (var tile in _tiles.Values)
        {
            var card = tile.Card;
            var matchCost = _costFilter < 0 || CostColumn(card.Cost) == _costFilter;
            var matchKeyword = keyword.Length == 0 || _searchTexts[card.Id].Contains(keyword);
            var visible = matchCost && matchKeyword;
            tile.gameObject.SetActive(visible);
            if (visible) visibleCount++;
        }
        _emptyText.SetActive(visibleCount == 0);

        for (int i = 0; i < _costFilterButtons.Length; i++)
        {
            _costFilterButtons[i].image.color = i - 1 == _costFilter ? _filterSelectedColor : _filterNormalColor;
        }
    }

    private void OpenDetail(CardData card)
    {
        if (!CanEdit) return;
        _detail.Show(card, _settings);
        RefreshDetail();
    }

    private void RefreshDetail()
    {
        var id = _detail.Card.Id;
        _detail.SetCount(CountOf(id), _settings.MaxSameCardCount, CanAdd(id));
    }

    private async UniTask Save()
    {
        if (!CanEdit || _deck.Count < _settings.MinDeckCount) return;
        _isBusy = true;
        try
        {
            if (!await _deckRepository.Save(new DeckData { CardIds = new List<int>(_deck) }))
            {
                //保存できなかったときは未保存のままにして、もう一度保存できるようにする
                _hintText.text = "デッキを保存できませんでした。もう一度保存してください";
                return;
            }
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

    private async UniTask ClearDeck()
    {
        if (!CanEdit || _deck.Count == 0) return;
        _isBusy = true;
        var isClear = await ShowPopup("デッキのカードをすべて外しますか？\n(保存するまでは元に戻せます)", "すべて外す", "やめる");
        if (isClear)
        {
            _deck.Clear();
            Refresh();
        }
        _isBusy = false;
    }

    private async UniTask BackToTitle()
    {
        if (_isBusy) return;
        _isBusy = true;

        if (IsDirty)
        {
            var isDiscard = await ShowPopup("デッキが保存されていません。\n保存せずにタイトルへ戻りますか？", "保存せずに戻る", "編集を続ける");
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

    /// <returns>はい側のボタンを押したらtrue</returns>
    private async UniTask<bool> ShowPopup(string body, string yesLabel, string noLabel)
    {
        _popupBodyText.text = body;
        _popupYesText.text = yesLabel;
        _popupNoText.text = noLabel;
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
        AssetLoader.Release(ResourceKey.Prefabs.DeckBuildScreen);
    }
}

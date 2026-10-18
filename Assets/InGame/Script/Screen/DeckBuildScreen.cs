//日本語対応
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
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
    /// <summary>キーワード検索に使う、名前とタグを除いた説明文(検索用に正規化済み)。入力のたびに作り直さないよう一度だけ作る</summary>
    private readonly Dictionary<int, string> _searchTexts = new Dictionary<int, string>();
    /// <summary>編集中のデッキ(カードのId)。コスト順に並べる</summary>
    private readonly List<int> _deck = new List<int>();
    /// <summary>カードごとのデッキに入っている枚数。_deckと一緒に更新する</summary>
    private readonly Dictionary<int, int> _counts = new Dictionary<int, int>();
    /// <summary>最後に保存したデッキ。編集中のデッキと違えば未保存</summary>
    private readonly List<int> _savedDeck = new List<int>();

    private TransitionService _transitionService;
    private ICardDataRepository _cardDataRepository;
    private IDeckRepository _deckRepository;
    private DeckBuildSettings _settings;
    private bool _isBusy = true;
    /// <summary>デッキと一覧を作り終えたらtrue。読み込みに失敗したときは編集させない</summary>
    private bool _isLoaded;
    /// <summary>保存したデッキがルールを満たしておらず、戦闘では初期デッキが使われる状態ならtrue</summary>
    private bool _isSavedDeckUnplayable;
    /// <summary>絞り込むコストの列。-1なら全て</summary>
    private int _costFilter = -1;
    /// <summary>コストの列数。設定とボタン・棒の数がずれていても、範囲外を触らないよう少ない方に合わせる</summary>
    private int _costColumnCount;
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
    /// コストの列数は設定(MaxCostColumn)に合わせる。ボタンや棒の数がずれていたら知らせて、少ない方の列数で動かす
    /// </summary>
    private void ValidateCostColumns()
    {
        var columnCount = _settings.MaxCostColumn + 1;
        _costColumnCount = Mathf.Max(1, Mathf.Min(columnCount, _costFilterButtons.Length - 1, _costCurve.ColumnCount));
        if (_costFilterButtons.Length != columnCount + 1)
        {
            Debug.LogError($"コストの絞り込みボタンは「全て」+{columnCount}個必要です(現在{_costFilterButtons.Length}個)。Prefabを再生成してください");
        }
        if (_costCurve.ColumnCount != columnCount)
        {
            Debug.LogError($"コスト分布の棒は{columnCount}本必要です(現在{_costCurve.ColumnCount}本)。Prefabを再生成してください");
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
            //列数より多いボタンは対応するコストが無いので押せなくする
            if (cost >= _costColumnCount)
            {
                _costFilterButtons[i].interactable = false;
                continue;
            }
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
        catch (Exception e)
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
    /// マスタから消えたカードの除外やルールの判定は、戦闘と同じくDeckRepositoryに任せる
    /// </summary>
    private async UniTask LoadDeck()
    {
        await _deckRepository.LoadData();
        var cardIds = _deckRepository.GetSavedCardIds();
        if (cardIds == null)
        {
            cardIds = await _deckRepository.LoadInitialCardIds();
        }
        else
        {
            _isSavedDeckUnplayable = !_deckRepository.IsPlayable(cardIds);
        }

        foreach (var id in cardIds) InsertSorted(id);
        _savedDeck.AddRange(_deck);
    }

    /// <summary>
    /// 一覧はどちらもコストの低い順、同じコストならId順に並べる
    /// </summary>
    private IEnumerable<CardData> SortedCards()
    {
        return _cardDataRepository.GetAll().OrderBy(c => c.Cost).ThenBy(c => c.Id);
    }

    private int CompareCard(int a, int b)
    {
        var cost = _cardDataRepository.FindById(a).Cost.CompareTo(_cardDataRepository.FindById(b).Cost);
        return cost != 0 ? cost : a.CompareTo(b);
    }

    /// <summary>
    /// コスト順を崩さない位置にカードを入れる。追加のたびにデッキ全体を並べ直さないようにする
    /// </summary>
    private void InsertSorted(int cardId)
    {
        var index = 0;
        while (index < _deck.Count && CompareCard(_deck[index], cardId) <= 0) index++;
        _deck.Insert(index, cardId);
        _counts[cardId] = CountOf(cardId) + 1;
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
            _searchTexts.Add(card.Id, NormalizeForSearch($"{card.Name}\n{Regex.Replace(card.Description ?? "", "<.*?>", "")}"));

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
        InsertSorted(card.Id);
        Refresh();
        _tiles[card.Id].Punch();
        _rows[card.Id].Flash();
    }

    private void RemoveCard(CardData card)
    {
        if (!CanEdit || card == null || !_deck.Remove(card.Id)) return;
        var count = CountOf(card.Id) - 1;
        if (count > 0) _counts[card.Id] = count;
        else _counts.Remove(card.Id);
        Refresh();
        if (CountOf(card.Id) > 0) _rows[card.Id].Flash();
    }

    private void RemoveAllCopies(CardData card)
    {
        if (!CanEdit || card == null) return;
        if (_deck.RemoveAll(id => id == card.Id) == 0) return;
        _counts.Remove(card.Id);
        Refresh();
    }

    private bool CanAdd(int cardId)
    {
        return _deck.Count < _settings.MaxDeckCount && CountOf(cardId) < _settings.MaxSameCardCount;
    }

    private int CountOf(int cardId)
    {
        return _counts.TryGetValue(cardId, out var count) ? count : 0;
    }

    private int CostColumn(int cost)
    {
        return Mathf.Clamp(cost, 0, _costColumnCount - 1);
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
        var curve = new int[_costColumnCount];
        var types = new Dictionary<EffectType, int>();
        foreach (var pair in _counts)
        {
            var card = _cardDataRepository.FindById(pair.Key);
            curve[CostColumn(card.Cost)] += pair.Value;
            var type = card.EffectType;
            types[type] = (types.TryGetValue(type, out var n) ? n : 0) + pair.Value;
        }
        _costCurve.SetCounts(curve);
        _typeText.text = string.Join("　", new[] { EffectType.Attack, EffectType.Defence, EffectType.Special }
            .Select(t => $"{DeckBuildSettings.GetEffectTypeName(t)} {(types.TryGetValue(t, out var n) ? n : 0)}"));

        var isEnough = _deck.Count >= _settings.MinDeckCount;
        var isPlayable = _deckRepository.IsPlayable(_deck);
        _deckCountText.text = $"{_deck.Count}<size=30> / {_settings.MaxDeckCount}</size>";
        _deckCountText.color = isPlayable ? _countNormalColor : _countErrorColor;
        _deckRuleText.text = $"{_settings.MinDeckCount}〜{_settings.MaxDeckCount}枚・同じカードは{_settings.MaxSameCardCount}枚まで";
        _saveButton.interactable = isPlayable && IsDirty;
        _clearButton.interactable = _deck.Count > 0;

        if (_isSavedDeckUnplayable && !IsDirty)
        {
            _hintText.text = "保存したデッキがルールを満たしていないため、戦闘では初期デッキが使われます";
        }
        else if (!isEnough)
        {
            _hintText.text = $"あと{_settings.MinDeckCount - _deck.Count}枚入れると保存できます";
        }
        else if (!isPlayable)
        {
            _hintText.text = "デッキの枚数か同じカードの枚数が上限を超えています。カードを外してください";
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
        var keyword = NormalizeForSearch(_keywordInput.text.Trim());
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

    /// <summary>
    /// 検索で大文字小文字・全角半角の英数字を区別しないよう、半角の小文字にそろえる
    /// </summary>
    private static string NormalizeForSearch(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            //全角の英数字・記号(！〜～)を半角にする
            var half = '！' <= c && c <= '～' ? (char)(c - '！' + '!') : c;
            builder.Append(char.ToLowerInvariant(half));
        }
        return builder.ToString();
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
        if (!CanEdit || !_deckRepository.IsPlayable(_deck)) return;
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
            _isSavedDeckUnplayable = false;
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
        try
        {
            var isClear = await ShowPopup("デッキのカードをすべて外しますか？\n(保存するまでは元に戻せます)", "すべて外す", "やめる");
            if (isClear)
            {
                _deck.Clear();
                _counts.Clear();
                Refresh();
            }
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

        try
        {
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
        catch (Exception e)
        {
            //遷移に失敗したら暗転を戻し、もう一度戻れるようにする
            Debug.LogException(e);
            if (this == null) return;
            _fadeImage.DOKill();
            _fadeImage.color = Color.clear;
            _fadeImage.raycastTarget = false;
            _hintText.text = "タイトルへ戻れませんでした。もう一度お試しください";
            _isBusy = false;
        }
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

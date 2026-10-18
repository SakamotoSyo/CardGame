//日本語対応
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ショップ画面。ガチャ石を払ってガチャを引き、出たカードを所持カードに加える
/// </summary>
public class ShopScreen : ScreenBase
{
    [Header("ヘッダー")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Text _stoneText;

    [Header("ガチャ")]
    [SerializeField] private Button _singleButton;
    [SerializeField] private Text _singleButtonText;
    [SerializeField] private Button _multiButton;
    [SerializeField] private Text _multiButtonText;
    [Tooltip("レアリティごとの排出率")]
    [SerializeField] private Text _rateText;
    [SerializeField] private Text _hintText;

    [Header("結果")]
    [SerializeField] private CanvasGroup _resultGroup;
    [SerializeField] private RectTransform _resultContent;
    [SerializeField] private GachaResultCardView _resultTemplate;
    [SerializeField] private Text _resultTitleText;
    [SerializeField] private Button _resultOkButton;
    [Tooltip("演出中に押すと最後まで飛ばす")]
    [SerializeField] private Button _resultSkipButton;
    [SerializeField] private float _resultCardSpacingX = 230f;
    [SerializeField] private float _resultCardSpacingY = 320f;
    [SerializeField] private int _resultColumnCount = 5;
    [Tooltip("1枚だけのときの拡大率")]
    [SerializeField] private float _singleResultScale = 1.5f;

    [Header("演出")]
    [SerializeField] private Image _fadeImage;

    private readonly List<GachaResultCardView> _resultViews = new List<GachaResultCardView>();
    private TransitionService _transitionService;
    private IWalletRepository _walletRepository;
    private GachaService _gachaService;
    private GachaSettings _gachaSettings;
    private DeckBuildSettings _deckBuildSettings;
    private bool _isBusy = true;
    private Sequence _resultSequence;

    [VContainer.Inject]
    public void Construct(TransitionService transitionService,
        IWalletRepository walletRepository,
        GachaService gachaService,
        GachaSettings gachaSettings,
        DeckBuildSettings deckBuildSettings)
    {
        _transitionService = transitionService;
        _walletRepository = walletRepository;
        _gachaService = gachaService;
        _gachaSettings = gachaSettings;
        _deckBuildSettings = deckBuildSettings;
    }

    private void Start()
    {
        _resultGroup.gameObject.SetActive(false);
        _resultTemplate.gameObject.SetActive(false);
        _fadeImage.color = Color.black;
        _fadeImage.raycastTarget = true;
        _backButton.onClick.AddListener(() => BackToTitle().Forget());
        _singleButton.onClick.AddListener(() => Draw(1, _gachaSettings.SingleCost).Forget());
        _multiButton.onClick.AddListener(() => Draw(_gachaSettings.MultiCount, _gachaSettings.MultiCost).Forget());
        _resultSkipButton.onClick.AddListener(() => _resultSequence?.Complete(true));
        _resultOkButton.onClick.AddListener(CloseResult);
        _walletRepository.OnGachaStoneChanged += OnGachaStoneChanged;
        _singleButtonText.text = $"1回引く\n<size=26>ガチャ石 {_gachaSettings.SingleCost}</size>";
        _multiButtonText.text = $"{_gachaSettings.MultiCount}連\n<size=26>ガチャ石 {_gachaSettings.MultiCost}</size>";
        _rateText.text = "排出率　" + string.Join("　", _gachaService.GetRarityRates().Select(r => $"レアリティ{r.Rare} {r.Percent:0.#}%"));
        SetUp().Forget();
    }

    private async UniTask SetUp()
    {
        try
        {
            await _walletRepository.LoadData();
            Refresh();
            _hintText.text = "ランで進んだ日数分のガチャ石がもらえます";
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            _hintText.text = "読み込みに失敗しました。タイトルへ戻ってください";
            _singleButton.interactable = false;
            _multiButton.interactable = false;
        }

        await _fadeImage.DOFade(0f, 0.4f).SetLink(gameObject).ToUniTask();
        _fadeImage.raycastTarget = false;
        _isBusy = false;
    }

    /// <summary>
    /// ガチャ以外(デバッグツールなど)で増減しても、所持数とボタンの状態を合わせる
    /// </summary>
    private void OnGachaStoneChanged(int stone)
    {
        Refresh();
    }

    private void Refresh()
    {
        var stone = _walletRepository.GachaStone;
        _stoneText.text = $"ガチャ石 {stone}";
        _singleButton.interactable = stone >= _gachaSettings.SingleCost;
        _multiButton.interactable = stone >= _gachaSettings.MultiCost;
    }

    private async UniTask Draw(int count, int cost)
    {
        if (_isBusy) return;
        if (_walletRepository.GachaStone < cost)
        {
            _hintText.text = "ガチャ石が足りません";
            return;
        }
        _isBusy = true;
        try
        {
            var results = await _gachaService.Draw(count, cost);
            Refresh();
            if (results == null)
            {
                _hintText.text = "ガチャを引けませんでした。もう一度お試しください";
                return;
            }
            _stoneText.rectTransform.DOKill(true);
            _stoneText.rectTransform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.8f).SetLink(gameObject);
            await ShowResult(results);
        }
        finally
        {
            _isBusy = false;
        }
    }

    /// <summary>
    /// 結果のカードを裏向きで並べ、1枚ずつめくる
    /// </summary>
    private async UniTask ShowResult(List<GachaResult> results)
    {
        foreach (var view in _resultViews) Destroy(view.gameObject);
        _resultViews.Clear();

        var newCount = results.Count(r => r.IsNew);
        _resultTitleText.text = newCount > 0 ? $"ガチャ結果　<color=#FFCC59>NEW {newCount}枚</color>" : "ガチャ結果";
        _resultOkButton.gameObject.SetActive(false);
        _resultSkipButton.gameObject.SetActive(true);
        //閉じてすぐ引き直したときに、閉じる演出の最後で非表示にされないよう止めておく
        _resultGroup.DOKill();
        _resultGroup.gameObject.SetActive(true);
        _resultGroup.alpha = 0f;

        var rows = Mathf.CeilToInt(results.Count / (float)_resultColumnCount);
        for (int i = 0; i < results.Count; i++)
        {
            var view = Instantiate(_resultTemplate, _resultContent);
            view.gameObject.SetActive(true);
            view.name = $"Result_{i}";
            view.SetResult(results[i], _deckBuildSettings);
            var rect = (RectTransform)view.transform;
            if (results.Count == 1)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one * _singleResultScale;
            }
            else
            {
                var row = i / _resultColumnCount;
                var column = i % _resultColumnCount;
                var columnsInRow = Mathf.Min(_resultColumnCount, results.Count - row * _resultColumnCount);
                rect.anchoredPosition = new Vector2(
                    (column - (columnsInRow - 1) / 2f) * _resultCardSpacingX,
                    ((rows - 1) / 2f - row) * _resultCardSpacingY);
            }
            _resultViews.Add(view);
        }

        _resultSequence = DOTween.Sequence().Append(_resultGroup.DOFade(1f, 0.2f));
        for (int i = 0; i < _resultViews.Count; i++)
        {
            _resultSequence.Insert(0.15f + i * 0.05f, _resultViews[i].Appear());
        }
        var revealStart = 0.4f + _resultViews.Count * 0.05f;
        for (int i = 0; i < _resultViews.Count; i++)
        {
            _resultSequence.Insert(revealStart + i * 0.15f, _resultViews[i].Reveal());
        }
        _resultSequence.SetLink(gameObject);
        await _resultSequence.ToUniTask();
        _resultSequence = null;

        _resultSkipButton.gameObject.SetActive(false);
        _resultOkButton.gameObject.SetActive(true);
    }

    private void CloseResult()
    {
        _resultGroup.DOKill();
        _resultGroup.DOFade(0f, 0.15f).OnComplete(() => _resultGroup.gameObject.SetActive(false)).SetLink(gameObject);
        _hintText.text = "手に入れたカードはデッキ編集で使えます";
    }

    private async UniTask BackToTitle()
    {
        if (_isBusy) return;
        _isBusy = true;
        _fadeImage.raycastTarget = true;
        await _fadeImage.DOFade(1f, 0.3f).SetLink(gameObject).ToUniTask();
        try
        {
            await _transitionService.BackToTitleScreen();
        }
        catch (Exception e)
        {
            //遷移に失敗しても暗転したまま操作できなくならないよう戻す
            Debug.LogException(e);
            if (this == null) return;
            _fadeImage.color = new Color(0f, 0f, 0f, 0f);
            _fadeImage.raycastTarget = false;
            _isBusy = false;
        }
    }

    private void OnDestroy()
    {
        if (_walletRepository != null) _walletRepository.OnGachaStoneChanged -= OnGachaStoneChanged;
        AssetLoader.Release(ResourceKey.Prefabs.ShopScreen);
    }
}

//日本語対応
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステージ選択画面(1日ごとの行動をカードから選ぶマップ)
/// 一番下の列が今日。前日に選んだカードの真上とその左右から1枚選び、選ぶと盤面が1列スクロールして翌日へ進む
/// </summary>
public class StageSelectScreen : ScreenBase
{
    [Header("マップ")]
    [Tooltip("カードの列をまとめて動かす盤面")]
    [SerializeField] private RectTransform _board;
    [Tooltip("手前(今日)から奥へ順に並べる")]
    [SerializeField] private RectTransform[] _rows;
    [SerializeField] private StageNodeView _nodeTemplate;
    [SerializeField] private float _columnSpacing = 280f;
    [SerializeField] private float _rowSpacing = 310f;
    [Tooltip("盤面の何もないところをタップしたら選択を解除する")]
    [SerializeField] private Button _boardBackgroundButton;

    [Header("ヘッダー")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Text _dayText;
    [SerializeField] private Text _bossText;
    [SerializeField] private Text _cycleText;
    [SerializeField] private Text _hpText;
    [SerializeField] private Text _goldText;
    [SerializeField] private Text _hintText;

    [Header("選択中のカードの上に出す説明")]
    [SerializeField] private RectTransform _tooltip;
    [SerializeField] private CanvasGroup _tooltipGroup;
    [SerializeField] private Image _tooltipAccent;
    [SerializeField] private Text _tooltipNameText;
    [SerializeField] private Text _tooltipDescriptionText;

    [Header("演出")]
    [SerializeField] private StageSelectEffects _effects;
    [Tooltip("決定したカードを拡大表示するための最前面のレイヤー")]
    [SerializeField] private RectTransform _focusLayer;
    [SerializeField] private Image _fadeImage;
    [Tooltip("決定したカードの後ろで盤面だけを暗くする")]
    [SerializeField] private Image _boardDimImage;

    [Header("未実装の行動を実行したときのポップアップ")]
    [SerializeField] private CanvasGroup _popup;
    [SerializeField] private Text _popupTitleText;
    [SerializeField] private Text _popupBodyText;
    [SerializeField] private Button _popupOkButton;

    private readonly List<StageNodeView[]> _nodes = new List<StageNodeView[]>();
    private TransitionService _transitionService;
    private IDungeonRunRepository _runRepository;
    private IPlayerStatus _playerStatus;
    private StageMapSettings _settings;
    private StageMapGenerator _generator;
    private DungeonRunData _run;
    private int _selectedColumn = -1;
    private bool _isBusy = true;
    private UniTaskCompletionSource _popupClosed;

    [VContainer.Inject]
    public void Construct(TransitionService transitionService,
        IDungeonRunRepository runRepository,
        IPlayerStatus playerStatus,
        StageMapSettings settings,
        StageMapGenerator generator)
    {
        _transitionService = transitionService;
        _runRepository = runRepository;
        _playerStatus = playerStatus;
        _settings = settings;
        _generator = generator;
    }

    private void Start()
    {
        _popup.gameObject.SetActive(false);
        _nodeTemplate.gameObject.SetActive(false);
        _tooltipGroup.alpha = 0f;
        _fadeImage.color = Color.black;
        _boardDimImage.color = new Color(0f, 0f, 0f, 0f);
        _boardDimImage.raycastTarget = false;
        _fadeImage.raycastTarget = true;
        BindView();
        SetUp().Forget();
    }

    private async UniTask SetUp()
    {
        try
        {
            await LoadRun();
            CreateNodes();
            Refresh(true);
        }
        catch (System.Exception e)
        {
            //読み込みに失敗しても暗転したまま操作できなくならないよう、タイトルへ戻れる状態にする
            Debug.LogException(e);
            _run = null;
            _hintText.text = "マップの読み込みに失敗しました。タイトルへ戻ってください";
            await _fadeImage.DOFade(0f, 0.5f).SetLink(gameObject).ToUniTask();
            _fadeImage.raycastTarget = false;
            _isBusy = false;
            return;
        }

        //暗転から明けながら、盤面が上から1列分スクロールして今日の列が手前に来る
        _board.anchoredPosition = new Vector2(0f, _rowSpacing);
        await DOTween.Sequence()
            .Join(_fadeImage.DOFade(0f, 0.5f))
            .Join(_board.DOAnchorPosY(0f, 0.8f).SetEase(Ease.OutCubic))
            .SetLink(gameObject)
            .ToUniTask();
        _fadeImage.raycastTarget = false;
        _isBusy = false;

        //中断した戦闘があれば、そのカードを選んだ状態から再開する
        if (_run.HasPendingStage) Select(_run.PendingColumn);
    }

    private async UniTask LoadRun()
    {
        await _runRepository.LoadData();
        var run = _runRepository.Current;
        if (run == null || !_generator.IsValid(run))
        {
            if (run != null) Debug.LogWarning("保存されていたランが現在のマップ設定と合わないため、新しいランを開始します");
            _runRepository.StartNewRun(_generator.CreateNewRun());
        }
        _run = _runRepository.Current;
        _generator.EnsureRows(_run);
        //戦闘に勝って戻ってきたら、ここで翌日へ進める
        if (_run.HasPendingStage && _run.IsPendingStageCleared)
        {
            _generator.Advance(_run, _run.PendingColumn);
        }
        await _runRepository.Save();
    }

    private void BindView()
    {
        _backButton.onClick.AddListener(() => BackToTitle().Forget());
        _boardBackgroundButton.onClick.AddListener(() =>
        {
            if (_isBusy || _selectedColumn < 0) return;
            Select(-1);
        });
        _popupOkButton.onClick.AddListener(() => _popupClosed?.TrySetResult());

        _playerStatus.CurrentHp.Subscribe(_ => UpdateStatusText()).AddTo(this);
        _playerStatus.MaxHp.Subscribe(_ => UpdateStatusText()).AddTo(this);
        _playerStatus.Gold.Subscribe(_ => UpdateStatusText()).AddTo(this);
    }

    private void CreateNodes()
    {
        for (int row = 0; row < _rows.Length; row++)
        {
            var nodes = new StageNodeView[_generator.Width];
            for (int x = 0; x < nodes.Length; x++)
            {
                var node = Instantiate(_nodeTemplate, _rows[row]);
                node.gameObject.SetActive(true);
                node.name = $"Node_{row}_{x}";
                ((RectTransform)node.transform).anchoredPosition = new Vector2((x - (nodes.Length - 1) / 2f) * _columnSpacing, 0f);
                node.OnClick += HandleNodeClick;
                nodes[x] = node;
            }
            _nodes.Add(nodes);
        }
    }

    /// <param name="snap">trueなら補間せずに見た目を切り替える(日送りで盤面を入れ替えたとき)</param>
    private void Refresh(bool snap = false)
    {
        for (int row = 0; row < _nodes.Count; row++)
        {
            var day = _run.Day + row;
            for (int x = 0; x < _nodes[row].Length; x++)
            {
                var node = _nodes[row][x];
                var isToday = row == 0;
                var isSelectable = isToday && _generator.IsSelectable(_run, x);

                node.SetCard(_settings.GetInfo(_generator.GetCell(_run, day, x)), day, x);
                node.SetState(
                    isDimmed: isToday ? !isSelectable : !IsReachableFromChoice(day, x),
                    isSelectable: isSelectable,
                    isSelected: isToday && x == _selectedColumn,
                    isPlanned: !isToday && _run.IsPlanned(day, x));
                if (snap) node.SnapVisual();
            }
        }

        _dayText.text = $"DAY {_run.Day}";
        var daysUntilBoss = _generator.GetDaysUntilBoss(_run.Day);
        _bossText.text = daysUntilBoss == 0 ? "本日 ボス戦！" : $"ボス戦まで あと{daysUntilBoss}日";
        _cycleText.text = $"第{_generator.GetCycle(_run.Day)}周";
        _hintText.text = _selectedColumn < 0
            ? "光っているカードから今日の行動を選ぼう　(奥のカードをタップで予定ルートに印)"
            : "もう一度タップで出発";
    }

    /// <summary>
    /// 選択中のカード(未選択なら今日選べるカードのどれか)からたどり着けるか
    /// </summary>
    private bool IsReachableFromChoice(int day, int column)
    {
        if (_selectedColumn >= 0) return _generator.IsReachable(_run, _selectedColumn, day, column);

        for (int x = 0; x < _generator.Width; x++)
        {
            if (_generator.IsSelectable(_run, x) && _generator.IsReachable(_run, x, day, column)) return true;
        }
        return false;
    }

    private void UpdateStatusText()
    {
        var maxHp = _playerStatus.MaxHp.Value;
        _hpText.text = maxHp > 0 ? $"HP {_playerStatus.CurrentHp.Value:0}/{maxHp:0}" : "HP --";
        _goldText.text = $"{_playerStatus.Gold.Value:0} G";
    }

    private void HandleNodeClick(StageNodeView node)
    {
        if (_isBusy || _run == null) return;

        if (node.Day == _run.Day)
        {
            if (!node.IsSelectable) return;
            //選択中のカードをもう一度タップしたら出発する
            if (_selectedColumn == node.Column)
            {
                Depart().Forget();
                return;
            }
            Select(node.Column);
        }
        else
        {
            _run.TogglePlanned(node.Day, node.Column);
            _runRepository.Save().Forget();
            _effects.Burst(node.transform.position, 6, 90f, new Color(1f, 0.85f, 0.4f));
            Refresh();
        }
    }

    private void Select(int column)
    {
        _selectedColumn = column;
        Refresh();
        _tooltip.DOKill();
        _tooltipGroup.DOKill();

        if (column < 0)
        {
            _tooltipGroup.DOFade(0f, 0.15f).SetLink(gameObject);
            return;
        }

        var node = _nodes[0][column];
        _effects.Burst(node.transform.position, 14, 170f, new Color(1f, 0.95f, 0.75f));

        //カードの真上に名前と説明を出す
        var info = _settings.GetInfo(_generator.GetCell(_run, _run.Day, column));
        _tooltipNameText.text = info.Name;
        _tooltipDescriptionText.text = info.Description;
        _tooltipAccent.color = info.Color;
        var parent = (RectTransform)_tooltip.parent;
        var local = (Vector2)parent.InverseTransformPoint(node.transform.position);
        var halfWidth = _tooltip.sizeDelta.x / 2f;
        var limit = parent.rect.width / 2f - halfWidth - 20f;
        _tooltip.anchoredPosition = new Vector2(Mathf.Clamp(local.x, -limit, limit), local.y + 345f);
        _tooltip.localScale = Vector3.one * 0.85f;
        _tooltipGroup.alpha = 0f;
        _tooltip.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetLink(gameObject);
        _tooltipGroup.DOFade(1f, 0.15f).SetLink(gameObject);
    }

    /// <summary>
    /// 選んだカードが光りながら手前に飛び出し、戦闘なら画面いっぱいに拡大して暗転する
    /// </summary>
    private async UniTask Depart()
    {
        if (_isBusy || _selectedColumn < 0) return;
        _isBusy = true;
        _fadeImage.raycastTarget = true;

        var node = _nodes[0][_selectedColumn];
        var type = _generator.GetCell(_run, _run.Day, _selectedColumn);
        _tooltipGroup.DOFade(0f, 0.15f).SetLink(gameObject);
        _hintText.text = "";

        //演出用に複製したカードを最前面で動かす
        var focus = Instantiate(node, _focusLayer);
        focus.transform.position = node.transform.position;
        focus.DetachFromBoard();
        node.gameObject.SetActive(false);
        var focusRect = (RectTransform)focus.transform;

        //盤面を少し暗くして、選んだカードだけを光らせながら手前に引き寄せる
        _effects.Burst(node.transform.position, 32, 360f, Color.white);
        _effects.Flash(0.55f, 0.5f).Forget();
        await DOTween.Sequence()
            .Join(_boardDimImage.DOFade(0.55f, 0.35f))
            .Join(focusRect.DOAnchorPos(new Vector2(0f, 70f), 0.45f).SetEase(Ease.OutCubic))
            .Join(focusRect.DOScale(1.6f, 0.45f).SetEase(Ease.OutBack))
            .SetLink(focus.gameObject)
            .ToUniTask();

        var info = _settings.GetInfo(type);
        var isBattle = type == StageType.Battle || type == StageType.Elite || type == StageType.Boss;
        if (isBattle)
        {
            //戦闘は勝つまで日を進めない(途中でアプリを終了して戦闘を飛ばせないようにする)
            _run.SetPendingStage(_selectedColumn);
        }
        else
        {
            _generator.Advance(_run, _selectedColumn);
        }
        _run.ActiveStage = type;
        _selectedColumn = -1;
        await _runRepository.Save();

        switch (type)
        {
            case StageType.Battle:
            case StageType.Elite:
            case StageType.Boss:
                //カードの中へ飛び込むように拡大しながら暗転する
                await DOTween.Sequence()
                    .Join(focusRect.DOScale(6f, 0.55f).SetEase(Ease.InCubic))
                    .Join(_fadeImage.DOFade(1f, 0.5f).SetEase(Ease.InQuad))
                    .SetLink(gameObject)
                    .ToUniTask();
                await _transitionService.BattleStart();
                return;
            default:
                //商人・休息・宝箱・イベントの画面はまだ無いので、何もせず翌日へ進める
                await ShowPopup(info.Name, "この行動の画面はまだ実装されていません。\n何も起きずに翌日へ進みます。");
                await DOTween.Sequence()
                    .Join(focus.GetComponent<CanvasGroup>().DOFade(0f, 0.25f))
                    .Join(_boardDimImage.DOFade(0f, 0.25f))
                    .SetLink(focus.gameObject)
                    .ToUniTask();
                Destroy(focus.gameObject);
                node.gameObject.SetActive(true);
                await ScrollToNextDay();
                break;
        }

        _fadeImage.raycastTarget = false;
        _isBusy = false;
    }

    /// <summary>
    /// 翌日の並びに入れ替え、盤面を1列分スクロールさせる
    /// </summary>
    private async UniTask ScrollToNextDay()
    {
        Refresh(true);
        _board.anchoredPosition = new Vector2(0f, _rowSpacing);
        await _board.DOAnchorPosY(0f, 0.7f).SetEase(Ease.OutCubic).SetLink(gameObject).ToUniTask();
    }

    private async UniTask ShowPopup(string title, string body)
    {
        _popupTitleText.text = title;
        _popupBodyText.text = body;
        _popup.gameObject.SetActive(true);
        _popup.alpha = 0f;
        _popup.transform.localScale = Vector3.one * 0.9f;
        DOTween.Sequence()
            .Join(_popup.DOFade(1f, 0.2f))
            .Join(_popup.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack))
            .SetLink(gameObject);

        _popupClosed = new UniTaskCompletionSource();
        await _popupClosed.Task;
        await _popup.DOFade(0f, 0.15f).SetLink(gameObject).ToUniTask();
        _popup.gameObject.SetActive(false);
    }

    private async UniTask BackToTitle()
    {
        if (_isBusy) return;
        _isBusy = true;
        _fadeImage.raycastTarget = true;
        await _fadeImage.DOFade(1f, 0.3f).SetLink(gameObject).ToUniTask();
        await _transitionService.BackToTitleScreen();
    }

    private void OnDestroy()
    {
        AssetLoader.Release(ResourceKey.Prefabs.StageSelectScreen);
    }
}

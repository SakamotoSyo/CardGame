//日本語対応
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 虫眼鏡やデッキ一覧の行から開くカードの詳細。ここからも枚数を増減できる
/// </summary>
public class DeckCardDetailView : MonoBehaviour
{
    public event Action<CardData> OnPlus;
    public event Action<CardData> OnMinus;

    [SerializeField] private CanvasGroup _group;
    [SerializeField] private RectTransform _window;
    [SerializeField] private DeckCardFaceView _face;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _infoText;
    [SerializeField] private Text _descriptionText;
    [SerializeField] private Text _countText;
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;
    [SerializeField] private Button _closeButton;
    [Tooltip("ウィンドウの外をタップしても閉じる")]
    [SerializeField] private Button _backgroundButton;

    public CardData Card { get; private set; }
    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        _plusButton.onClick.AddListener(() => OnPlus?.Invoke(Card));
        _minusButton.onClick.AddListener(() => OnMinus?.Invoke(Card));
        _closeButton.onClick.AddListener(Hide);
        _backgroundButton.onClick.AddListener(Hide);
    }

    public void Show(CardData card, DeckBuildSettings settings)
    {
        Card = card;
        _face.SetCard(card, settings);
        _nameText.text = card.Name;
        _infoText.text = $"コスト {card.Cost}　/　{DeckBuildSettings.GetEffectTypeName(DeckBuildSettings.GetEffectType(card))}　/　レアリティ {card.Rare}";
        _descriptionText.text = card.Description;

        gameObject.SetActive(true);
        _group.DOKill();
        _window.DOKill();
        _group.alpha = 0f;
        _window.localScale = Vector3.one * 0.9f;
        _group.DOFade(1f, 0.15f).SetLink(gameObject);
        _window.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    public void SetCount(int count, int maxCount, bool canPlus)
    {
        _countText.text = $"デッキに {count} / {maxCount}枚";
        _plusButton.interactable = canPlus;
        _minusButton.interactable = count > 0;
    }

    public void Hide()
    {
        if (!IsOpen) return;
        _group.DOKill();
        _group.DOFade(0f, 0.12f).OnComplete(() => gameObject.SetActive(false)).SetLink(gameObject);
    }
}

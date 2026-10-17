//日本語対応
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デッキ編成一覧の1行(カード1種類分)。「＋」「−」で枚数を増減し、「×」でデッキから外す
/// </summary>
public class DeckListRowView : MonoBehaviour
{
    public event Action<DeckListRowView> OnPlus;
    public event Action<DeckListRowView> OnMinus;
    public event Action<DeckListRowView> OnRemove;
    public event Action<DeckListRowView> OnDetail;

    [SerializeField] private RectTransform _body;
    [SerializeField] private Text _costText;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _countText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _rarityImage;
    [SerializeField] private Button _detailButton;
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;
    [SerializeField] private Button _removeButton;
    [Tooltip("追加したときに光らせる")]
    [SerializeField] private Image _flashImage;

    public CardData Card { get; private set; }

    private void Awake()
    {
        _detailButton.onClick.AddListener(() => OnDetail?.Invoke(this));
        _plusButton.onClick.AddListener(() => OnPlus?.Invoke(this));
        _minusButton.onClick.AddListener(() => OnMinus?.Invoke(this));
        _removeButton.onClick.AddListener(() => OnRemove?.Invoke(this));
        var color = _flashImage.color;
        color.a = 0f;
        _flashImage.color = color;
    }

    public void SetCard(CardData card, DeckBuildSettings settings)
    {
        Card = card;
        _costText.text = card.Cost.ToString();
        _nameText.text = card.Name;
        var icon = settings.GetIcon(DeckBuildSettings.GetEffectType(card));
        _iconImage.sprite = icon;
        _iconImage.enabled = icon != null;
        _rarityImage.color = settings.GetRarityColor(card.Rare);
    }

    public void SetCount(int count, bool canPlus)
    {
        _countText.text = $"×{count}";
        _plusButton.interactable = canPlus;
    }

    /// <summary>
    /// 追加・削除したときに行を光らせて弾ませる
    /// </summary>
    public void Flash()
    {
        _flashImage.DOKill();
        _flashImage.DOFade(0.55f, 0.08f)
            .OnComplete(() => _flashImage.DOFade(0f, 0.35f).SetLink(gameObject))
            .SetLink(gameObject);
        _body.DOKill(true);
        _body.DOPunchScale(Vector3.one * 0.04f, 0.25f, 6, 0.8f).SetLink(gameObject);
    }
}

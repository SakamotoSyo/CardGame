//日本語対応
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デッキ編集画面の左側に並べるカード1枚分のタイル
/// カードか下の「＋」を押すとデッキに追加し、虫眼鏡を押すと詳細を開く
/// </summary>
public class DeckCardTileView : MonoBehaviour
{
    public event Action<DeckCardTileView> OnAdd;
    public event Action<DeckCardTileView> OnDetail;

    [SerializeField] private DeckCardFaceView _face;
    [SerializeField] private RectTransform _body;
    [SerializeField] private Button _cardButton;
    [SerializeField] private Button _addButton;
    [SerializeField] private Button _detailButton;
    [Tooltip("デッキに入っている枚数")]
    [SerializeField] private Text _countText;
    [SerializeField] private Image _countBadge;
    [SerializeField] private Color _countBadgeEmptyColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Color _countBadgeColor = new Color(0.85f, 0.6f, 0.15f);
    [Tooltip("同じカードの上限に達したときに暗くする")]
    [SerializeField] private GameObject _limitOverlay;

    public CardData Card { get; private set; }

    private void Awake()
    {
        _cardButton.onClick.AddListener(() => OnAdd?.Invoke(this));
        _addButton.onClick.AddListener(() => OnAdd?.Invoke(this));
        _detailButton.onClick.AddListener(() => OnDetail?.Invoke(this));
    }

    public void SetCard(CardData card, DeckBuildSettings settings)
    {
        Card = card;
        _face.SetCard(card, settings);
    }

    /// <param name="count">デッキに入っている枚数</param>
    /// <param name="canAdd">これ以上追加できないときはfalse</param>
    public void SetState(int count, int maxCount, bool canAdd)
    {
        _countText.text = $"{count}/{maxCount}";
        _countBadge.color = count > 0 ? _countBadgeColor : _countBadgeEmptyColor;
        _cardButton.interactable = canAdd;
        _addButton.interactable = canAdd;
        _limitOverlay.SetActive(count >= maxCount);
    }

    /// <summary>
    /// 追加したときに少し弾ませる
    /// </summary>
    public void Punch()
    {
        _body.DOKill(true);
        _body.DOPunchScale(Vector3.one * 0.08f, 0.25f, 8, 0.8f).SetLink(gameObject);
    }
}

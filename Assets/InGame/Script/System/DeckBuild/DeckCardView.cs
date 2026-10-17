//日本語対応
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デッキ編集画面の一覧に並べるカード1種類分の行
/// </summary>
public class DeckCardView : MonoBehaviour
{
    public event Action<DeckCardView> OnClick;

    [SerializeField] private Button _button;
    [SerializeField] private RectTransform _body;
    [SerializeField] private Text _costText;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _descriptionText;
    [SerializeField] private Text _countText;
    [Tooltip("押せないときに上から重ねて暗くする")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _disabledAlpha = 0.45f;

    public CardData Card { get; private set; }

    private void Awake()
    {
        _button.onClick.AddListener(() =>
        {
            Punch();
            OnClick?.Invoke(this);
        });
    }

    public void SetCard(CardData card)
    {
        Card = card;
        _costText.text = card.Cost.ToString();
        _nameText.text = card.Name;
        _descriptionText.text = card.Description;
    }

    /// <param name="countText">右端に出す枚数の表示</param>
    /// <param name="interactable">これ以上追加/削除できないときはfalse</param>
    public void SetState(string countText, bool interactable)
    {
        _countText.text = countText;
        _button.interactable = interactable;
        _canvasGroup.alpha = interactable ? 1f : _disabledAlpha;
    }

    /// <summary>
    /// 押したときに少し弾ませる
    /// </summary>
    public void Punch()
    {
        _body.DOKill(true);
        _body.DOPunchScale(Vector3.one * 0.06f, 0.2f, 8, 0.8f).SetLink(gameObject);
    }
}

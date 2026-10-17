//日本語対応
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ステージ選択マップのカード1枚分の見た目
/// </summary>
public class StageNodeView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<StageNodeView> OnClick;

    [SerializeField] private RectTransform _body;
    [SerializeField] private GameObject _slot;
    [SerializeField] private Image _glowImage;
    [SerializeField] private Image _artImage;
    [SerializeField] private Text _iconText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Text _nameText;
    [SerializeField] private Image _dimImage;
    [SerializeField] private GameObject _plannedMark;

    [Header("演出")]
    [SerializeField] private float _dimAlpha = 0.62f;
    [SerializeField] private float _selectedLift = 30f;
    [SerializeField] private float _selectedScale = 1.1f;
    [SerializeField] private float _hoverScale = 1.04f;
    [SerializeField] private float _followSpeed = 14f;

    private bool _isSelectable;
    private bool _isSelected;
    private bool _isHover;
    private float _targetDim;

    public int Day { get; private set; }
    public int Column { get; private set; }
    public bool IsSelectable => _isSelectable;

    public void SetCard(StageCardInfo info, int day, int column)
    {
        Day = day;
        Column = column;
        _artImage.color = info.Color;
        _nameText.text = info.Name;
        _iconImage.sprite = info.Sprite;
        _iconImage.gameObject.SetActive(info.Sprite != null);
        _iconText.gameObject.SetActive(info.Sprite == null);
        _iconText.text = info.Icon;
    }

    /// <param name="isDimmed">今日選べない、またはたどり着けないカードは暗くする</param>
    /// <param name="isSelectable">今日選べるカードか</param>
    public void SetState(bool isDimmed, bool isSelectable, bool isSelected, bool isPlanned)
    {
        _isSelectable = isSelectable;
        _isSelected = isSelected;
        _targetDim = isDimmed ? _dimAlpha : 0f;
        _plannedMark.SetActive(isPlanned);
        if (isSelected) transform.SetAsLastSibling();
    }

    /// <summary>
    /// 状態を変えたあと、補間せずに見た目を合わせる
    /// </summary>
    public void SnapVisual()
    {
        _dimImage.color = new Color(0f, 0f, 0f, _targetDim);
        _body.anchoredPosition = TargetPosition();
        _body.localScale = Vector3.one * TargetScale();
    }

    /// <summary>
    /// 盤面から取り出して拡大表示するときは、盤面のくぼみを消す
    /// </summary>
    public void DetachFromBoard()
    {
        _slot.SetActive(false);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    private void Update()
    {
        var t = 1f - Mathf.Exp(-_followSpeed * Time.unscaledDeltaTime);
        _body.anchoredPosition = Vector2.Lerp(_body.anchoredPosition, TargetPosition(), t);
        _body.localScale = Vector3.Lerp(_body.localScale, Vector3.one * TargetScale(), t);

        var dim = _dimImage.color;
        dim.a = Mathf.Lerp(dim.a, _targetDim, t);
        _dimImage.color = dim;

        //選べるカードはゆっくり光らせ、選択中のカードは強く脈打たせる
        var glow = _glowImage.color;
        var wave = (Mathf.Sin(Time.unscaledTime * (_isSelected ? 6f : 2.5f)) + 1f) * 0.5f;
        glow.a = _isSelected ? Mathf.Lerp(0.65f, 1f, wave) : _isSelectable ? Mathf.Lerp(0.1f, 0.35f, wave) : 0f;
        _glowImage.color = glow;
    }

    private Vector2 TargetPosition()
    {
        return new Vector2(0f, _isSelected ? _selectedLift : 0f);
    }

    private float TargetScale()
    {
        if (_isSelected) return _selectedScale;
        return _isSelectable && _isHover ? _hoverScale : 1f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHover = false;
    }
}

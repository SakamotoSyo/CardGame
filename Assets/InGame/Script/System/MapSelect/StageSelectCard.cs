using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using UniRx;

public class StageSelectCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<StageType, MapPosData> OnClick;

    [Header("カーソルを合わせたときのカードの大きさ倍率")]
    [SerializeField] private float _cardPickSize;
    [SerializeField] private RectTransform _rectpos;
    [SerializeField] private CardController _controller;
    [SerializeField] private UIOutline _uiLine;
    [SerializeField] private GameObject _cardDirectionObj;
    [SerializeField] private Transform _cardDirectionInsPos;
    [SerializeField] private Image _cardImage;
    [SerializeField] private Text _cardText;

    [Tooltip("このカードのPosition")]
    private MapPosData _cardMapPosition;
    [Tooltip("")]
    private Vector3 _localScale;
    private StageType _stageType;
    private bool _isNext;
    private GameObject _insEffectCardObj;
    private Tween _tween;

    private void Awake()
    {
        _localScale = _rectpos.localScale;
    }

    private void Start()
    {
        //選択できるカードを強調する
        if (!_uiLine) return;
        transform.DOScale(new Vector3(_cardPickSize, _cardPickSize, _cardPickSize), 1f)
                 .SetLink(gameObject)
                 .SetLoops(-1, LoopType.Yoyo);
    }

    public void SetView(Sprite cardSprite, string cardText)
    {
        _cardImage.sprite = cardSprite;
        _cardText.text = cardText;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_uiLine || _isNext) return;
        _isNext = true;
        ViewpointShift().Forget();
        Debug.Log($"X{_cardMapPosition.X} Y{_cardMapPosition.Y}に移動");
    }

    /// <summary>
    /// カードを選んだ時に視点を移動
    /// </summary>
    private async UniTask ViewpointShift()
    {
        _ = DOTween.To(() => Camera.main.transform.position,
                   x => Camera.main.transform.position = x,
                   new Vector3(transform.position.x, transform.position.y + 700f, Camera.main.transform.position.z), 2f)
                   .SetLink(gameObject)
                   .SetEase(Ease.OutSine);
        var canvas = ScreenContainer.Find("OverRayCanvas");
        _insEffectCardObj = Instantiate(_cardDirectionObj, canvas.transform);
        var directionCs = _insEffectCardObj.GetComponent<StageSelectCardDirection>();
        directionCs.SetView(_cardImage.sprite, _cardText.text);
        await directionCs.StartMove();
        OnClick?.Invoke(_stageType, _cardMapPosition);
        Camera.main.transform.position = Vector3.zero;
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_uiLine) return;
        _tween = _uiLine.DOColor(new Color(_uiLine.color.r, _uiLine.color.g, _uiLine.color.b, 1), 1.5f)
               .SetLink(gameObject);
        _rectpos.localScale *= _cardPickSize;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_uiLine) return;
        _tween.Kill();
        _uiLine.color = new Color(_uiLine.color.r, _uiLine.color.g, _uiLine.color.b, 0.2f);
        _rectpos.localScale = _localScale;
    }

    public void SetStageType(StageType stageType)
    {
        _stageType = stageType;
    }

    /// <summary>
    /// このカードがMapのどこにいるかの情報をSetする
    /// </summary>
    public void SetCardMapPos(int x, int y)
    {
        _cardMapPosition.X = x;
        _cardMapPosition.Y = y;
    }

    private void OnDestroy()
    {
        Destroy(_insEffectCardObj);
    }
}

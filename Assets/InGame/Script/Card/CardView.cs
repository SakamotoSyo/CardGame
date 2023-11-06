using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardView : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CardAnimation CardAnimation => _cardAnimation;

    [SerializeField] private CardAnimation _cardAnimation;
    [SerializeField] Text _cardCost;
    [SerializeField] Text _cardName;
    [Tooltip("カードの説明文")]
    [SerializeField] Text _cardDescription;
    [SerializeField] Image _cardImage;
    [Header("カーソルを合わせたときのカードの大きさ倍率")]
    [SerializeField] private float _cardPickMagnification;
    [Header("カードをデフォルトの大きさ")]
    [SerializeField] private float _cardDefaultSize;
    [SerializeField] private RectTransform _cardRectpos;
    [SerializeField] private CardController _controller;
    [Tooltip("ドローした際のAnimationのPositionを変えれる")]
    [SerializeField] private Vector3 _drawAnimOffSet;

    private bool _moveFenish = true;
    private Vector3 _localScale;
    private Vector3 _savePos;

    /// <summary>
    /// カードの情報をセットする
    /// Modelに情報が入ったらPresenterを通して呼ばれる関数
    /// </summary>
    /// <param name="card">カードの情報</param>
    public void SetInfo(CardData card)
    {
        if (card == null) return;
        _cardCost.text = card.Cost.ToString();
        _cardName.text = card.Name;
        //_cardImage.sprite = card.CardSprite;
        _cardDescription.text = card.Description;

    }

    public void DrawAnim(Transform transform)
    {
        DOTween.To(() => transform.localPosition - _drawAnimOffSet,
           x => transform.localPosition = x,
           transform.localPosition, 0.5f)
          .OnStart(() => _moveFenish = true)
          .OnComplete(() => 
          {
              _localScale = _cardRectpos.localScale;
              _moveFenish = false;
          } );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_moveFenish)
        {
           // _cardRectpos.position = eventData.position;
        }
    }

    private void OnMouseDrag()
    {
        Vector3 objPos = Camera.main.WorldToScreenPoint(transform.position);

        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, objPos.z);

        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_moveFenish)
        {
            _savePos = transform.localPosition;
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_moveFenish) return;
        //クリックの終了時クリックし始めたpositionまで戻る
        DOTween.To(() => transform.localPosition,
        x => transform.localPosition = x,
        _savePos, 0.5f)
        .OnStart(() => _moveFenish = true)
        .SetLink(gameObject)
        .OnComplete(() =>
        {
            transform.localPosition = _savePos;
            _moveFenish = false;
        });
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        _cardAnimation.SelectAnim(true);
        _cardRectpos.localScale *= _cardPickMagnification;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _cardAnimation.SelectAnim(false);
        _cardRectpos.localScale = new Vector3(_cardDefaultSize, _cardDefaultSize, _cardDefaultSize);
    }
}

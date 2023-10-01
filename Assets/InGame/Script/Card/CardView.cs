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
    [Tooltip("�J�[�h�̐�����")]
    [SerializeField] Text _cardDescription;
    [SerializeField] Image _cardImage;
    [Header("�J�[�\�������킹���Ƃ��̃J�[�h�̑傫���{��")]
    [SerializeField] private float _cardPickSize;
    [SerializeField] private RectTransform _cardRectpos;
    [SerializeField] private CardController _controller;
    [Tooltip("�h���[�����ۂ�Animation��Position��ς����")]
    [SerializeField] private Vector3 _drawAnimOffSet;

    private bool _moveFenish = true;
    private Vector3 _localScale;
    private Vector3 _savePos;

    /// <summary>
    /// �J�[�h�̏����Z�b�g����
    /// Model�ɏ�񂪓�������Presenter��ʂ��ČĂ΂��֐�
    /// </summary>
    /// <param name="card">�J�[�h�̏��</param>
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
            _cardRectpos.position = eventData.position;
        }
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
        //�N���b�N�̏I�����N���b�N���n�߂�position�܂Ŗ߂�
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
        if (_moveFenish) return;
        _cardAnimation.SelectAnim(true);
        _cardRectpos.localScale *= _cardPickSize;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_moveFenish) return;
        _cardAnimation.SelectAnim(false);
        _cardRectpos.localScale = _localScale;
    }
}

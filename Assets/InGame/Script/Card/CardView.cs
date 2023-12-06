using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;

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
    [SerializeField] private float _cardPickMagnification;
    [Header("�J�[�h���f�t�H���g�̑傫��")]
    [SerializeField] private float _cardDefaultSize;
    [SerializeField] private RectTransform _cardRectpos;
    [Tooltip("�h���[�����ۂ�Animation��Position��ς����")]
    [SerializeField] private Vector3 _drawAnimOffSet;
    [SerializeField] private GameObject _cardEffectActivated;

    private bool _moveFenish = true;
    private Vector3 _localScale;
    private Vector3 _savePos;

    /// <summary>
    /// �J�[�h�̏����Z�b�g����
    /// Model�ɏ�񂪓�������Presenter��ʂ��ČĂ΂��֐�
    /// </summary>
    /// <param name="card">�J�[�h�̏��</param>
    public async UniTask SetInfo(CardData card)
    {
        if (card != null)
        {
            _cardCost.text = card.Cost.ToString();
            _cardName.text = card.Name;
            //_cardImage.sprite = card.CardSprite;
            _cardDescription.text = card.Description;
        }
        else
        {
            await _cardAnimation.ThrowAnim(transform, this.GetCancellationTokenOnDestroy());
            Destroy(gameObject);
        }
    }

    public void DrawAnim()
    {
        DOTween.To(() => transform.localPosition - _drawAnimOffSet,
            x => transform.localPosition = x,
            transform.localPosition, 0.5f)
           .OnStart(() => _moveFenish = true)
           .SetLink(gameObject)
           .OnComplete(() =>
           {
              _localScale = _cardRectpos.localScale;
              _moveFenish = false;
           });
    }

    public void EffectActivated()
    {
        Instantiate(_cardEffectActivated, 
             transform.position, 
             transform.rotation);
        Destroy(gameObject);
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
        _cardAnimation.SelectAnim(true);
        _cardRectpos.localScale *= _cardPickMagnification;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _cardAnimation.SelectAnim(false);
        _cardRectpos.localScale = new Vector3(_cardDefaultSize, _cardDefaultSize, _cardDefaultSize);
    }
}

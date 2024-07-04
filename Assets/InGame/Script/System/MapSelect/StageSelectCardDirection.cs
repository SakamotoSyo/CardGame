//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

/// <summary>
/// ステージ選択したときのカード演出をさせるためのクラス
/// </summary>
public class StageSelectCardDirection : MonoBehaviour
{
    [SerializeField] private Image _cardImage;
    [SerializeField] private Text _cardText;

    public void SetView(Sprite cardSprite, string cardText)
    {
        _cardImage.sprite = cardSprite;
        _cardText.text = cardText;
    }

    public async UniTask StartMove()
    {
        await DOTween.To(() => transform.position,
                   x => transform.position = x,
                   new Vector3(transform.position.x, transform.position.y + 500f, transform.position.z), 1f)
                  .SetLink(gameObject)
                  .SetEase(Ease.InOutBack);

        await transform.DOScale(4f, 1f)
                   .SetLink(gameObject)
                   .SetEase(Ease.InOutBack);
    }
}

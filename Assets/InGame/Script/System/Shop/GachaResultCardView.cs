//日本語対応
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ガチャ結果のカード1枚。裏向きで出て、めくると表(とNEWの印)が見える
/// </summary>
public class GachaResultCardView : MonoBehaviour
{
    [SerializeField] private RectTransform _body;
    [SerializeField] private DeckCardFaceView _face;
    [SerializeField] private GameObject _back;
    [SerializeField] private GameObject _newBadge;
    [Tooltip("めくったときに後ろで光らせる。色はレアリティに合わせる")]
    [SerializeField] private Image _glow;

    private GachaResult _result;

    public void SetResult(GachaResult result, DeckBuildSettings settings)
    {
        _result = result;
        _face.SetCard(result.Card, settings);
        var color = settings.GetRarityColor(result.Card.Rare);
        color.a = 0f;
        _glow.color = color;
        _face.gameObject.SetActive(false);
        _back.SetActive(true);
        _newBadge.SetActive(false);
        _body.localScale = Vector3.zero;
    }

    //Appear・Revealは画面側のSequenceに入れて使うので、ここではSetLinkしない(親のSequenceで付ける)

    /// <summary>
    /// 裏向きのまま飛び出してくる
    /// </summary>
    public Tween Appear()
    {
        return _body.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// 横に縮めて裏返し、表にして戻す
    /// </summary>
    public Sequence Reveal()
    {
        return DOTween.Sequence()
            .Append(_body.DOScaleX(0f, 0.12f).SetEase(Ease.InQuad))
            .AppendCallback(() =>
            {
                _back.SetActive(false);
                _face.gameObject.SetActive(true);
                _newBadge.SetActive(_result.IsNew);
            })
            .Append(_body.DOScaleX(1f, 0.15f).SetEase(Ease.OutQuad))
            .Join(_glow.DOFade(0.8f, 0.15f))
            .Append(_glow.DOFade(0.35f, 0.4f));
    }
}

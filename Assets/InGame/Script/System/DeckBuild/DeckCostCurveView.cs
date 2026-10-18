//日本語対応
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デッキのコスト分布(マナカーブ)の棒グラフ
/// </summary>
public class DeckCostCurveView : MonoBehaviour
{
    [Tooltip("コスト0から順に並べる。最後の棒はそれ以上のコストをまとめたもの")]
    [SerializeField] private RectTransform[] _bars;
    [SerializeField] private Text[] _countTexts;
    [SerializeField] private float _maxBarHeight = 90f;
    [Tooltip("棒が無いように見えないよう、0枚でも少しだけ出す")]
    [SerializeField] private float _minBarHeight = 4f;

    public int ColumnCount => _bars.Length;

    public void SetCounts(int[] counts)
    {
        var max = 1;
        foreach (var c in counts) max = Mathf.Max(max, c);

        for (int i = 0; i < _bars.Length; i++)
        {
            var count = i < counts.Length ? counts[i] : 0;
            var height = Mathf.Max(_minBarHeight, _maxBarHeight * count / max);
            _bars[i].DOKill();
            _bars[i].DOSizeDelta(new Vector2(_bars[i].sizeDelta.x, height), 0.25f).SetEase(Ease.OutCubic).SetLink(gameObject);
            //棒と数字の数がずれていても範囲外を触らない
            if (i < _countTexts.Length) _countTexts[i].text = count > 0 ? count.ToString() : "";
        }
    }
}

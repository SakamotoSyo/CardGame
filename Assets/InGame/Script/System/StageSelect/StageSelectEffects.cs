//日本語対応
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステージ選択画面の光の粒や画面フラッシュなどの演出
/// </summary>
public class StageSelectEffects : MonoBehaviour
{
    [SerializeField] private RectTransform _layer;
    [SerializeField] private Image _sparkTemplate;
    [SerializeField] private Image _flashImage;

    private void Awake()
    {
        _sparkTemplate.gameObject.SetActive(false);
        _flashImage.color = new Color(1f, 1f, 1f, 0f);
        _flashImage.raycastTarget = false;
    }

    /// <summary>
    /// 指定した位置から光の粒を飛び散らせる
    /// </summary>
    public void Burst(Vector3 worldPosition, int count, float radius, Color color)
    {
        var center = (Vector2)_layer.InverseTransformPoint(worldPosition);
        for (int i = 0; i < count; i++)
        {
            var spark = Instantiate(_sparkTemplate, _layer);
            spark.gameObject.SetActive(true);
            spark.color = color;
            var rect = spark.rectTransform;
            rect.anchoredPosition = center + Random.insideUnitCircle * 30f;
            var size = Random.Range(14f, 34f);
            rect.sizeDelta = new Vector2(size, size);

            var dir = Random.insideUnitCircle.normalized;
            var duration = Random.Range(0.45f, 0.8f);
            DOTween.Sequence()
                .Join(rect.DOAnchorPos(center + dir * Random.Range(radius * 0.5f, radius) + Vector2.up * 40f, duration).SetEase(Ease.OutCubic))
                .Join(rect.DOScale(0.2f, duration).SetEase(Ease.InQuad))
                .Join(spark.DOFade(0f, duration).SetEase(Ease.InQuad))
                .SetLink(spark.gameObject)
                .OnComplete(() => Destroy(spark.gameObject));
        }
    }

    public async UniTask Flash(float peakAlpha, float duration)
    {
        _flashImage.DOKill();
        await DOTween.Sequence()
            .Append(_flashImage.DOFade(peakAlpha, duration * 0.3f))
            .Append(_flashImage.DOFade(0f, duration * 0.7f))
            .SetLink(gameObject)
            .ToUniTask();
    }
}

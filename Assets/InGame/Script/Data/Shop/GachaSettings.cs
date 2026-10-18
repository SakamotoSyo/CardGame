//日本語対応
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガチャの値段・排出率と、ランで手に入るガチャ石の量
/// </summary>
[CreateAssetMenu(fileName = "GachaSettings", menuName = "SakamotoAsset/Shop/GachaSettings")]
public class GachaSettings : ScriptableObject
{
    [Header("ガチャ石の入手")]
    [Tooltip("ランが終わったときに、進んだ1日につきもらえるガチャ石")]
    public int StonePerDay = 1;

    [Header("値段")]
    public int SingleCost = 3;
    public int MultiCost = 30;
    [Tooltip("まとめて引くときの回数")]
    public int MultiCount = 10;

    [Header("排出率")]
    [Tooltip("レアリティ1から順に、そのレアリティのカードが出る重み。同じレアリティのカードは同じ確率で出る")]
    public List<float> RarityWeights = new List<float> { 70f, 22f, 6.5f, 1.5f };

    public float GetRarityWeight(int rare)
    {
        if (RarityWeights.Count == 0) return 1f;
        return Mathf.Max(0f, RarityWeights[Mathf.Clamp(rare - 1, 0, RarityWeights.Count - 1)]);
    }
}

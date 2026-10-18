//日本語対応
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// デッキ編集のルール(デッキの枚数と同じカードを入れられる枚数)とカードの見た目の設定
/// </summary>
[CreateAssetMenu(fileName = "DeckBuildSettings", menuName = "SakamotoAsset/DeckBuild/DeckBuildSettings")]
public class DeckBuildSettings : ScriptableObject
{
    [Header("ルール")]
    [Tooltip("この枚数未満のデッキは保存できない")]
    public int MinDeckCount = 3;
    [Tooltip("この枚数を超えてカードを追加できない")]
    public int MaxDeckCount = 30;
    [Tooltip("同じカードを入れられる枚数")]
    public int MaxSameCardCount = 3;

    [Header("見た目")]
    [Tooltip("コスト分布で、この値以上のコストをまとめて1本にする")]
    public int MaxCostColumn = 6;
    public Sprite AttackIcon;
    public Sprite DefenceIcon;
    public Sprite SpecialIcon;
    [Tooltip("レアリティ1から順に、カード下の帯の色")]
    public List<Color> RarityColors = new List<Color>();

    public Sprite GetIcon(EffectType type)
    {
        switch (type)
        {
            case EffectType.Attack: return AttackIcon;
            case EffectType.Defence: return DefenceIcon;
            default: return SpecialIcon;
        }
    }

    public Color GetRarityColor(int rare)
    {
        if (RarityColors.Count == 0) return Color.gray;
        return RarityColors[Mathf.Clamp(rare - 1, 0, RarityColors.Count - 1)];
    }

    public static string GetEffectTypeName(EffectType type)
    {
        switch (type)
        {
            case EffectType.Attack: return "攻撃";
            case EffectType.Defence: return "防御";
            default: return "特殊";
        }
    }
}

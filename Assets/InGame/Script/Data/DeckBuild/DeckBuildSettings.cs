//日本語対応
using UnityEngine;

/// <summary>
/// デッキ編集のルール(デッキの枚数と同じカードを入れられる枚数)
/// </summary>
[CreateAssetMenu(fileName = "DeckBuildSettings", menuName = "SakamotoAsset/DeckBuild/DeckBuildSettings")]
public class DeckBuildSettings : ScriptableObject
{
    [Tooltip("この枚数未満のデッキは保存できない")]
    public int MinDeckCount = 3;
    [Tooltip("この枚数を超えてカードを追加できない")]
    public int MaxDeckCount = 30;
    [Tooltip("同じカードを入れられる枚数")]
    public int MaxSameCardCount = 3;
}

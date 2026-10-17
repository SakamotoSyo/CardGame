//日本語対応
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージ選択マップの生成ルールとカードの見た目の設定
/// </summary>
[CreateAssetMenu(fileName = "StageMapSettings", menuName = "SakamotoAsset/StageSelect/StageMapSettings")]
public class StageMapSettings : ScriptableObject
{
    [Tooltip("1日あたりのカードの枚数(横の列数)")]
    public int Width = 5;
    [Tooltip("何日ごとにボス戦が来るか")]
    public int BossInterval = 10;
    [Tooltip("初日に何列目から始めるか")]
    public int StartColumn = 2;
    [Tooltip("出現するカードの重み")]
    public List<StageWeight> Weights = new List<StageWeight>();
    [Tooltip("カードの種類ごとの表示内容")]
    public List<StageCardInfo> CardInfos = new List<StageCardInfo>();

    public StageCardInfo GetInfo(StageType type)
    {
        var info = CardInfos.Find(c => c.Type == type);
        return info ?? new StageCardInfo { Type = type, Name = type.ToString(), Icon = "?", Color = Color.gray };
    }
}

[Serializable]
public sealed class StageWeight
{
    public StageType Type;
    public float Weight = 1f;
    [Tooltip("この日数以降でないと出現しない")]
    public int MinDay = 1;
}

[Serializable]
public sealed class StageCardInfo
{
    public StageType Type;
    public string Name;
    [Tooltip("カードの中央に大きく表示する文字")]
    public string Icon;
    [TextArea] public string Description;
    public Color Color = Color.gray;
    [Tooltip("設定するとアイコン文字の代わりに表示する")]
    public Sprite Sprite;
}

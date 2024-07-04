//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GenerateMapData", menuName = "SakamotoAsset/MapSelect/GenerateMapData")]
public class GenerateMapSettingData : ScriptableObject
{
    [Tooltip("何階層ごとにマップを生成するか")]
    public int MapGenerationNum;
    [Tooltip("一階層ごとに何部屋増やすか")]
    public int IncreaseRoomNum;
    [Tooltip("最小の部屋の数")]
    public int MinRoomNum;
    [Tooltip("最大の部屋の数")]
    public int MaxRoomNum;
    public List<StageGachaData> StageGachaList = new List<StageGachaData>();
}

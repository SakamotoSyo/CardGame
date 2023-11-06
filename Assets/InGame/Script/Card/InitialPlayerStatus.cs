//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 初期のステータスを設定するクラス
/// </summary>
[CreateAssetMenu(fileName = "InitialStatus", menuName = "Scriptable/InitialStatus")]
public class InitialPlayerStatus : ScriptableObject
{
    public int MaxHp;
    public int MaxCost;
    public List<int> CardId = new List<int>();
}

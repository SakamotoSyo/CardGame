//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Scriptable/EffectMasterTableAsset")]
public class EffectMasterTableAsset : ScriptableObject
{
    public EffectMasterTable MasterTable => _masterTable;
    [SerializeField] private EffectMasterTable _masterTable = new EffectMasterTable();
}

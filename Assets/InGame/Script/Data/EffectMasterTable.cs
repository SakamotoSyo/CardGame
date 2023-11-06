//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class EffectMasterTable
{
    [SerializeField] private List<EffectMaster> _effectData = new List<EffectMaster>();
    private Dictionary<string, EffectMaster> _effectDataDic;

    public EffectMaster FindById(string id)
    {
        if (_effectDataDic == null) Init();
        return !_effectDataDic.TryGetValue(id, out var effectData) ? null : effectData;
    }

    public void Init() 
    {
        _effectDataDic = _effectData.ToDictionary(x => x.Id);
    }
}

//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EffectMaster
{
    public string Id;
    [SubclassSelector, SerializeReference]
    public List<IEffect> EffectList = new List<IEffect>();
    public EffectType EffectType;

    public float GetTotalPower() 
    {
        var sum = 0f;
        for (int i = 0; i < EffectList.Count; i++) 
        {
            sum += EffectList[i].EffectPower;
        }
        return sum;
    }
}

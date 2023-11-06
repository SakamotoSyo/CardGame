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
}

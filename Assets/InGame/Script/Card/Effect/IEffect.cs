//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffect
{
    public EffectType EffectType { get;} 
    public float Power { get;}
    public void Execute(BattleEnviroment env, TargetType targetType);
}

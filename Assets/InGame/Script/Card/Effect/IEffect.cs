//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffect
{
    public EffectType EffectType { get;} 
    public float EffectPower { get; set; }
    public void Execute(BattleEnviroment env, TargetType targetType);
    /// <summary>
    /// 効果マスタの効果は同じEffectIdのカードや敵と共有されているので、威力を書き換える前に複製する
    /// </summary>
    public IEffect Clone();
}

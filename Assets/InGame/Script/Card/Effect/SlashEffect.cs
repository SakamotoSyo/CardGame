//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SlashEffect : IEffect
{
    public EffectType EffectType => _effectType;
    private EffectType _effectType = EffectType.Attack;
    public float EffectPower { get; set; }

    public void Execute(BattleEnviroment env, TargetType targetType)
    {
        if (TargetType.Player == targetType) 
        {
            env.PlayerStatus.CurrentHp.Value = Mathf.Max(0, env.PlayerStatus.CurrentHp.Value - EffectPower);
        }
        else if(TargetType.Enemy == targetType)
        {
            var target = env.GetTargetEnemy();
            if (target == null) return;
            target.CurrentHp.Value = Mathf.Max(0, target.CurrentHp.Value - EffectPower);
        }
    }

    public IEffect Clone()
    {
        return (IEffect)MemberwiseClone();
    }
}

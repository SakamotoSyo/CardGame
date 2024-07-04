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
            env.PlayerStatus.CurrentHp.Value -= EffectPower; 
        }
        else if(TargetType.Enemy == targetType)
        {
            env.EnemyStatusList[env.TargetEnemy].CurrentHp.Value -= EffectPower;
        }
    }
}

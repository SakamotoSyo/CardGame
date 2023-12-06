//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashEffect : IEffect
{
    public EffectType EffectType => _effectType;
    private EffectType _effectType = EffectType.Attack;
    public float Power => _damage;
    private float _damage = 10;

    public void Execute(BattleEnviroment env, TargetType targetType)
    {
        if (TargetType.Player == targetType) 
        {
            env.PlayerStatus.CurrentHp.Value -= _damage; 
        }
        else if(TargetType.Enemy == targetType)
        {
            env.EnemyStatusList[env.TargetEnemy].CurrentHp.Value -= _damage;
        }
    }
}

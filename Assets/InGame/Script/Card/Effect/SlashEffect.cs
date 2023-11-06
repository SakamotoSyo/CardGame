//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashEffect : IEffect
{
    private float _damage = 10;
    public void Execute(CardEnviroment env)
    {
        env.PlayerStatus.CurrentHp.Value -= _damage;
    }
}

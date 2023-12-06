//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public interface IEnemyStatus
{
    public IReactiveCollection<EffectMaster> EnemyTurnEffect { get; }
    public IObservable<EffectMaster> OnAttack { get; }
    public IReactiveProperty<float> DefenceNum { get; }
    public IReactiveProperty<float> MaxHp { get; }
    public IReactiveProperty<float> CurrentHp { get; }
    public IReactiveProperty<float> AttackNum { get; }
    public void AttackExecute();
    public void AttackEnd();
}

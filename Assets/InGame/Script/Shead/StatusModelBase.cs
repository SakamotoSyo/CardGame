using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public abstract class StatusModelBase : DisposeState
{
    public IReactiveProperty<float> MaxHp => _maxHp;
    protected readonly ReactiveProperty<float> _maxHp = new();

    public IReactiveProperty<float> CurrentHp => _currentHp;
    protected readonly ReactiveProperty<float> _currentHp = new();

    public IReactiveProperty<float> Attack => _attack;
    protected readonly ReactiveProperty<float> _attack = new();
    public IReactiveProperty<float> Defence => _defence;
    protected readonly ReactiveProperty<float> _defence = new();

    #region Set関数

    public virtual void AddDamage(float value)
    {
        var num = (int)value - _defence.Value;

        if (0 < num)
        {
            var hpNum = Mathf.Max(0, _currentHp.Value - num);
            _currentHp.Value = hpNum;
            _defence.Value = 0;
        }
        else
        {
            _defence.Value = num * -1;
        }
    }

    public virtual void Healing(float value)
    {
        var healNum = Mathf.Min(_currentHp.Value + value, _maxHp.Value);
        _currentHp.Value = healNum;
    }
    #endregion


    /// <summary>
    /// 攻撃でダウンするか判定する
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public bool DownJudge(float damage)
    {
        return 0 < _currentHp.Value + _defence.Value - damage;
    }

    protected override void DisposeInternal() 
    {
        _maxHp.Dispose();
        _currentHp.Dispose();
        _attack.Dispose();
        _defence.Dispose();
    }
}

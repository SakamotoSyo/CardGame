//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Random = UnityEngine.Random;

public class EnemyStatus : StatusModelBase, IEnemyStatus
{
    private EnemyData _enemyStatus;
    public IReactiveCollection<EffectMaster> EnemyTurnEffect => _enemyTurnEffect;
    [Tooltip("１ターンの間にやる行動")]
    private readonly ReactiveCollection<EffectMaster> _enemyTurnEffect = new ReactiveCollection<EffectMaster>();
    public IObservable<EffectMaster> OnAttack => _onAttack;
    private Subject<EffectMaster> _onAttack = new Subject<EffectMaster>();
    private BattleEnviroment _env;

    private int _actionNum;

    public EnemyStatus(EnemyData enemyData, BattleEnviroment env)
    {
        _enemyStatus = enemyData;
        _maxHp.Value = enemyData.EnemyHp;
        _currentHp.Value = enemyData.EnemyHp;
        _env = env;
    }

    public void StatusSet(EnemyStatusData enemy)
    {
        //_maxHp.Value = Mathf.Floor(enemy.MaxHp * EffectMagnifivationNum(enemy.BaseCurrentLevel));
        //_currentHp.Value = _maxHp.Value;
        //_actionNum = enemy.ActionNum;
        //_effectDataList = new List<EnemyEffectData>(enemy.enemyEffectDataList);
        //for (int i = 0; i < _effectDataList.Count; i++)
        //{
        //    _effectDataList[i].EffectPowerMultiplication(EffectMagnifivationNum(enemy.BaseCurrentLevel));
        //}
    }

    /// <summary>
    /// このターンの行動を決定する
    /// </summary>
    public void AttackDecision()
    {
        for (int i = 0; i < _enemyStatus.ActionsNum; i++)
        {
            _enemyTurnEffect.Add(_enemyStatus.EffectList[Random.Range(0, _enemyStatus.EffectList.Count)]);
        }
    }

    /// <summary>
    /// 攻撃を実行する
    /// </summary>
    public void AttackExecute()
    {
        try
        {
            for (int i = 0; i < _enemyTurnEffect[0].EffectList.Count; i++)
            {
                _enemyTurnEffect[0].EffectList[i].Execute(_env, TargetType.Player);
            }
        }
        catch 
        {
            Debug.Log("#a");
        }
       
        _onAttack.OnNext(_enemyTurnEffect[0]);
    }

    public void AttackEnd() 
    {
        _enemyTurnEffect.RemoveAt(0);
        if (_enemyTurnEffect.Count == 0) return;
        AttackExecute();
    }

   

    public void AttackDecisionReset()
    {
        for (int i = 0; i < _enemyTurnEffect.Count; i++)
        {
            _enemyTurnEffect.RemoveAt(0);
        }
    }

    public IReactiveCollection<EffectMaster> GetEnemyTurnEffectOb()
    {
        return _enemyTurnEffect;
    }

    /// <summary>
    /// 階層にあったStatusを上昇させる倍率を返してくれる
    /// </summary>
    /// <param name="BaseLevel"></param>
    /// <returns></returns>
    public float EffectMagnifivationNum(int BaseLevel)
    {
        return default;
        //return 1 + ((GameManager.CurremtLevel + 1) - BaseLevel) * DataBaseScript.EFFECT_MAGNIFICATION;
    }

    public ReactiveCollection<EffectMaster> GetEnemyTurnEffect()
    {
        return _enemyTurnEffect;
    }
}
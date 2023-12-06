//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;
using Cysharp.Threading.Tasks;

public class EnemyPresenter : IDisposable
{
    private IEnemyStatus _enemyStatus;
    private EnemyView _enemyView;
    private CompositeDisposable _compositeDisposable = new();

    public EnemyPresenter(IEnemyStatus enemyStatus, EnemyView enemyView) 
    {
        _enemyStatus = enemyStatus;
        _enemyView = enemyView;
    }

    public void SetUp() 
    {
        _enemyStatus.DefenceNum.Subscribe(_enemyView.SetDefense).AddTo(_compositeDisposable);
        _enemyStatus.MaxHp.Subscribe(_enemyView.MaxHpSet).AddTo(_compositeDisposable);
        _enemyStatus.CurrentHp.Subscribe(_enemyView.SetHpCurrent).AddTo(_compositeDisposable);
        _enemyStatus.EnemyTurnEffect.ObserveAdd().Subscribe(x => _enemyView.AttackDecisionView(x.Value)).AddTo(_compositeDisposable);
        _enemyStatus.OnAttack.Subscribe(_ => _enemyView.DeleteIcon().Forget()).AddTo(_compositeDisposable);
        _enemyView.AttackAnimEnd.Subscribe(_ => _enemyStatus.AttackEnd()).AddTo(_compositeDisposable);
    }

    public void Dispose()
    {
        
    }
}

//日本語対応
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using State = StateMachine<BattleTurnController>.State;

public class EnemyTurnState : State
{
    protected override async UniTask OnEnterAsync(State currentState, CancellationToken token)
    {
        await Owner.BattleStateView.TrunAnimAsync(this);
        for (int i = 0; i < Owner.OrderOfAction.Count; i++)
        {
            //行動を決めたあとに倒された敵は、決めていた行動を取りやめる
            if (Owner.OrderOfAction[i].EnemyStatus.IsDead)
            {
                Owner.OrderOfAction[i].EnemyStatus.AttackDecisionReset();
                continue;
            }
            Owner.OrderOfAction[i].EnemyStatus.AttackExecute();
            await UniTask.WaitUntil(() => Owner.OrderOfAction[i].EnemyStatus.EnemyTurnEffect.Count == 0
            , cancellationToken: token);
        }

        Owner.OrderOfAction.Clear();
        Owner.ChangeState(BattleTurnController.TurnStateType.SelectNextActorTransitionState);
    }

    protected override void OnExit(State nextState)
    {

    }

}

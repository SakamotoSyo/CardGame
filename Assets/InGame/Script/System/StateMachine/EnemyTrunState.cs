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
        for (int i = 0; i < Owner.OrderOfAction.Count; i++) 
        {
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

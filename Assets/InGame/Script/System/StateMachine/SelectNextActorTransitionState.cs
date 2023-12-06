//日本語対応
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using State = StateMachine<BattleTurnController>.State;

public class SelectNextActorTransitionState : State
{
    protected override void OnEnter(State currentState)
    {
        if (Owner.OrderOfAction.Count == 0)
        {
            Owner.ActionSequentialDetermining();
            Debug.Log("順番を決めなおします"); 
        }

        if (Owner.OrderOfAction[0].PlayerStatus == null)
        {
            Owner.ChangeState(BattleTurnController.TurnStateType.EnemyState);
            Debug.Log("EnemyState");
        }
        else if (Owner.OrderOfAction[0].EnemyStatus == null)
        {
            Owner.ChangeState(BattleTurnController.TurnStateType.PlayerState);
            Debug.Log("PlayerState");
        }
        else
        {
            Debug.LogError("turnの遷移が出来ません");
        }
    }
}

//日本語対応
using System.Collections;
using System.Collections.Generic;
using State = StateMachine<BattleTurnController>.State;
using UnityEngine;
using System.Threading;

public class PlayerTurnState : State
{
    protected override void OnEnter(State currentState)
    {
        Owner.BattleStateView.TurnAnim(this);
        Owner.PlayerStatus.DrawCard(3);
        Owner.PlayerStatus.ResetCost();
        Owner.OrderOfAction.RemoveAt(0);
    }

    protected override void OnExit(State nextState)
    {
        Debug.Log("turnを抜けます");
        Owner.PlayerStatus.DiscardAllHandCards();
    }
}

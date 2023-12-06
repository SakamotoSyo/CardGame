//日本語対応
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using State = StateMachine<BattleTurnController>.State;

public class BattleStartState : State
{
    protected override void OnEnter(State currentState)
    {
        Owner.ChangeState(BattleTurnController.TurnStateType.StartSelect);
    }

    protected override void OnUpdate()
    {
        
    }

    protected override void OnExit(State nextState)
    {
        
    }
}

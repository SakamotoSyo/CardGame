//日本語対応
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = StateMachine<BattleTurnController>.State;

public interface IBattleTurnController
{
    public State CurrentState{ get;}

    public UniTask Setup();
    public void ChangeState(BattleTurnController.TurnStateType turnStateType);
}

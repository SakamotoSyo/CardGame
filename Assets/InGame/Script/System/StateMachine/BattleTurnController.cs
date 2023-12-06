//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Cysharp.Threading;
using System.Threading;
using System;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;

public class BattleTurnController : IBattleTurnController, IDisposable
{
    public List<ActorAction> OrderOfAction => _orderOfAction;
    public IPlayerStatus PlayerStatus { get; private set; }
    public StateMachine<BattleTurnController>.State CurrentState => _turnStateMachine.CurrentState;
    public BattleStateView BattleStateView => _battleStateView;

    private BattleStateView _battleStateView;
    private StateMachine<BattleTurnController> _turnStateMachine;
    private readonly List<ActorAction> _orderOfAction = new List<ActorAction>();
    private CancellationTokenSource _tokenSouce = new();
    private BattleEnviroment _env;

    [Inject]
    public BattleTurnController(IPlayerStatus playerStatus, BattleEnviroment env)
    {
        PlayerStatus = playerStatus;
        _env = env;
    }

    public async UniTask Setup()
    {
        var result = await AssetLoader.LoadAssetAsync<GameObject>(ResourceKey.Prefabs.StateTrun);
        var obj = Object.Instantiate(result, MainUi.MainUiTransform);
        _battleStateView = obj.GetComponent<BattleStateView>();

        _turnStateMachine = new(this);
        _turnStateMachine.AddAnyTransition<PlayerTurnState>((int)TurnStateType.PlayerState);
        _turnStateMachine.AddAnyTransition<EnemyTurnState>((int)TurnStateType.EnemyState);
        _turnStateMachine.AddTransition<PlayerTurnState, SelectNextActorTransitionState>
                                         ((int)TurnStateType.SelectNextActorTransitionState);
        _turnStateMachine.AddTransition<EnemyTurnState, SelectNextActorTransitionState>
                                         ((int)TurnStateType.SelectNextActorTransitionState);
        _turnStateMachine.AddTransition<BattleStartState, SelectNextActorTransitionState>((int)TurnStateType.StartSelect);
        StartTurn();
    }

    private void StartTurn()
    {
        _turnStateMachine.Start<SelectNextActorTransitionState>(_tokenSouce.Token);
    }

    /// <summary>
    /// 行動順の決定
    /// 素早さが追加された時この辺りに追記する
    /// </summary>
    public void ActionSequentialDetermining()
    {
        _orderOfAction.Add(new ActorAction(_env.PlayerStatus));

        for (int i = 0; i < _env.EnemyStatusList.Count; i++) 
        {
            _orderOfAction.Add(new ActorAction(_env.EnemyStatusList[i]));
        }

        for (int i = 0; i < _orderOfAction.Count; i++)
        {
            if (_orderOfAction[i].EnemyStatus == null) continue;
            _orderOfAction[i].EnemyStatus.AttackDecision();
        }
    }


    public void ChangeState(TurnStateType turnStateType)
    {
        _turnStateMachine.Dispatch((int)turnStateType, _tokenSouce.Token);
    }

    public void Dispose()
    {
        _tokenSouce.Cancel();
        AssetLoader.Release(ResourceKey.Prefabs.StateTrun);
    }

    public enum TurnStateType
    {
        PlayerState,
        EnemyState,
        SelectNextActorTransitionState,
        StartSelect,
    }

    public struct ActorAction
    {
        public IPlayerStatus PlayerStatus;
        public EnemyStatus EnemyStatus;
        public bool AlreadyActed;
        public ActorAction(IPlayerStatus playerStatus)
        {
            PlayerStatus = playerStatus;
            EnemyStatus = null;
            AlreadyActed = false;
        }

        public ActorAction(EnemyStatus enemyStatus) 
        {
            PlayerStatus = null;
            EnemyStatus = enemyStatus;
            AlreadyActed = false;
        }
    }
}

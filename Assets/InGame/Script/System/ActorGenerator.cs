//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using VContainer.Unity;
using VContainer;
using System;
using System.Linq;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>
/// Actorを生成するクラス
/// </summary>
public sealed class ActorGenerator : MonoBehaviour, IDisposable
{
    public IPlayerStatus PlayerStatus => _playerStatus;

    [SerializeField] private Transform _playerParentTransform;
    [SerializeField] private Transform _enemyParentTransform;

    private PlayerView _playerView;
    private IPlayerStatus _playerStatus;
    private PlayerPresenter _playerPresenter;
    private List<EnemyStatus> _enemyDataList = new List<EnemyStatus>();
    private List<EnemyPresenter> _enemyPresenterList = new List<EnemyPresenter>();
    private IEnemyDataRepository _enemyDataRepository;
    private BattleEnviroment _env;
    private IBattleTurnController _battleTurnController;

    [Inject]
    public void Construct(IPlayerStatus playerStatus, 
        IEnemyDataRepository enemyDataRepository, 
        BattleEnviroment env,
        IBattleTurnController battleTurnController)
    {
        _playerStatus = playerStatus;
        _enemyDataRepository = enemyDataRepository;
        _env = env;
        _battleTurnController = battleTurnController;
    }

    public async UniTask Setup()
    {
        ChooseEnemy();
        await LoadView();
        _playerPresenter = new PlayerPresenter(_playerStatus, _playerView, _battleTurnController);
        await _playerPresenter.SetUp();
    }

    private void ChooseEnemy()
    {
        var enemyDatas = _enemyDataRepository.FindHierarchyManifestation(1);
        var enemyNum = Random.Range(1, 4);

        Debug.Log(enemyNum);
        for (int i = 0; i < enemyNum; i++) 
        {
            var num = Random.Range(0, enemyDatas.Length);
            _enemyDataList.Add(new EnemyStatus(enemyDatas[num], _env));
        }
        _env.EnemyStatusList = _enemyDataList;
    }

    /// <summary>
    /// Viewの部分を読み込み
    /// </summary>
    public async UniTask LoadView()
    {
        //Player
        var addressableHandle = Addressables.LoadAssetAsync<GameObject>(ResourceKey.Prefabs.PlayerView);
        var playerViewObj = addressableHandle.WaitForCompletion();
        _playerView = Instantiate(playerViewObj, _playerParentTransform).GetComponent<PlayerView>();
        Addressables.Release(addressableHandle);

        var obj = await AssetLoader.LoadAssetAsync<GameObject>(ResourceKey.Prefabs.EnemyView);
        //Enemy
        for (int i = 0; i < _enemyDataList.Count; i++) 
        {
            var enemyView = Instantiate(obj, _enemyParentTransform).GetComponent<EnemyView>();
            _env.EnemyViewList.Add(enemyView);
            var presenter = new EnemyPresenter(_enemyDataList[i], enemyView);
            _enemyPresenterList.Add(presenter);
            presenter.SetUp();
        }

        AssetLoader.Release(ResourceKey.Prefabs.EnemyView);
    }

    /// <summary>
    /// VContainerの機能でDisposeがインスタンス破棄時に呼ばれる
    /// </summary>
    public void Dispose()
    {
        _playerPresenter?.Dispose();
        _enemyPresenterList.ForEach(x => x.Dispose());
    }
}

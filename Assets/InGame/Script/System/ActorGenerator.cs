//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using VContainer.Unity;
using VContainer;
using System;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

/// <summary>
/// Actorを生成するクラス
/// </summary>
public sealed class ActorGenerator :MonoBehaviour, IDisposable
{
    public IPlayerStatus PlayerStatus => _playerStatus;

    [SerializeField] private Transform _playerParentTransform;
    [SerializeField] private Transform _enemyParentTransform;

    private PlayerView _playerView;
    private IPlayerStatus _playerStatus;
    private PlayerPresenter _playerPresenter;
    private EnemyView _enemyView;
    private EnemyStatus _enemyStatus;

    [Inject]
    public void Construct(IPlayerStatus playerStatus)
    {
        _playerStatus = playerStatus;
    }

    public async UniTask Setup()
    {
        LoadView();
        _playerPresenter = new PlayerPresenter(_playerStatus, _playerView);
        await _playerPresenter.SetUp();
        new EnemyPresenter(_enemyStatus, _enemyView);
    }

    /// <summary>
    /// Viewの部分を読み込み
    /// </summary>
    public void LoadView()
    {
        //Player
        var addressableHandle = Addressables.LoadAssetAsync<GameObject>(ResourceKey.Prefabs.PlayerView);
        var playerViewObj = addressableHandle.WaitForCompletion();
        _playerView = Instantiate(playerViewObj, _playerParentTransform).GetComponent<PlayerView>();
        Addressables.Release(addressableHandle);

        //Enemy
        _enemyStatus = new EnemyStatus();

    }

    /// <summary>
    /// VContainerの機能でDisposeがインスタンス破棄時に呼ばれる
    /// </summary>
    public void Dispose()
    {
        _playerPresenter?.Dispose();
    }
}

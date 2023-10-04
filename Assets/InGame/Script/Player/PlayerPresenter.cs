//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

public sealed class PlayerPresenter : IDisposable
{
    private IPlayerStatus _playerStatus;
    private PlayerView _playerView;
    private CompositeDisposable _compositeDisposable = new();
    public PlayerPresenter(IPlayerStatus playerStatus, PlayerView playerView)
    {
        _playerStatus = playerStatus;
        _playerView = playerView;
    }

    public async UniTask SetUp()
    {
        await _playerStatus.SetUp();
        _playerStatus.HandCardList.ObserveAdd().Subscribe(x => _playerView.DrawView(x.Value)).AddTo(_compositeDisposable);
        _playerStatus.GraveyardCards.ObserveCountChanged().Subscribe(_playerView.GraveyardCardsView).AddTo(_compositeDisposable);
        _playerStatus.DeckCardList.ObserveCountChanged().Subscribe(_playerView.DeckCardView).AddTo(_compositeDisposable);
        _playerStatus.Defence.Subscribe(_playerView.SetDefense).AddTo(_compositeDisposable);
        _playerStatus.Cost.Subscribe(x => _playerView.SetCostText(x).Forget()).AddTo(_compositeDisposable);
        _playerStatus.MaxHp.Subscribe(_playerView.MaxHpSet).AddTo(_compositeDisposable);
        _playerStatus.CurrentHp.Subscribe(_playerView.SetHpCurrent).AddTo(_compositeDisposable);
    }

    public void Dispose()
    {
        _compositeDisposable.Dispose();
    }
}

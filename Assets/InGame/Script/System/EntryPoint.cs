//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using VContainer;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.AddressableAssets;

public class EntryPoint : IAsyncStartable
{
    private MainUi _mainUi;
    private ActorGenerator _actorGenerator;
    private TransitionService _transitionService;
    private IEnemyDataRepository _enemyDataRepository;
    private ICardDataRepository _cardDataRepository;
    private IEffectDataRepository _effectDataRepository;
    private IDataLoader _dataLoader;

    public EntryPoint(MainUi mainUi, ActorGenerator actorGenerator,
                      IEnemyDataRepository enemyDataRepository,
                      ICardDataRepository cardDataRepository,
                      IEffectDataRepository effectDataRepository,
                      IDataLoader dataLoader,
                      TransitionService transitionService)
    {
        _mainUi = mainUi;
        _actorGenerator = actorGenerator;
        _dataLoader = dataLoader;
        _enemyDataRepository = enemyDataRepository;
        _cardDataRepository = cardDataRepository;
        _effectDataRepository = effectDataRepository;
        _transitionService = transitionService;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        Application.targetFrameRate = 60;
        await LoadData();
        _mainUi.SetUp();
        _transitionService.BattleStart();
    }

    private async UniTask LoadData()
    {
        List<UniTask> masterDataDownloads = new List<UniTask>()
        {
            _effectDataRepository.LoadData(_dataLoader),
            _cardDataRepository.LoadData(_dataLoader),
            _enemyDataRepository.LoadData(_dataLoader),
        };

        await masterDataDownloads;
    }
}

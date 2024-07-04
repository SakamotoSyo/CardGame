//日本語対応
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSelectScreen : ScreenBase
{
    [SerializeField] private List<StageMapView> _mapView;

    private IMapSelectPosRepository _mapSelectPos;
    private IDungeonMapDataRepository _mapDataRepository;
    private GenerationDungeonMap _generationDungeonMap = new();
    private TransitionService _transitionService;

    [VContainer.Inject]
    public void Construct(IMapSelectPosRepository mapSelectPosRepository, 
        IDungeonMapDataRepository dungeonMapDataRepository,
        TransitionService transitionService)
    {
        _mapSelectPos = mapSelectPosRepository;
        _mapDataRepository = dungeonMapDataRepository;
        _transitionService = transitionService;
    }

    private void Start()
    {
        UniTask.Create(async () =>
        {
            await LoadData();
            MapGenerate().Forget();
        });
    }

    private async UniTask LoadData()
    {
        await _mapSelectPos.LoadData();
        await _mapDataRepository.LoadData();
    }

    private async UniTask MapGenerate() 
    {
        await _generationDungeonMap.SetUp(_mapDataRepository);


        BindView();
        var mapPos = _mapSelectPos.GetValue();
        for (int i = 0; i < _mapView.Count; i++)
        {
            _mapView[i].SetUp(mapPos);
        }
    }

    private void BindView() 
    {
        for (int i = 0; i < _mapView.Count; i++) 
        {
            _mapView[i].OnCellClick += NextStage;
        }
    }

    private void NextStage(StageType stageType, MapPosData mapPosData) 
    {
        _mapSelectPos.Update(mapPosData);

        if (StageType.Battle == stageType) 
        {
            _transitionService.BattleStart().Forget();
        }
    }
}

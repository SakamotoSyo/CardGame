using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// �_���W�����̃}�b�v�𐶐����邽�߂̃N���X
/// </summary>
public class GenerationDungeonMap
{
    public static int[,] DungeonMapData => _dungeonMapData;

    private GenerateMapSettingData _data;
    private static int[,] _dungeonMapData;

    private IDungeonMapDataRepository _dungeonMapDataRepository;
    public async UniTask SetUp(IDungeonMapDataRepository dungeonMapDataRepository) 
    {
        _dungeonMapDataRepository = dungeonMapDataRepository;
        await LoadData();
        if (_dungeonMapDataRepository.GetValue().MapData == null)
        {
            MapGeneration();
        }
        Dump();
    }

    public async UniTask LoadData() 
    {
       _data = await AssetLoader.LoadAssetAsync<GenerateMapSettingData>(ResourceKey.MasterData.GenerateMapData);
    }

    ~GenerationDungeonMap() 
    {
        AssetLoader.Release(ResourceKey.MasterData.GenerateMapData);
    }

    /// <summary>
    /// �_���W�����̃}�b�v�𐶐�����
    /// </summary>
    public void MapGeneration()
    {
        _dungeonMapData = new int[_data.MapGenerationNum, _data.MaxRoomNum];
        _dungeonMapDataRepository.Update(new DungeonMapData(_dungeonMapData));

        var minRoomNum = _data.MinRoomNum;
        var maxRootNum = minRoomNum + _data.IncreaseRoomNum;
        for (int y = 0; y <  _data.MapGenerationNum; y++)
        {
            //�Ō�̊K�w�ɂ̓{�X�o�g����z�u����
            if (_data.MapGenerationNum - 1 == y) 
            {
                for(int x = 0; x < maxRootNum + 1; x++) 
                {
                    _dungeonMapData[y, x] = (int)StageType.Boss;
                } 
                return;
            }

            for (int x = minRoomNum; x < maxRootNum + 1; x++) 
            {
                _dungeonMapData[y, x] = StageChoose();
            }

            if (minRoomNum != 0) 
            {
                minRoomNum--;
                maxRootNum++;
            }
        }
    }

    /// <summary>
    ///�ݒ肵���m������X�e�[�W��I��
    /// </summary>
    public int StageChoose() 
    {
        var stageGachaList = _data.StageGachaList;
        //���v�̏d�݂��v�Z
        float total = 0;
        for (int i = 0; i < stageGachaList.Count; i++)
        {
            total += stageGachaList[i].Probability;
        }

        float randomNum = Random.value * total;
        float saveProbability = 0;
        //���I
        for(int i = 0; i < stageGachaList.Count; i++) 
        {
            if (randomNum < stageGachaList[i].Probability + saveProbability)
            {
                return (int)stageGachaList[i].StageType;
            }
            else 
            {
                saveProbability += stageGachaList[i].Probability;
            }
        }

        return (int)stageGachaList[stageGachaList.Count - 1].StageType;
    }

    /// <summary>
    /// Debug�p
    /// </summary>
    public void Dump() 
    {
        for (int i = 0; i < _data.MapGenerationNum; i++) 
        {
            var test = "";
            for (int j = 0; j < _data.MaxRoomNum; j++) 
            {
                test += _dungeonMapData[i, j];
            }
        }
    }
}

[System.Serializable]
public class StageGachaData
{
    public float Probability => _probability;
    public StageType StageType => _stageType;
    [SerializeField] private StageType _stageType;
    [SerializeField] private float _probability;
}

//日本語対応
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMapDataRepositoryMock : IDungeonMapDataRepository
{
    private DungeonMapData _dungeonMapData;

    public async UniTask LoadData()
    {
        if (LocalData.GetFileExists("MapData"))
        {
            _dungeonMapData = await LocalData.LoadAsyncData<DungeonMapData>("DungeonMapData");
        }
    }

    public DungeonMapData GetValue() 
    {
        return _dungeonMapData;
    } 

    public void Update(DungeonMapData dungeonMapData) 
    {
        _dungeonMapData = dungeonMapData;
    }
}

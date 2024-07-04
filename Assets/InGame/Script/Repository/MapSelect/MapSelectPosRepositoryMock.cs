//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MapSelectPosRepositoryMock : IMapSelectPosRepository
{
    private MapPosData _mapPosData;
    public async UniTask LoadData()
    {
        if (LocalData.GetFileExists("MapPosData"))
        {
            Debug.Log("ぉー度");
            _mapPosData = await LocalData.LoadAsyncData<MapPosData>("MapPosData");
        }
        else 
        {
            Debug.Log("Map新規");
            _mapPosData = new MapPosData(4, 0);
        }
    }

    public MapPosData GetValue()
    {
        return _mapPosData;
    }

    public void Update(MapPosData mapPosData)
    {
        _mapPosData = mapPosData;
    }
}

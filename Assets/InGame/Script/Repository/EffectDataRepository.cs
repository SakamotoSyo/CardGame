//日本語対応
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDataRepository : IEffectDataRepository
{
    public EffectMasterTable EffectTable => _effectMasterTable;
    private EffectMasterTable _effectMasterTable;

    public async UniTask LoadData(IDataLoader loader)
    {
        var data = await AssetLoader.LoadAssetAsync<EffectMasterTableAsset>(ResourceKey.MasterData.EffectMasterTableAsset);
        _effectMasterTable = data.MasterTable;
        AssetLoader.Release(ResourceKey.MasterData.EffectMasterTableAsset);
        EffectTable.Init();
    }
}

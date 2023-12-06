//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;

public class EnemyDataRepositoryMock : IEnemyDataRepository
{
    [VContainer.Inject]
    public EnemyDataRepositoryMock(IEffectDataRepository effectDataRepository)
    {
        _effectMasterTable = effectDataRepository;
    }

    private IEffectDataRepository _effectMasterTable;
    private Dictionary<int, EnemyData> _loadEnemyData = new Dictionary<int, EnemyData>();

    public async UniTask LoadData(IDataLoader loader) 
    {
       var enemys = await loader.LoadMasterData<EnemyDataTable>("EnemyData");

        foreach (var d in enemys.Data)
        {
            EnemyData enemy = new EnemyData();
            enemy.ReflectsLoadEnemyData(d, _effectMasterTable);
            _loadEnemyData.Add(d.EnemyId, enemy);
        }
    }

    public EnemyData FindById(int id) 
    {
       return !_loadEnemyData.TryGetValue(id, out var enemyData) ? null : enemyData;
    }

    /// <summary>
    /// 階層に出現する敵を返す
    /// </summary>
    /// <param name="currentHierarchy">現在の階層</param>
    /// <returns>出現する敵のList</returns>
    public EnemyData[] FindHierarchyManifestation(int currentHierarchy)
    {
       return _loadEnemyData.Values.Where(x => { return x.HierarchyManifestation.Item1 - 1 < currentHierarchy
                                    && currentHierarchy <  x.HierarchyManifestation.Item2; }).ToArray();
    }
}

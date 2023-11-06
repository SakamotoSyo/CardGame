//日本語対応
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemyData
{
    public int EnemyId;
    public string EnemyName;
    public (int, int) HierarchyManifestation;

    public void ReflectsLoadEnemyData(LoadEnemyData data)
    {
        EnemyId = data.EnemyId;
        EnemyName = data.EnemyName;
       // var hierarchyManifestation = Array.ConvertAll(data.HierarchyManifestation.Split(','), int.Parse);
        //HierarchyManifestation = Tuple.Create(item1: hierarchyManifestation[0], item2: hierarchyManifestation[1]).ToValueTuple();
    }
}

[Serializable]
public class EnemyDataTable : MasterDataBase
{
    public LoadEnemyData[] Data;
}

[Serializable]
public class LoadEnemyData 
{
    public int EnemyId;
    public string EnemyName;
    public string HierarchyManifestation;
}
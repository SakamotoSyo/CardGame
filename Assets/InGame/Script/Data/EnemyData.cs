//日本語対応
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemyData
{
    public int EnemyId;
    public string EnemyName;
    public (int, int) HierarchyManifestation;
    public int EnemyHp;
    public List<EffectMaster> EffectList = new List<EffectMaster>();
    public int ActionsNum;

    public void ReflectsLoadEnemyData(LoadEnemyData data, IEffectDataRepository effectData)
    {
        EnemyId = data.EnemyId;
        EnemyName = data.EnemyName;
        HierarchyManifestation = (data.BeginningHierarchy, data.EndHierarchy);
        EnemyHp = data.EnemyHp;
        var effectNum = data.EffectList.Split(',');
        for (int i = 0; i < effectNum.Length; i++) 
        {
            EffectList.Add(effectData.EffectTable.FindById(effectNum[i]));
        }
        ActionsNum = data.ActionsNum;
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
    public int BeginningHierarchy;
    public int EndHierarchy;
    public int EnemyHp;
    public string EffectList;
    public int ActionsNum;
}
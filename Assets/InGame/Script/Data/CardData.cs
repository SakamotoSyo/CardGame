//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public sealed class LoadCardData
{
    public int Id;
    public string Name;
    public int Rare;
    public string Description;
    public int EffectId;
    public int Cost;
    public string EffectPower;
}

[Serializable]
public sealed class CardDataTable : MasterDataBase
{
    public LoadCardData[] Data;
}

[Serializable]
public class MasterDataBase
{
    public int Version;
}

public sealed class CardData
{
    public int Id;
    public string Name;
    public int Rare;
    public string Description;
    public int Cost;
    public List<IEffect> Effect;

    public void EffectExecute(BattleEnviroment env, TargetType targetType)
    {
        for (int i = 0; i < Effect.Count; i++) 
        {
            Effect[i].Execute(env, targetType);
        }
    }

    public void ReflectsLoadCardData(LoadCardData data)
    {
        Id = data.Id;
        Name = data.Name;
        Rare = data.Rare;
        Description = data.Description;
        Cost = data.Cost;
    }
}

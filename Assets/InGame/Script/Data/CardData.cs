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
    public string Discription;
    public int EffectId;
    public int Cost;
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
    public IEffect Effect;

    public void ReflectsLoadCardData(LoadCardData data)
    {
        Id = data.Id;
        Name = data.Name;
        Rare = data.Rare;
        Description = data.Discription;
        Cost = data.Cost;
    }
}

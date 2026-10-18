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
    /// <summary>効果マスタに設定されたカードの分類(攻撃/防御/特殊)</summary>
    public EffectType EffectType;
    public List<IEffect> Effect;

    public void EffectExecute(BattleEnviroment env, TargetType targetType)
    {
        for (int i = 0; i < Effect.Count; i++) 
        {
            Effect[i].Execute(env, targetType);
        }
    }

    /// <summary>
    /// 山札に入れる1枚分を作る。同じカードを複数枚入れても、手札や捨て札で別の1枚として扱えるようにする
    /// </summary>
    public CardData Clone()
    {
        return (CardData)MemberwiseClone();
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

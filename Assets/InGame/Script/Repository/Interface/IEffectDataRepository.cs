//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectDataRepository : IRepository
{
    public EffectMasterTable EffectTable{ get; }
}

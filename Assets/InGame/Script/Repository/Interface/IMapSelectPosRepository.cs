//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IMapSelectPosRepository
{
    public UniTask LoadData();

    public MapPosData GetValue();

    public void Update(MapPosData mapPosData);
}

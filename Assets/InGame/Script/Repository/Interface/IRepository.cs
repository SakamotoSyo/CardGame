//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IRepository
{
    public UniTask LoadData(IDataLoader loader);
}

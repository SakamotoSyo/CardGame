//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IDataLoader
{
    public UniTask<T> LoadMasterData<T>(string SheetName);
}

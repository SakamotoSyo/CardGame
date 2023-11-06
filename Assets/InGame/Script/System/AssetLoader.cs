//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Assetをロードするためのクラス
/// </summary>
public class AssetLoader : MonoBehaviour
{
    private readonly static Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    /// <summary>
    /// 同期的にAssetをLoadする
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <returns></returns>
    public static T AssetSynchronousLoad<T>(string address)
    {
        AsyncOperationHandle<T> addressableHandle = Addressables.LoadAssetAsync<T>(address);
        _handles.Add(address, addressableHandle);
        var result = addressableHandle.WaitForCompletion();
        return result;
    }

    public static async UniTask<T> LoadAssetAsync<T>(string address) 
    {
        AsyncOperationHandle<T> addressableHandle = Addressables.LoadAssetAsync<T>(address);
        _handles.Add(address, addressableHandle);
        await addressableHandle.Task;
        return addressableHandle.Result;
    }

    public static void BindToLoadAsset<T>() 
    {
            

    }

    /// <summary>
    ///　リソースを開放する
    /// </summary>      
    /// <param name="address"></param>
    public static void Release(string address) 
    {
        Addressables.Release(_handles[address]);
        _handles.Remove(address);
    }


    //public static void AllRe
}

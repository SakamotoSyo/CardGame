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
    //public static T AssetSynchronousLoad<T>(string address)
    //{
    //    AsyncOperationHandle<T> addressableHandle = default;
    //    T result = default;
    //    if (!_handles.ContainsKey(address))
    //    {
    //        addressableHandle = Addressables.LoadAssetAsync<T>(address);
    //        _handles.Add(address, addressableHandle);
    //        result = addressableHandle.WaitForCompletion();
    //    }
    //    else 
    //    {
    //        result = (T)_handles[address].WaitForCompletion();
    //    }

    //    UniTask.Yield(PlayerLoopTiming.Update);
    //    return result;
    //}

    /// <summary>
    /// Domain Reloadを無効にしているとstaticなキャッシュが前回のPlayから残るので、Play開始時に破棄する
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _handles.Clear();
    }

    public static async UniTask<T> LoadAssetAsync<T>(string address)
    {
        if (_handles.TryGetValue(address, out var cachedHandle))
        {
            if (cachedHandle.IsValid())
            {
                //読み込み中に再度呼ばれた場合も完了を待ってから返す
                if (!cachedHandle.IsDone) await cachedHandle.Task;
                if (cachedHandle.IsValid() && cachedHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    return (T)cachedHandle.Result;
                }
                //失敗したハンドルの後始末は読み込みを始めた側が行う。次に呼ばれたときに読み込み直す
                throw new System.InvalidOperationException($"{address}の読み込みに失敗しました");
            }
            //解放済みなどで無効になったハンドルは読み込み直す
            _handles.Remove(address);
        }

        var addressableHandle = Addressables.LoadAssetAsync<T>(address);
        _handles.Add(address, addressableHandle);
        await addressableHandle.Task;
        if (addressableHandle.Status != AsyncOperationStatus.Succeeded)
        {
            //失敗したハンドルをキャッシュに残すと、次に呼ばれても読み込み直せないので取り除く
            var exception = addressableHandle.OperationException;
            AsyncOperationHandle untypedHandle = addressableHandle;
            if (_handles.TryGetValue(address, out var current) && current.Equals(untypedHandle))
            {
                _handles.Remove(address);
            }
            Addressables.Release(addressableHandle);
            throw new System.InvalidOperationException($"{address}の読み込みに失敗しました", exception);
        }
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
        if (!_handles.TryGetValue(address, out var handle)) return;
        _handles.Remove(address);
        if (handle.IsValid()) Addressables.Release(handle);
    }


    //public static void AllRe
}

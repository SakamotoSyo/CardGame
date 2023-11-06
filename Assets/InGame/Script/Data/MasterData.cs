//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.AddressableAssets;

public sealed class MasterData : IDataLoader
{
    public bool IsLoading { get; private set; }

    [SerializeField]
    private string URL = "https://script.google.com/macros/s/AKfycbxYLJKBi81z61xp90iFhhyL9XJeM2sjpX7JYjq5Dg5ssw5p36RvJvCZn-gVgd6rqRAwOQ/exec";
    [SerializeField]
    private const string _sheetMergeSt = "?sheet=";
    private const string DataPrefix = "MasterData";
    private EffectMasterTable _effectMasterTable;


    public async UniTask Setup()
    {
        var data = await Addressables.LoadAssetAsync<EffectMasterTableAsset>(ResourceKey.MasterData.EffectMasterTableAsset);
        _effectMasterTable = data.MasterTable;
        _effectMasterTable.Init();
        //ネットワークに接続できなかったら
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            List<UniTask> masterDataDownloads = new List<UniTask>()
            {
               LoadMasterData<CardDataTable>("Card"),
               LoadMasterData<EnemyDataTable>("EnemyData"),
            };
            await masterDataDownloads;
        }
    }

    string GetFileName(string sheetName)
    {
        return string.Format("{0}/{1}.json", DataPrefix, sheetName);
    }

    /// <summary>
    /// Webから読み込んでくる
    /// </summary>
    /// <returns></returns>
    public async UniTask<T> LoadMasterData<T>(string sheetName)
    {
        var filename = GetFileName(sheetName);
        T data = default;

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            IsLoading = true;
            var url = $"{URL}{_sheetMergeSt}{sheetName}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();
            IsLoading = false;
            var protocol_error = request.result == UnityWebRequest.Result.ProtocolError ? true : false;
            var connection_error = request.result == UnityWebRequest.Result.ConnectionError ? true : false;

            if (protocol_error || connection_error)
            {
                data = await LocalData.LoadAsyncData<T>(GetFileName(sheetName));
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                data = JsonUtility.FromJson<T>(request.downloadHandler.text);
                await LocalData.SaveAsyncData<T>(filename, data);
            }
        }
        else
        {
            data = await LocalData.LoadAsyncData<T>(GetFileName(sheetName));
        }

        return data;
    }
}

//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.AddressableAssets;

public sealed class MasterData
{
    public static MasterData Instance => _instance;
    public bool IsLoading { get; private set; }
    public Dictionary<int, LoadCardData> CardData => _cardData;
    public EffectMasterTable EffectMasterTable => _effectMasterTable;

    private static MasterData _instance = new MasterData();
    [SerializeField]
    private string URL = "https://script.google.com/macros/s/AKfycbxYLJKBi81z61xp90iFhhyL9XJeM2sjpX7JYjq5Dg5ssw5p36RvJvCZn-gVgd6rqRAwOQ/exec";
    [SerializeField]
    private const string _sheetMergeSt = "?sheet=";
    private const string DataPrefix = "MasterData";

    private Dictionary<int, LoadCardData> _cardData = new Dictionary<int, LoadCardData>();
    private EffectMasterTable _effectMasterTable;


    public async UniTask Setup()
    {
        var data = await Addressables.LoadAssetAsync<EffectMasterTableAsset>(ResourceKey.MasterData.EffectMasterTableAsset);
        _effectMasterTable = data.MasterTable;
        _effectMasterTable.Init();

        List<UniTask> masterDataDownloads = new List<UniTask>()
        {
           LoadMasterData<CardDataTable>("Card")
        };
        await masterDataDownloads;
        await LoadLocalData();
    }

    string GetFileName(string sheetName)
    {
        return string.Format("{0}/{1}.json", DataPrefix, sheetName);
    }

    /// <summary>
    /// Webから読み込んでくる
    /// </summary>
    /// <returns></returns>
    private async UniTask LoadMasterData<T>(string sheetName)
    {
        var filename = GetFileName(sheetName);
        IsLoading = true;
        var url = $"{URL}{_sheetMergeSt}{sheetName}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        await request.SendWebRequest();
        IsLoading = false;
        var protocol_error = request.result == UnityWebRequest.Result.ProtocolError ? true : false;
        var connection_error = request.result == UnityWebRequest.Result.ConnectionError ? true : false;

        if (protocol_error || connection_error)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            T data = JsonUtility.FromJson<T>(request.downloadHandler.text);
            await LocalData.SaveAsyncData<T>(filename, data);
        }
    }

    /// <summary>
    /// ローカルのデータをロードする
    /// </summary>
    private async UniTask LoadLocalData() 
    {
        var cards = await LocalData.LoadAsyncData<CardDataTable>(GetFileName("Card"));

        foreach (var d in cards.Data) 
        {
            LoadCardData card = new LoadCardData();
            card.Id = d.Id;
            card.Name = d.Name;
            card.Rare = d.Rare;
            card.Discription = d.Discription;
            card.EffectId = d.EffectId;
            _cardData.Add(d.Id, card);
        }
    }
}

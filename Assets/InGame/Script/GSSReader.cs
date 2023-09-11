//日本語対応
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

/// <summary>
/// スプレットシートをWebから読み込んでくるScript
/// </summary>
public class GSSReader : MonoBehaviour
{
    [SerializeField]
    private string URL = "読み込むシートのURL";
    [SerializeField]
    private string SheetName = "読み込むシート";
    private const string _sheetMergeSt = "?sheet=";
    public UnityEvent OnLoadEnd;
    public bool IsLoading { get; private set; }
    public string[][] Datas { get; private set; }

    /// <summary>
    /// Webから読み込んでくる
    /// </summary>
    /// <returns></returns>
    IEnumerator GetFromWeb()
    {
        IsLoading = true;
        //https://script.google.com/macros/s/AKfycbxYLJKBi81z61xp90iFhhyL9XJeM2sjpX7JYjq5Dg5ssw5p36RvJvCZn-gVgd6rqRAwOQ/exec
        var url = $"{URL}{_sheetMergeSt}{SheetName}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        IsLoading = false;
        var protocol_error = request.result == UnityWebRequest.Result.ProtocolError ? true : false;
        var connection_error = request.result == UnityWebRequest.Result.ConnectionError ? true : false;
        if (protocol_error || connection_error)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Datas = ConvertCSVtoJaggedArray(request.downloadHandler.text);
            Debug.Log(request.downloadHandler.text);
            OnLoadEnd.Invoke();
        }
    }
    public void Reload() => StartCoroutine(GetFromWeb());

    /// <summary>
    /// 読み込んできたTextを2次元配列にする
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    static string[][] ConvertCSVtoJaggedArray(string t)
    {
        var reader = new StringReader(t);
        reader.ReadLine();  //ヘッダ読み飛ばし
        var rows = new List<string[]>();
        while (reader.Peek() >= 0)
        {
            var line = reader.ReadLine();        // 一行ずつ読み込み
            var elements = line.Split(',');    // 行のセルは,で区切られる
            for (var i = 0; i < elements.Length; i++)
            {
                elements[i] = elements[i].TrimStart('"').TrimEnd('"');
            }
            rows.Add(elements);
        }
        return rows.ToArray();
    }
}
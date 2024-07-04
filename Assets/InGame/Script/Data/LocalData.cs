//日本語対応
using System;
using UnityEngine;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Text;
using System.Linq;

public class LocalData
{

    public static bool GetFileExists(string file, string path = null) 
    {
        return File.Exists(path + "/" + file);
    }

    /// <summary>
    /// ファイルをロードする
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="file"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async UniTask<T> LoadAsyncData<T>(string file, string path = null) 
    {
        try
        {
            if (path == null)
            {
                path = Application.persistentDataPath;
            }

            if (!File.Exists(path + "/" + file))
            {
                return default(T);
            }

            string json = "";

            using (FileStream fs = new FileStream(path + "/" + file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var bytes = new byte[fs.Length];
                await fs.ReadAsync(bytes, 0, bytes.Length);

                json = Encoding.UTF8.GetString(bytes);
            }

            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"LoadAsync{file}");
            Debug.LogError(ex.Message);
        }
        return default(T);
    }

    /// <summary>
    /// ローカルにデータをセーブする
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static async UniTask SaveAsyncData<T>(string file, T data, string path = null)
    {
        try
        {
            if (path == null)
            {
                path = Application.persistentDataPath;
                Debug.Log(Application.persistentDataPath);
            }

            var json = JsonUtility.ToJson(data);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            var pathes = (path + "/" + file).Split('/').ToList();
            pathes.RemoveAt(pathes.Count - 1);
            var dir = string.Join("/", pathes);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (FileStream fs = new FileStream(path + "/" + file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Save{file}");
            Debug.LogError(ex.Message);
        }
    }

}

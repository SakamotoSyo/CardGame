// 日本語対応
using UnityEngine;

public class TestReader : MonoBehaviour
{
    [SerializeField] private GSSReader _reader;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnGSSLoad() 
    {
        //読み込んだデータ
        var data = _reader.Datas;

        for(int x = 0; x < data.Length; x++) 
        {
            for (int y = 0; y < data[x].Length; y++) 
            {
                Debug.Log(data[x][y]);
            }
        }
    }
}

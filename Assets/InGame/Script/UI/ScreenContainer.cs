//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScreenContainer : MonoBehaviour
{
    private Stack<GameObject> _objStack;
    
    public async void Push(string resouseKey) 
    {
        var obj = await AssetLoader.LoadAssetAsync<GameObject>(resouseKey);
        //ここに画面遷移のAnimationの処理を入れたい
        _objStack.Push(obj);
    }

    public void Pop(int popCount) 
    {
        //画面遷移
        if (popCount > _objStack.Count) 
        {
            throw new InvalidOperationException("指定の数以上のPopするUIがありません");
        }
        Destroy(_objStack.Pop());
    }
}

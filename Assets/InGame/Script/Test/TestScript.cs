//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using VContainer;

public class TestScript: IInitializable
{
    private IPlayerStatus _status;

    [Inject]
    public TestScript(IPlayerStatus status)
    {
        _status = status;
    }

    public void Initialize()
    {

    }

    public async UniTask SetUp() 
    {
        Debug.Log("実行");
        for (int i = 0; i < 3; i++) 
        {
            await UniTask.DelayFrame(20);
            _status.DrawCard(1);
        }
    }
}

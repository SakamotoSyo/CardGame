//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using VContainer;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EntryPoint : IAsyncStartable
{
    MainUi _mainUi;
    private ActorGenerator _actorGenerator;
    private TestScript _testScript;

    public EntryPoint(MainUi mainUi, ActorGenerator actorGenerator, TestScript testScript) 
    {
        _mainUi = mainUi;
        _actorGenerator = actorGenerator;
        _testScript = testScript;
    }
    public async UniTask StartAsync(CancellationToken cancellation)
    {
        Application.targetFrameRate = 60;
        await MasterData.Instance.Setup();
        _mainUi.SetUp();
        await _actorGenerator.Setup();
        _testScript.SetUp(_actorGenerator.PlayerStatus).Forget();

    }
}

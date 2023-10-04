//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using VContainer;

public sealed class TransitionService : IInitializable
{
    private ActorGenerator _actorGenerator;
    private TestScript _script;

    [Inject]
    public TransitionService(ActorGenerator actorGenerator, TestScript testScript)
    {
        _actorGenerator = actorGenerator;
        _script = testScript;
    }

    public void Initialize()
    {
        ApplicationStart().Forget();
    }

    private async UniTask ApplicationStart()
    {
        await MasterData.Instance.Setup();
        Debug.Log("来ました");
        await _actorGenerator.Setup();
        _script.SetUp(_actorGenerator.PlayerStatus).Forget();
    }
}

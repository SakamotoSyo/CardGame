//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;

public class MainBattleScreen : MonoBehaviour
{
    [SerializeField] private HandleCardEffectField _handleCardEffectField;
    [SerializeField] private ActorGenerator _actorGenerator;

    private TestScript _testScript = default;
    private CardEnviroment _cardEnv = default;

    [Inject]
    public void Construct(CardEnviroment cardEnviroment, TestScript testScript) 
    {
        Debug.Log(cardEnviroment);
        _cardEnv = cardEnviroment;
        _testScript = testScript;
    }

    private void Start() 
    {
        SetUp().Forget();
    }

    public async UniTask SetUp() 
    {
        await _actorGenerator.Setup();
        await _testScript.SetUp();
        _handleCardEffectField.SetUp(_cardEnv);
    }

    void Update()
    {
        
    }
}

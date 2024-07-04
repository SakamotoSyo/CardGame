//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using UnityEngine.UI;

public class MainBattleScreen : ScreenBase
{
    [SerializeField] private HandleCardEffectField _handleCardEffectField;
    [SerializeField] private ActorGenerator _actorGenerator;
    [SerializeField] private Text _currentTurnText;

    private BattleEnviroment _cardEnv = default;
    private IBattleTurnController _battleTurnController;

    [Inject]
    public void Construct(BattleEnviroment cardEnviroment, IBattleTurnController battleController) 
    {
        _cardEnv = cardEnviroment;
        _battleTurnController = battleController;
    }

    private void Start() 
    {
        SetUp().Forget();
    }

    public async UniTask SetUp() 
    {
        await _actorGenerator.Setup();
        await _battleTurnController.Setup();
        _handleCardEffectField.SetUp(_cardEnv);
    }

    public void BindView() 
    {
        
    }

    void Update()
    {
        //if (_battleTurnController == null && _battleTurnController.CurrentState == null) return;
        //_currentTurnText.text = _battleTurnController.CurrentState.ToString();
    }
}

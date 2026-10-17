//日本語対応
using Cysharp.Threading.Tasks;
using VContainer;

/// <summary>
/// 戦闘の決着をランに反映し、次の画面へ遷移する
/// </summary>
public sealed class BattleResultService
{
    private readonly IDungeonRunRepository _runRepository;
    private readonly TransitionService _transitionService;

    [Inject]
    public BattleResultService(IDungeonRunRepository runRepository, TransitionService transitionService)
    {
        _runRepository = runRepository;
        _transitionService = transitionService;
    }

    /// <summary>
    /// 勝利したら戦闘をクリア済みにしてステージ選択画面へ戻る(翌日へはステージ選択画面側で進める)
    /// </summary>
    public async UniTask Win()
    {
        await _runRepository.LoadData();
        _runRepository.Current?.ClearPendingStage();
        await _runRepository.Save();
        await _transitionService.StageSelectScreen();
    }

    /// <summary>
    /// 敗北したらランを破棄してタイトルへ戻る
    /// </summary>
    public async UniTask Lose()
    {
        await _runRepository.DeleteRun();
        await _transitionService.BackToTitleScreen();
    }
}

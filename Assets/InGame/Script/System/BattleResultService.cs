//日本語対応
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

/// <summary>
/// 戦闘の決着をランに反映し、次の画面へ遷移する
/// </summary>
public sealed class BattleResultService
{
    private readonly IDungeonRunRepository _runRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly GachaSettings _gachaSettings;
    private readonly TransitionService _transitionService;

    [Inject]
    public BattleResultService(IDungeonRunRepository runRepository,
        IWalletRepository walletRepository,
        GachaSettings gachaSettings,
        TransitionService transitionService)
    {
        _runRepository = runRepository;
        _walletRepository = walletRepository;
        _gachaSettings = gachaSettings;
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
    /// 敗北したらランを終え、進んだ日数分のガチャ石を渡してからランを破棄し、タイトルへ戻る
    /// </summary>
    public async UniTask Lose()
    {
        await _runRepository.LoadData();
        var run = _runRepository.Current;
        if (run != null)
        {
            if (string.IsNullOrEmpty(run.RunId))
            {
                //Idが付く前のランは、破棄に失敗して次に起動したときも同じIdになるよう、先に付けて保存しておく
                run.RunId = System.Guid.NewGuid().ToString("N");
                await _runRepository.Save();
            }
            //1日目で負けたら0日、DAY 8で負けたら7日進んだことになる
            var advancedDays = Mathf.Max(0, run.Day - 1);
            var reward = new RunReward
            {
                RunId = run.RunId,
                Day = run.Day,
                AdvancedDays = advancedDays,
                GachaStone = advancedDays * _gachaSettings.StonePerDay,
            };
            //保存できなくてもランは破棄する(残すと負けた戦闘をやり直せてしまう)。受け取れなかったことはタイトルで伝える
            if (!await _walletRepository.ReceiveRunReward(reward))
            {
                Debug.LogError("ガチャ石を保存できませんでした");
            }
        }
        //破棄に失敗してランが残っても、報酬はRunIdで重ねて渡さないのでタイトルへは戻る
        try
        {
            await _runRepository.DeleteRun();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        await _transitionService.BackToTitleScreen();
    }
}

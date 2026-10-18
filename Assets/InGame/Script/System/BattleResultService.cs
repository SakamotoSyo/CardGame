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
        var isDeleted = false;
        if (run != null)
        {
            var isRunIdSaved = true;
            if (string.IsNullOrEmpty(run.RunId))
            {
                //Idが付く前のランは、破棄に失敗して次に起動したときも同じIdになるよう、先に付けて保存しておく
                run.RunId = System.Guid.NewGuid().ToString("N");
                isRunIdSaved = await _runRepository.Save();
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
            if (!isRunIdSaved)
            {
                //Idを保存できず、ランも残ると次に起動したとき別のIdになって報酬を重ねて渡してしまうので、
                //先に破棄して、破棄できたときだけ渡す
                isDeleted = await TryDeleteRun();
            }
            if (isRunIdSaved || isDeleted)
            {
                //保存できなくてもランは破棄する(残すと負けた戦闘をやり直せてしまう)。受け取れなかったことはタイトルで伝える
                if (!await _walletRepository.ReceiveRunReward(reward))
                {
                    Debug.LogError("ガチャ石を保存できませんでした");
                }
            }
            else
            {
                Debug.LogError("ランのIdを保存できず破棄もできなかったため、ガチャ石を渡しません");
                _walletRepository.SetUnreceivedRunReward(reward);
            }
        }
        //破棄に失敗してランが残っても、報酬はRunIdで重ねて渡さないのでタイトルへは戻る
        if (!isDeleted) await TryDeleteRun();
        await _transitionService.BackToTitleScreen();
    }

    /// <returns>破棄できたらtrue</returns>
    private async UniTask<bool> TryDeleteRun()
    {
        try
        {
            await _runRepository.DeleteRun();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }
}

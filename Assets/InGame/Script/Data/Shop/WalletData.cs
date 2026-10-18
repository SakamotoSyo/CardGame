//日本語対応
using System;

/// <summary>
/// ランをまたいで持ち越す通貨(ガチャ石)
/// </summary>
[Serializable]
public sealed class WalletData
{
    public int GachaStone;
    /// <summary>最後に報酬を受け取ったランのId。同じランの報酬を2回受け取らないために使う</summary>
    public string LastRewardedRunId;
}

/// <summary>
/// 終わったランで手に入れたガチャ石。タイトル画面で結果を表示するのに使う
/// </summary>
public sealed class RunReward
{
    /// <summary>報酬を渡したランのId</summary>
    public string RunId;
    /// <summary>ランが終わった日(負けた日)</summary>
    public int Day;
    /// <summary>進んだ日数</summary>
    public int AdvancedDays;
    public int GachaStone;
    /// <summary>ガチャ石を保存できたらtrue。falseならタイトルで受け取れなかったことを伝える</summary>
    public bool IsReceived;
}

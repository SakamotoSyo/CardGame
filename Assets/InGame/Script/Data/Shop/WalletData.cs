//日本語対応
using System;

/// <summary>
/// ランをまたいで持ち越す通貨(ガチャ石)
/// </summary>
[Serializable]
public sealed class WalletData
{
    public int GachaStone;
}

/// <summary>
/// 終わったランで手に入れたガチャ石。タイトル画面で結果を表示するのに使う
/// </summary>
public sealed class RunReward
{
    /// <summary>ランが終わった日(負けた日)</summary>
    public int Day;
    /// <summary>進んだ日数</summary>
    public int AdvancedDays;
    public int GachaStone;
}

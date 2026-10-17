//日本語対応
using System;
using System.Collections.Generic;

/// <summary>
/// デッキ編集画面で保存したデッキ。戦闘開始時の山札になる
/// </summary>
[Serializable]
public sealed class DeckData
{
    /// <summary>デッキに入っているカードのId。同じカードは枚数分並ぶ</summary>
    public List<int> CardIds = new List<int>();
}

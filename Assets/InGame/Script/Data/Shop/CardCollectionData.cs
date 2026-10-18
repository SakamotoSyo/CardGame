//日本語対応
using System;
using System.Collections.Generic;

/// <summary>
/// 所持しているカードと枚数
/// </summary>
[Serializable]
public sealed class CardCollectionData
{
    //JsonUtilityはDictionaryを保存できないのでリストで持つ
    public List<OwnedCard> Cards = new List<OwnedCard>();
}

[Serializable]
public sealed class OwnedCard
{
    public int Id;
    public int Count;
}

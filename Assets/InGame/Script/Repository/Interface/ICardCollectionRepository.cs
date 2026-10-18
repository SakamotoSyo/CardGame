//日本語対応
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
/// 所持カードを保存・読み込みする
/// </summary>
public interface ICardCollectionRepository
{
    /// <summary>
    /// 読み込む。まだ保存されていなければ、初期デッキと保存済みのデッキに入っているカードを所持済みにして作る
    /// </summary>
    public UniTask LoadData();
    public int GetCount(int cardId);
    /// <returns>保存に成功したらtrue。失敗したときは所持数も変わらない</returns>
    public UniTask<bool> AddCards(IReadOnlyList<int> cardIds);
}

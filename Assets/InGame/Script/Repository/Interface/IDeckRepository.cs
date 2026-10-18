//日本語対応
using Cysharp.Threading.Tasks;

/// <summary>
/// デッキ編集画面で組んだデッキを保存・読み込みする
/// </summary>
public interface IDeckRepository
{
    /// <summary>保存されたデッキ。まだ一度も保存していなければnull</summary>
    public DeckData Current { get; }

    public UniTask LoadData();
    /// <returns>保存に成功したらtrue。失敗したときはCurrentも変わらない</returns>
    public UniTask<bool> Save(DeckData deckData);
}

//日本語対応
using System.Collections.Generic;
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

    /// <summary>保存したデッキから、マスタにないカードを除いたもの。まだ保存していなければnull</summary>
    public List<int> GetSavedCardIds();
    /// <summary>初期デッキ(InitialPlayerStatusの札)から、マスタにないカードを除いたもの</summary>
    public UniTask<List<int>> LoadInitialCardIds();
    /// <summary>枚数と同じカードの枚数がルールを満たしていればtrue</summary>
    public bool IsPlayable(IReadOnlyList<int> cardIds);
    /// <summary>
    /// 戦闘で使う山札のカードId。保存したデッキがルールを満たしていればそれを、満たしていなければ初期デッキを使う
    /// </summary>
    public List<int> ResolveBattleDeck(IReadOnlyList<int> initialCardIds);
}

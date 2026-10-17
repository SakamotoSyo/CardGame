//日本語対応
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デッキ編集画面で使うカードの表面(戦闘のカードと同じ枠にコスト・名前・説明・アイコンを載せる)
/// </summary>
public class DeckCardFaceView : MonoBehaviour
{
    [SerializeField] private Text _costText;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _descriptionText;
    [Tooltip("カード中央の絵の代わりに出す、分類のアイコン")]
    [SerializeField] private Image _iconImage;
    [Tooltip("レアリティの色の帯")]
    [SerializeField] private Image _rarityImage;

    public void SetCard(CardData card, DeckBuildSettings settings)
    {
        _costText.text = card.Cost.ToString();
        _nameText.text = card.Name;
        _descriptionText.text = card.Description;
        var icon = settings.GetIcon(DeckBuildSettings.GetEffectType(card));
        _iconImage.sprite = icon;
        _iconImage.enabled = icon != null;
        _rarityImage.color = settings.GetRarityColor(card.Rare);
    }
}

//日本語対応
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面のPrefabを自動生成するエディタ拡張で共通して使う、見た目の色・素材とUIの部品の作り方
/// 画面ごとに見た目がずれないよう、ここを直せばどの画面の生成にも反映される
/// </summary>
public static class UiPrefabBuilder
{
    public const string TitlePrefabPath = "Assets/InGame/Prefab/ScreenPrefab/PrefabTitleScreen.prefab";
    private const string FontPath = "Assets/InGame/Font/ReggaeOne-Regular.ttf";
    private const string CardFramePath = "Assets/InGame/Image/Card/カードフレーム2黄色M無し-removebg-preview.png";
    private const string CardBackgroundPath = "Assets/InGame/Image/Card/カードフレーム背景.png";

    public static readonly Color Gold = new Color(1f, 0.8f, 0.35f);
    public static readonly Color Ink = new Color(0.23f, 0.16f, 0.1f);
    public static readonly Color Brown = new Color(0.42f, 0.32f, 0.18f);
    public static readonly Color ButtonGray = new Color(0.25f, 0.2f, 0.25f);
    public static readonly Color ButtonGreen = new Color(0.24f, 0.48f, 0.27f);
    public static readonly Color ButtonRed = new Color(0.7f, 0.12f, 0.12f);
    public static readonly Color SubText = new Color(1f, 1f, 1f, 0.6f);

    //カード1枚の大きさ(戦闘のカード枠と同じ縦横比)
    public static readonly Vector2 CardSize = new Vector2(210, 280);

    public static Font TextFont { get; private set; }
    public static Sprite RoundSprite { get; private set; }
    public static Sprite KnobSprite { get; private set; }
    public static Sprite FrameSprite { get; private set; }
    public static Sprite CardBackgroundSprite { get; private set; }

    /// <summary>
    /// 部品を作る前に呼ぶ。フォントと画像を読み込む
    /// </summary>
    public static void LoadAssets()
    {
        TextFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        RoundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        KnobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        FrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CardFramePath);
        CardBackgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CardBackgroundPath);
    }

    /// <summary>
    /// 金色の縁取りをした暗いパネル
    /// </summary>
    public static Transform CreatePanel(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var border = CreateImage(name, parent, Brown, RoundSprite);
        SetRect(border.rectTransform, new Vector2(0.5f, 0.5f), pos, size);
        var inner = CreateImage("Inner", border.transform, new Color(0.1f, 0.075f, 0.086f, 0.95f), RoundSprite);
        Stretch(inner.rectTransform, 3);
        return border.transform;
    }

    /// <summary>
    /// タイトル画面と同じ設定のCanvasを作る
    /// </summary>
    public static GameObject CreateCanvas(Transform parent)
    {
        var obj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        obj.layer = 5;
        obj.transform.SetParent(parent, false);

        var title = AssetDatabase.LoadAssetAtPath<GameObject>(TitlePrefabPath);
        var srcCanvas = title != null ? title.GetComponentInChildren<Canvas>(true) : null;
        if (srcCanvas != null)
        {
            EditorUtility.CopySerialized(srcCanvas, obj.GetComponent<Canvas>());
            EditorUtility.CopySerialized(srcCanvas.GetComponent<CanvasScaler>(), obj.GetComponent<CanvasScaler>());
            var src = (RectTransform)srcCanvas.transform;
            var dst = (RectTransform)obj.transform;
            dst.anchorMin = src.anchorMin;
            dst.anchorMax = src.anchorMax;
            dst.pivot = src.pivot;
            dst.sizeDelta = src.sizeDelta;
            dst.anchoredPosition3D = src.anchoredPosition3D;
            dst.localRotation = src.localRotation;
            dst.localScale = src.localScale;
        }
        else
        {
            Debug.LogWarning("タイトル画面のCanvasが見つからないためデフォルト設定で生成します");
            var scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
        return obj;
    }

    /// <summary>
    /// 押せないときに灰色に見えるボタンの色
    /// </summary>
    public static ColorBlock DisabledGrayColors()
    {
        var colors = ColorBlock.defaultColorBlock;
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        return colors;
    }

    public static RectTransform CreateRect(string name, Transform parent)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.layer = 5;
        obj.transform.SetParent(parent, false);
        return (RectTransform)obj.transform;
    }

    public static Image CreateImage(string name, Transform parent, Color color, Sprite sprite = null)
    {
        var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
        image.color = color;
        image.sprite = sprite;
        if (sprite != null && sprite == RoundSprite) image.type = Image.Type.Sliced;
        return image;
    }

    public static Text CreateText(string name, Transform parent, string text, int size, Color color, TextAnchor anchor)
    {
        var t = CreateRect(name, parent).gameObject.AddComponent<Text>();
        t.font = TextFont;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    public static Button CreateButton(string name, Transform parent, string label, int size, Color color, out Text labelText)
    {
        var image = CreateImage(name, parent, color, RoundSprite);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.colors = DisabledGrayColors();
        labelText = CreateText("Text", image.transform, label, size, Color.white, TextAnchor.MiddleCenter);
        Stretch(labelText.rectTransform);
        return button;
    }

    public static void AddShadow(GameObject obj)
    {
        var shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(3, -3);
    }

    public static void SetLayer(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t) SetLayer(child, layer);
    }

    public static void Stretch(RectTransform rect, float inset = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    public static void SetRect(RectTransform rect, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
    }
}

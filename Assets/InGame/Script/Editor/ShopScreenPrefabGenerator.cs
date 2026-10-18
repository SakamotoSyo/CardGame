//日本語対応
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UiPrefabBuilder;

/// <summary>
/// ショップ画面のPrefabとガチャの設定アセットを自動生成し、タイトル画面にショップへのボタンを追加するエディタ拡張
/// </summary>
public static class ShopScreenPrefabGenerator
{
    private const string PrefabPath = "Assets/InGame/Prefab/ScreenPrefab/PrefabShopScreen.prefab";
    private const string SettingsPath = "Assets/InGame/Data/Shop/GachaSettings.asset";

    [MenuItem("Tools/CardGame/Generate Shop Screen Prefab")]
    public static void Generate()
    {
        LoadAssets();
        LoadOrCreateSettings();

        var root = new GameObject("PrefabShopScreen");
        root.layer = 5;
        var screen = root.AddComponent<ShopScreen>();
        var canvas = CreateCanvas(root.transform).transform;

        var bg = CreateImage("Background", canvas, new Color(0.05f, 0.035f, 0.045f));
        Stretch(bg.rectTransform);
        var bgMap = CreateImage("BackgroundMap", canvas, new Color(1f, 1f, 1f, 0.08f), CardBackgroundSprite);
        Stretch(bgMap.rectTransform);
        bgMap.raycastTarget = false;

        //ヘッダー
        var header = CreateImage("Header", canvas, new Color(0.03f, 0.02f, 0.03f, 0.92f));
        header.rectTransform.anchorMin = header.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        header.rectTransform.anchoredPosition = new Vector2(0, -50);
        header.rectTransform.sizeDelta = new Vector2(1920, 100);
        var headerLine = CreateImage("Line", header.transform, new Color(0.6f, 0.45f, 0.2f));
        SetRect(headerLine.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 1), new Vector2(1920, 3));
        var backButton = CreateButton("BackButton", header.transform, "＜ タイトル", 28, ButtonGray, out _);
        SetRect(backButton.image.rectTransform, new Vector2(0f, 0.5f), new Vector2(140, 0), new Vector2(230, 66));
        var title = CreateText("TitleText", header.transform, "ショップ", 50, Gold, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 64));
        AddShadow(title.gameObject);
        var stoneText = CreateStoneLabel(header.transform, new Vector2(1f, 0.5f), new Vector2(-200, 0));

        //ガチャの看板
        var panel = CreatePanel("GachaPanel", canvas, new Vector2(0, 60), new Vector2(1100, 640));
        var gachaTitle = CreateText("GachaTitle", panel, "カードガチャ", 64, Gold, TextAnchor.MiddleCenter);
        SetRect(gachaTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -75), new Vector2(900, 80));
        AddShadow(gachaTitle.gameObject);
        var gachaSub = CreateText("GachaSubText", panel, "手に入れたカードはデッキ編集で使えます", 26, SubText, TextAnchor.MiddleCenter);
        SetRect(gachaSub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -135), new Vector2(900, 36));
        //裏向きのカードを扇状に並べて飾る
        var showcase = CreateRect("Showcase", panel);
        SetRect(showcase, new Vector2(0.5f, 0.5f), new Vector2(0, -20), CardSize);
        //中央のカードが一番手前に来るよう、左右を先に作る
        foreach (var i in new[] { -1, 1, 0 })
        {
            var back = CreateCardBack(showcase);
            back.anchoredPosition = new Vector2(i * 150, i == 0 ? 10 : -10);
            back.localEulerAngles = new Vector3(0, 0, -i * 10);
        }
        var rateText = CreateText("RateText", panel, "排出率", 22, SubText, TextAnchor.MiddleCenter);
        SetRect(rateText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(1000, 30));

        var singleButton = CreateButton("SingleButton", canvas, "1回引く", 36, ButtonGreen, out var singleText);
        SetRect(singleButton.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-250, -380), new Vector2(420, 120));
        singleText.lineSpacing = 0.9f;
        AddShadow(singleText.gameObject);
        var multiButton = CreateButton("MultiButton", canvas, "10連", 36, ButtonRed, out var multiText);
        SetRect(multiButton.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(250, -380), new Vector2(420, 120));
        multiText.lineSpacing = 0.9f;
        AddShadow(multiText.gameObject);

        var hint = CreateText("HintText", canvas, "", 24, new Color(1f, 1f, 1f, 0.75f), TextAnchor.MiddleCenter);
        SetRect(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 25), new Vector2(1600, 34));
        AddShadow(hint.gameObject);

        //ガチャ結果
        var result = CreateImage("Result", canvas, new Color(0f, 0f, 0f, 0.85f));
        Stretch(result.rectTransform);
        var resultGroup = result.gameObject.AddComponent<CanvasGroup>();
        var skip = CreateImage("SkipButton", result.transform, new Color(0f, 0f, 0f, 0f));
        Stretch(skip.rectTransform);
        var skipButton = skip.gameObject.AddComponent<Button>();
        skipButton.transition = Selectable.Transition.None;
        var resultTitle = CreateText("ResultTitle", result.transform, "ガチャ結果", 48, Color.white, TextAnchor.MiddleCenter);
        SetRect(resultTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 450), new Vector2(1200, 64));
        AddShadow(resultTitle.gameObject);
        var content = CreateRect("Content", result.transform);
        SetRect(content, new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(1200, 700));
        var template = CreateResultCard(content);
        var okButton = CreateButton("OkButton", result.transform, "OK", 32, ButtonRed, out _);
        SetRect(okButton.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -455), new Vector2(280, 76));
        result.gameObject.SetActive(false);

        var fade = CreateImage("Fade", canvas, Color.black);
        Stretch(fade.rectTransform);

        //LifetimeScope
        var scopeObj = new GameObject("LifeTimeScope");
        scopeObj.transform.SetParent(root.transform, false);
        var scope = scopeObj.AddComponent<ShopLifeTimeScope>();
        scope.parentReference = VContainer.Unity.ParentReference.Create<ApplicationCompositionRoot>();
        var scopeSo = new SerializedObject(scope);
        var inject = scopeSo.FindProperty("autoInjectGameObjects");
        inject.arraySize = 1;
        inject.GetArrayElementAtIndex(0).objectReferenceValue = root;
        scopeSo.ApplyModifiedPropertiesWithoutUndo();

        var so = new SerializedObject(screen);
        so.FindProperty("_backButton").objectReferenceValue = backButton;
        so.FindProperty("_stoneText").objectReferenceValue = stoneText;
        so.FindProperty("_singleButton").objectReferenceValue = singleButton;
        so.FindProperty("_singleButtonText").objectReferenceValue = singleText;
        so.FindProperty("_multiButton").objectReferenceValue = multiButton;
        so.FindProperty("_multiButtonText").objectReferenceValue = multiText;
        so.FindProperty("_rateText").objectReferenceValue = rateText;
        so.FindProperty("_hintText").objectReferenceValue = hint;
        so.FindProperty("_resultGroup").objectReferenceValue = resultGroup;
        so.FindProperty("_resultContent").objectReferenceValue = content;
        so.FindProperty("_resultTemplate").objectReferenceValue = template;
        so.FindProperty("_resultTitleText").objectReferenceValue = resultTitle;
        so.FindProperty("_resultOkButton").objectReferenceValue = okButton;
        so.FindProperty("_resultSkipButton").objectReferenceValue = skipButton;
        so.FindProperty("_fadeImage").objectReferenceValue = fade;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AddTitleParts();
        AssetDatabase.SaveAssets();
        Debug.Log($"ショップ画面のPrefabを生成し、タイトル画面にショップのボタンを追加しました: {PrefabPath}");
    }

    /// <summary>
    /// タイトル画面の右上にガチャ石の表示とショップのボタン、中央にランの結果のポップアップを置く。何度実行しても作り直すだけ
    /// </summary>
    private static void AddTitleParts()
    {
        var title = PrefabUtility.LoadPrefabContents(TitlePrefabPath);
        try
        {
            var canvas = title.GetComponentInChildren<Canvas>(true).transform;
            foreach (var name in new[] { "ShopButton", "StoneLabel", "RewardPopup" })
            {
                var old = canvas.Find(name);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            //タイトルのCanvasはカメラより少し上にずらして置かれているので、上端から余裕を持たせて画面内に収める
            var shopButton = CreateButton("ShopButton", canvas, "ショップ", 34, ButtonRed, out _);
            SetRect(shopButton.image.rectTransform, new Vector2(1f, 1f), new Vector2(-170, -150), new Vector2(260, 90));
            AddShadow(shopButton.GetComponentInChildren<Text>().gameObject);
            var stoneText = CreateStoneLabel(canvas, new Vector2(1f, 1f), new Vector2(-490, -150));

            var popup = CreateImage("RewardPopup", canvas, new Color(0f, 0f, 0f, 0.65f));
            Stretch(popup.rectTransform);
            var popupGroup = popup.gameObject.AddComponent<CanvasGroup>();
            var frame = CreateImage("Frame", popup.transform, Gold, RoundSprite);
            SetRect(frame.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 420));
            var inner = CreateImage("Inner", frame.transform, new Color(0.07f, 0.05f, 0.1f), RoundSprite);
            Stretch(inner.rectTransform, 6);
            var header = CreateText("TitleText", frame.transform, "ランの結果", 44, Gold, TextAnchor.MiddleCenter);
            SetRect(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -55), new Vector2(700, 60));
            var body = CreateText("BodyText", frame.transform, "", 32, Color.white, TextAnchor.MiddleCenter);
            SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(700, 180));
            var ok = CreateButton("OkButton", frame.transform, "OK", 30, ButtonRed, out _);
            SetRect(ok.image.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 65), new Vector2(240, 70));
            popup.gameObject.SetActive(false);

            var so = new SerializedObject(title.GetComponent<TitleScreen>());
            so.FindProperty("_shopButton").objectReferenceValue = shopButton;
            so.FindProperty("_stoneText").objectReferenceValue = stoneText;
            so.FindProperty("_rewardPopup").objectReferenceValue = popupGroup;
            so.FindProperty("_rewardBodyText").objectReferenceValue = body;
            so.FindProperty("_rewardOkButton").objectReferenceValue = ok;
            so.ApplyModifiedPropertiesWithoutUndo();
            SetLayer(canvas, 5);
            PrefabUtility.SaveAsPrefabAsset(title, TitlePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(title);
        }
    }

    private static GachaSettings LoadOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<GachaSettings>(SettingsPath);
        if (settings != null) return settings;
        var folder = System.IO.Path.GetDirectoryName(SettingsPath).Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/InGame/Data", "Shop");
        settings = ScriptableObject.CreateInstance<GachaSettings>();
        AssetDatabase.CreateAsset(settings, SettingsPath);
        return settings;
    }

    /// <summary>
    /// 丸い石のアイコンを付けたガチャ石の所持数の表示
    /// </summary>
    private static Text CreateStoneLabel(Transform parent, Vector2 anchor, Vector2 pos)
    {
        var bg = CreateImage("StoneLabel", parent, new Color(0f, 0f, 0f, 0.55f), RoundSprite);
        SetRect(bg.rectTransform, anchor, pos, new Vector2(320, 66));
        bg.raycastTarget = false;
        var gem = CreateImage("Gem", bg.transform, new Color(0.45f, 0.85f, 1f), KnobSprite);
        SetRect(gem.rectTransform, new Vector2(0f, 0.5f), new Vector2(36, 0), new Vector2(40, 40));
        gem.raycastTarget = false;
        var shine = CreateImage("Shine", gem.transform, new Color(1f, 1f, 1f, 0.8f), KnobSprite);
        SetRect(shine.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-6, 6), new Vector2(12, 12));
        shine.raycastTarget = false;
        var text = CreateText("StoneText", bg.transform, "ガチャ石 0", 30, Gold, TextAnchor.MiddleLeft);
        SetRect(text.rectTransform, new Vector2(0f, 0.5f), new Vector2(180, 0), new Vector2(250, 50));
        AddShadow(text.gameObject);
        return text;
    }

    /// <summary>
    /// カードの裏面(金の縁と中央の紋章)
    /// </summary>
    private static RectTransform CreateCardBack(Transform parent)
    {
        var border = CreateImage("CardBack", parent, Gold, RoundSprite);
        SetRect(border.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, CardSize);
        border.raycastTarget = false;
        var inner = CreateImage("Inner", border.transform, new Color(0.3f, 0.18f, 0.35f), RoundSprite);
        Stretch(inner.rectTransform, 8);
        inner.raycastTarget = false;
        var pattern = CreateImage("Pattern", border.transform, new Color(1f, 1f, 1f, 0.15f), CardBackgroundSprite);
        Stretch(pattern.rectTransform, 12);
        pattern.raycastTarget = false;
        var emblem = CreateImage("Emblem", border.transform, Gold, KnobSprite);
        SetRect(emblem.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110, 110));
        emblem.raycastTarget = false;
        var mark = CreateText("Mark", emblem.transform, "？", 64, Ink, TextAnchor.MiddleCenter);
        Stretch(mark.rectTransform);
        return border.rectTransform;
    }

    /// <summary>
    /// ガチャ結果のカード。後ろに光、表と裏、右上にNEWの印
    /// </summary>
    private static GachaResultCardView CreateResultCard(Transform parent)
    {
        var rect = CreateRect("ResultTemplate", parent);
        SetRect(rect, new Vector2(0.5f, 0.5f), Vector2.zero, CardSize);
        var view = rect.gameObject.AddComponent<GachaResultCardView>();
        var body = CreateRect("Body", rect);
        SetRect(body, new Vector2(0.5f, 0.5f), Vector2.zero, CardSize);

        var glow = CreateImage("Glow", body, new Color(1f, 0.8f, 0.35f, 0f), KnobSprite);
        SetRect(glow.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320, 390));
        glow.raycastTarget = false;
        //表はデッキ編集と同じ見た目にする
        var face = DeckBuildScreenPrefabGenerator.CreateCardFace(body);
        var back = CreateCardBack(body);
        var badge = CreateImage("NewBadge", body, ButtonRed, RoundSprite);
        SetRect(badge.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(72, 128), new Vector2(84, 40));
        badge.rectTransform.localEulerAngles = new Vector3(0, 0, -12);
        badge.raycastTarget = false;
        var badgeText = CreateText("Text", badge.transform, "NEW", 24, Color.white, TextAnchor.MiddleCenter);
        Stretch(badgeText.rectTransform);
        badgeText.fontStyle = FontStyle.Bold;

        var so = new SerializedObject(view);
        so.FindProperty("_body").objectReferenceValue = body;
        so.FindProperty("_face").objectReferenceValue = face;
        so.FindProperty("_back").objectReferenceValue = back.gameObject;
        so.FindProperty("_newBadge").objectReferenceValue = badge.gameObject;
        so.FindProperty("_glow").objectReferenceValue = glow;
        so.ApplyModifiedPropertiesWithoutUndo();
        rect.gameObject.SetActive(false);
        return view;
    }
}

//日本語対応
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UiPrefabBuilder;

/// <summary>
/// デッキ編集画面のPrefabと設定アセットを自動生成するエディタ拡張
/// 左にカード一覧(検索・コスト絞り込み付き)、右にデッキのステータスと編成一覧を置く
/// </summary>
public static class DeckBuildScreenPrefabGenerator
{
    private const string PrefabPath = "Assets/InGame/Prefab/ScreenPrefab/PrefabDeckBuildScreen.prefab";
    private const string SettingsPath = "Assets/InGame/Data/DeckBuild/DeckBuildSettings.asset";
    private const string AttackIconPath = "Assets/InGame/Image/Icon/戦のアイコン (1).png";
    private const string DefenceIconPath = "Assets/InGame/Image/Icon/シールドアイコン5 (1).png";

    /// <summary>コストの列数。設定(MaxCostColumn)から決める</summary>
    private static int CostColumnCount;

    [MenuItem("Tools/CardGame/Generate DeckBuild Screen Prefab")]
    public static void Generate()
    {
        LoadAssets();
        //設定アセットはApplicationCompositionRootから登録するので、ここでは作成と値の更新だけ行う
        var settings = LoadOrCreateSettings();
        //ボタンと棒の数は設定に合わせる。設定を変えたら再生成すればPrefabも揃う
        CostColumnCount = Mathf.Max(1, settings.MaxCostColumn + 1);

        var root = new GameObject("PrefabDeckBuildScreen");
        root.layer = 5;
        var screen = root.AddComponent<DeckBuildScreen>();
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
        var title = CreateText("TitleText", header.transform, "デッキ編集", 50, Gold, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 64));
        AddShadow(title.gameObject);
        var clearButton = CreateButton("ClearButton", header.transform, "すべて削除", 26, ButtonGray, out _);
        SetRect(clearButton.image.rectTransform, new Vector2(1f, 0.5f), new Vector2(-395, 0), new Vector2(220, 66));
        var saveButton = CreateButton("SaveButton", header.transform, "デッキ保存", 28, ButtonRed, out _);
        SetRect(saveButton.image.rectTransform, new Vector2(1f, 0.5f), new Vector2(-140, 0), new Vector2(240, 66));

        //カード一覧
        var listPanel = CreatePanel("CardListPanel", canvas, new Vector2(-310, -35), new Vector2(1260, 900));
        var keyword = CreateInputField("KeywordInput", listPanel, "カード名・効果で検索");
        SetRect((RectTransform)keyword.transform, new Vector2(0f, 1f), new Vector2(210, -45), new Vector2(380, 56));
        var costLabel = CreateText("CostLabel", listPanel, "コスト", 24, new Color(0.92f, 0.9f, 0.86f), TextAnchor.MiddleCenter);
        SetRect(costLabel.rectTransform, new Vector2(0f, 1f), new Vector2(455, -45), new Vector2(90, 56));
        var costButtons = new Object[CostColumnCount + 1];
        var allButton = CreateButton("Cost_All", listPanel, "全て", 24, ButtonGray, out _);
        SetRect(allButton.image.rectTransform, new Vector2(0f, 1f), new Vector2(550, -45), new Vector2(90, 56));
        costButtons[0] = allButton;
        for (int i = 0; i < CostColumnCount; i++)
        {
            var label = i == CostColumnCount - 1 ? $"{i}+" : i.ToString();
            var costButton = CreateButton($"Cost_{i}", listPanel, label, 26, ButtonGray, out _);
            SetRect(costButton.image.rectTransform, new Vector2(0f, 1f), new Vector2(630 + i * 70, -45), new Vector2(60, 56));
            costButtons[i + 1] = costButton;
        }
        var listLine = CreateImage("Line", listPanel, Brown);
        SetRect(listLine.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -88), new Vector2(1220, 3));
        var tileContent = CreateScroll(listPanel, -100f, true);
        var emptyText = CreateText("EmptyText", listPanel, "条件に合うカードがありません", 28, new Color(1f, 1f, 1f, 0.5f), TextAnchor.MiddleCenter);
        SetRect(emptyText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(800, 60));
        var tileTemplate = CreateTile(canvas);

        //デッキのステータスと編成一覧
        var deckPanel = CreatePanel("DeckPanel", canvas, new Vector2(640, -35), new Vector2(560, 900));
        var countLabel = CreateText("CountLabel", deckPanel, "デッキ枚数", 22, SubText, TextAnchor.MiddleLeft);
        SetRect(countLabel.rectTransform, new Vector2(0f, 1f), new Vector2(125, -30), new Vector2(210, 30));
        var deckCount = CreateText("DeckCountText", deckPanel, "0 / 30", 56, Color.white, TextAnchor.MiddleLeft);
        SetRect(deckCount.rectTransform, new Vector2(0f, 1f), new Vector2(140, -80), new Vector2(240, 70));
        AddShadow(deckCount.gameObject);
        var ruleText = CreateText("RuleText", deckPanel, "", 15, SubText, TextAnchor.MiddleLeft);
        SetRect(ruleText.rectTransform, new Vector2(0f, 1f), new Vector2(145, -125), new Vector2(250, 26));
        SetBestFit(ruleText, 10);
        var typeText = CreateText("TypeText", deckPanel, "", 20, new Color(0.92f, 0.9f, 0.86f), TextAnchor.MiddleLeft);
        SetRect(typeText.rectTransform, new Vector2(0f, 1f), new Vector2(145, -158), new Vector2(250, 30));
        var curve = CreateCostCurve(deckPanel);
        var deckLine = CreateImage("Line", deckPanel, Brown);
        SetRect(deckLine.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -188), new Vector2(520, 3));
        var rowContent = CreateScroll(deckPanel, -200f, false);
        var deckEmpty = CreateText("DeckEmptyText", deckPanel, "デッキにカードがありません\n左の一覧から追加してください", 24, new Color(1f, 1f, 1f, 0.5f), TextAnchor.MiddleCenter);
        SetRect(deckEmpty.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(500, 100));
        var rowTemplate = CreateRow(canvas);

        var hint = CreateText("HintText", canvas, "", 24, new Color(1f, 1f, 1f, 0.75f), TextAnchor.MiddleCenter);
        SetRect(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 25), new Vector2(1600, 34));
        AddShadow(hint.gameObject);

        var detail = CreateDetail(canvas);

        //確認のポップアップ
        var popup = CreateImage("Popup", canvas, new Color(0f, 0f, 0f, 0.65f));
        Stretch(popup.rectTransform);
        var popupGroup = popup.gameObject.AddComponent<CanvasGroup>();
        var popupFrame = CreateImage("Frame", popup.transform, Gold, RoundSprite);
        SetRect(popupFrame.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820, 320));
        var popupInner = CreateImage("Inner", popupFrame.transform, new Color(0.07f, 0.05f, 0.1f), RoundSprite);
        Stretch(popupInner.rectTransform, 6);
        var popupBody = CreateText("BodyText", popupFrame.transform, "", 30, Color.white, TextAnchor.MiddleCenter);
        SetRect(popupBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(760, 140));
        var popupYes = CreateButton("YesButton", popupFrame.transform, "はい", 28, ButtonRed, out var popupYesText);
        SetRect(popupYes.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-180, -90), new Vector2(300, 70));
        var popupNo = CreateButton("NoButton", popupFrame.transform, "いいえ", 28, ButtonGray, out var popupNoText);
        SetRect(popupNo.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(180, -90), new Vector2(300, 70));
        popup.gameObject.SetActive(false);

        var fade = CreateImage("Fade", canvas, Color.black);
        Stretch(fade.rectTransform);

        //LifetimeScope
        var scopeObj = new GameObject("LifeTimeScope");
        scopeObj.transform.SetParent(root.transform, false);
        var scope = scopeObj.AddComponent<DeckBuildScreenLifeTimeScope>();
        scope.parentReference = VContainer.Unity.ParentReference.Create<ApplicationCompositionRoot>();
        var scopeSo = new SerializedObject(scope);
        var inject = scopeSo.FindProperty("autoInjectGameObjects");
        inject.arraySize = 1;
        inject.GetArrayElementAtIndex(0).objectReferenceValue = root;
        scopeSo.ApplyModifiedPropertiesWithoutUndo();

        var so = new SerializedObject(screen);
        so.FindProperty("_tileContent").objectReferenceValue = tileContent;
        so.FindProperty("_tileTemplate").objectReferenceValue = tileTemplate;
        so.FindProperty("_emptyText").objectReferenceValue = emptyText.gameObject;
        so.FindProperty("_keywordInput").objectReferenceValue = keyword;
        SetArray(so.FindProperty("_costFilterButtons"), costButtons);
        so.FindProperty("_filterNormalColor").colorValue = ButtonGray;
        so.FindProperty("_rowContent").objectReferenceValue = rowContent;
        so.FindProperty("_rowTemplate").objectReferenceValue = rowTemplate;
        so.FindProperty("_costCurve").objectReferenceValue = curve;
        so.FindProperty("_deckCountText").objectReferenceValue = deckCount;
        so.FindProperty("_deckRuleText").objectReferenceValue = ruleText;
        so.FindProperty("_typeText").objectReferenceValue = typeText;
        so.FindProperty("_deckEmptyText").objectReferenceValue = deckEmpty.gameObject;
        so.FindProperty("_backButton").objectReferenceValue = backButton;
        so.FindProperty("_clearButton").objectReferenceValue = clearButton;
        so.FindProperty("_saveButton").objectReferenceValue = saveButton;
        so.FindProperty("_hintText").objectReferenceValue = hint;
        so.FindProperty("_detail").objectReferenceValue = detail;
        so.FindProperty("_fadeImage").objectReferenceValue = fade;
        so.FindProperty("_popup").objectReferenceValue = popupGroup;
        so.FindProperty("_popupBodyText").objectReferenceValue = popupBody;
        so.FindProperty("_popupYesButton").objectReferenceValue = popupYes;
        so.FindProperty("_popupYesText").objectReferenceValue = popupYesText;
        so.FindProperty("_popupNoButton").objectReferenceValue = popupNo;
        so.FindProperty("_popupNoText").objectReferenceValue = popupNoText;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log($"デッキ編集画面のPrefabを生成しました: {PrefabPath}");
    }

    /// <summary>
    /// ほかの画面(ショップのガチャ結果など)でも、デッキ編集と同じ見た目のカードの表面を作る
    /// </summary>
    public static DeckCardFaceView CreateCardFace(Transform parent)
    {
        LoadAssets();
        return CreateFace(parent);
    }

    private static DeckBuildSettings LoadOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<DeckBuildSettings>(SettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<DeckBuildSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
        }
        if (settings.AttackIcon == null) settings.AttackIcon = AssetDatabase.LoadAssetAtPath<Sprite>(AttackIconPath);
        if (settings.DefenceIcon == null) settings.DefenceIcon = AssetDatabase.LoadAssetAtPath<Sprite>(DefenceIconPath);
        if (settings.RarityColors.Count == 0)
        {
            //ブロンズ・シルバー・ゴールド・レジェンド
            settings.RarityColors = new List<Color>
            {
                new Color(0.75f, 0.54f, 0.35f),
                new Color(0.79f, 0.82f, 0.85f),
                Gold,
                new Color(0.79f, 0.55f, 1f),
            };
        }
        EditorUtility.SetDirty(settings);
        return settings;
    }

    /// <summary>
    /// 戦闘のカードと同じ枠にコスト・名前・説明・分類のアイコンを載せたカードの表面
    /// </summary>
    private static DeckCardFaceView CreateFace(Transform parent)
    {
        var rect = CreateRect("Face", parent);
        SetRect(rect, new Vector2(0.5f, 0.5f), Vector2.zero, CardSize);
        var face = rect.gameObject.AddComponent<DeckCardFaceView>();

        var background = CreateImage("Background", rect, Color.white, CardBackgroundSprite);
        //枠の角からはみ出さないよう、枠より一回り小さくする
        SetRect(background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(2, 0), new Vector2(174, 246));
        background.raycastTarget = false;
        var icon = CreateImage("Icon", rect, new Color(0.29f, 0.23f, 0.16f, 0.85f));
        SetRect(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(2, 22), new Vector2(92, 92));
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var frame = CreateImage("Frame", rect, Color.white, FrameSprite);
        SetRect(frame.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, CardSize);
        frame.raycastTarget = false;
        var rarity = CreateImage("Rarity", rect, Gold);
        SetRect(rarity.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(2, -126), new Vector2(120, 5));
        rarity.raycastTarget = false;
        var cost = CreateText("Cost", rect, "1", 28, Ink, TextAnchor.MiddleCenter);
        SetRect(cost.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-66, 100), new Vector2(40, 40));
        cost.fontStyle = FontStyle.Bold;
        var name = CreateText("Name", rect, "カード名", 16, Ink, TextAnchor.MiddleCenter);
        SetRect(name.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(16, 103), new Vector2(112, 26));
        SetBestFit(name, 9);
        var description = CreateText("Description", rect, "説明", 14, Ink, TextAnchor.MiddleCenter);
        SetRect(description.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(2, -78), new Vector2(158, 70));
        SetBestFit(description, 9);
        description.verticalOverflow = VerticalWrapMode.Truncate;

        var so = new SerializedObject(face);
        so.FindProperty("_costText").objectReferenceValue = cost;
        so.FindProperty("_nameText").objectReferenceValue = name;
        so.FindProperty("_descriptionText").objectReferenceValue = description;
        so.FindProperty("_iconImage").objectReferenceValue = icon;
        so.FindProperty("_rarityImage").objectReferenceValue = rarity;
        so.ApplyModifiedPropertiesWithoutUndo();
        return face;
    }

    /// <summary>
    /// カード一覧のタイル。カードの下に「＋ 追加」、右上に虫眼鏡、右下にデッキに入っている枚数
    /// </summary>
    private static DeckCardTileView CreateTile(Transform parent)
    {
        var rect = CreateRect("TileTemplate", parent);
        rect.sizeDelta = new Vector2(CardSize.x, 336);
        var view = rect.gameObject.AddComponent<DeckCardTileView>();

        var body = CreateImage("Body", rect, new Color(0f, 0f, 0f, 0f));
        body.rectTransform.anchorMin = body.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        body.rectTransform.anchoredPosition = new Vector2(0, -CardSize.y / 2f);
        body.rectTransform.sizeDelta = CardSize;
        var face = CreateFace(body.transform);
        var cardButton = body.gameObject.AddComponent<Button>();
        cardButton.targetGraphic = face.transform.Find("Frame").GetComponent<Image>();
        cardButton.colors = DisabledGrayColors();

        var badge = CreateImage("CountBadge", body.transform, new Color(0f, 0f, 0f, 0.6f), RoundSprite);
        SetRect(badge.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(64, -18), new Vector2(58, 32));
        badge.raycastTarget = false;
        var count = CreateText("CountText", badge.transform, "0/3", 20, Color.white, TextAnchor.MiddleCenter);
        Stretch(count.rectTransform);

        var overlay = CreateImage("LimitOverlay", body.transform, new Color(0f, 0f, 0f, 0.6f), RoundSprite);
        SetRect(overlay.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(2, 0), new Vector2(196, 268));
        overlay.raycastTarget = false;
        var limitText = CreateText("Text", overlay.transform, "上限", 40, Color.white, TextAnchor.MiddleCenter);
        Stretch(limitText.rectTransform);
        AddShadow(limitText.gameObject);
        overlay.gameObject.SetActive(false);

        var detailButton = CreateMagnifier(body.transform, new Vector2(66, 58));

        var addButton = CreateButton("AddButton", rect, "＋ 追加", 24, ButtonGreen, out _);
        addButton.image.rectTransform.anchorMin = addButton.image.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        addButton.image.rectTransform.anchoredPosition = new Vector2(0, -310);
        addButton.image.rectTransform.sizeDelta = new Vector2(190, 46);

        var so = new SerializedObject(view);
        so.FindProperty("_face").objectReferenceValue = face;
        so.FindProperty("_body").objectReferenceValue = body.rectTransform;
        so.FindProperty("_cardButton").objectReferenceValue = cardButton;
        so.FindProperty("_addButton").objectReferenceValue = addButton;
        so.FindProperty("_detailButton").objectReferenceValue = detailButton;
        so.FindProperty("_countText").objectReferenceValue = count;
        so.FindProperty("_countBadge").objectReferenceValue = badge;
        so.FindProperty("_limitOverlay").objectReferenceValue = overlay.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();
        rect.gameObject.SetActive(false);
        return view;
    }

    /// <summary>
    /// 丸い背景に虫眼鏡の形を重ねた詳細ボタン
    /// </summary>
    private static Button CreateMagnifier(Transform parent, Vector2 pos)
    {
        var bg = CreateImage("DetailButton", parent, new Color(0f, 0f, 0f, 0.7f), KnobSprite);
        SetRect(bg.rectTransform, new Vector2(0.5f, 0.5f), pos, new Vector2(44, 44));
        var button = bg.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;
        var lens = CreateImage("Lens", bg.transform, Color.white, KnobSprite);
        SetRect(lens.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-3, 3), new Vector2(22, 22));
        lens.raycastTarget = false;
        var hole = CreateImage("Hole", lens.transform, Color.black, KnobSprite);
        SetRect(hole.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(14, 14));
        hole.raycastTarget = false;
        var handle = CreateImage("Handle", bg.transform, Color.white);
        SetRect(handle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(9, -9), new Vector2(12, 5));
        handle.rectTransform.localEulerAngles = new Vector3(0, 0, -45);
        handle.raycastTarget = false;
        return button;
    }

    /// <summary>
    /// デッキ編成一覧の1行。左にコストと名前、右に枚数と「−」「＋」「×」
    /// </summary>
    private static DeckListRowView CreateRow(Transform parent)
    {
        var rect = CreateRect("RowTemplate", parent);
        rect.sizeDelta = new Vector2(528, 64);
        var layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = layout.preferredHeight = 64;
        var view = rect.gameObject.AddComponent<DeckListRowView>();

        var body = CreateImage("Body", rect, new Color(0.16f, 0.13f, 0.15f), RoundSprite);
        Stretch(body.rectTransform);
        var art = CreateImage("Art", body.transform, new Color(1f, 1f, 1f, 0.12f), CardBackgroundSprite);
        Stretch(art.rectTransform, 2);
        art.raycastTarget = false;
        var flash = CreateImage("Flash", body.transform, new Color(1f, 0.91f, 0.65f), RoundSprite);
        Stretch(flash.rectTransform);
        flash.raycastTarget = false;
        var detailHit = CreateImage("DetailButton", body.transform, new Color(0f, 0f, 0f, 0f));
        SetRect(detailHit.rectTransform, new Vector2(0f, 0.5f), new Vector2(160, 0), new Vector2(320, 64));
        var detailButton = detailHit.gameObject.AddComponent<Button>();
        detailButton.targetGraphic = detailHit;
        var rarity = CreateImage("Rarity", body.transform, Gold);
        SetRect(rarity.rectTransform, new Vector2(0f, 0.5f), new Vector2(4, 0), new Vector2(6, 56));
        rarity.raycastTarget = false;
        var costBg = CreateImage("Cost", body.transform, Gold, KnobSprite);
        SetRect(costBg.rectTransform, new Vector2(0f, 0.5f), new Vector2(38, 0), new Vector2(46, 46));
        costBg.raycastTarget = false;
        var cost = CreateText("CostText", costBg.transform, "1", 26, Ink, TextAnchor.MiddleCenter);
        Stretch(cost.rectTransform);
        cost.fontStyle = FontStyle.Bold;
        var icon = CreateImage("Icon", body.transform, new Color(0.92f, 0.9f, 0.86f));
        SetRect(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(84, 0), new Vector2(32, 32));
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var name = CreateText("NameText", body.transform, "カード名", 24, Color.white, TextAnchor.MiddleLeft);
        SetRect(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(205, 0), new Vector2(200, 40));
        SetBestFit(name, 12);
        AddShadow(name.gameObject);
        var count = CreateText("CountText", body.transform, "×1", 28, Gold, TextAnchor.MiddleCenter);
        SetRect(count.rectTransform, new Vector2(1f, 0.5f), new Vector2(-192, 0), new Vector2(64, 40));
        var minus = CreateButton("MinusButton", body.transform, "−", 30, ButtonGray, out _);
        SetRect(minus.image.rectTransform, new Vector2(1f, 0.5f), new Vector2(-138, 0), new Vector2(42, 42));
        var plus = CreateButton("PlusButton", body.transform, "＋", 28, ButtonGreen, out _);
        SetRect(plus.image.rectTransform, new Vector2(1f, 0.5f), new Vector2(-90, 0), new Vector2(42, 42));
        var remove = CreateButton("RemoveButton", body.transform, "×", 28, new Color(0.54f, 0.11f, 0.11f), out _);
        SetRect(remove.image.rectTransform, new Vector2(1f, 0.5f), new Vector2(-36, 0), new Vector2(42, 42));

        var so = new SerializedObject(view);
        so.FindProperty("_body").objectReferenceValue = body.rectTransform;
        so.FindProperty("_costText").objectReferenceValue = cost;
        so.FindProperty("_nameText").objectReferenceValue = name;
        so.FindProperty("_countText").objectReferenceValue = count;
        so.FindProperty("_iconImage").objectReferenceValue = icon;
        so.FindProperty("_rarityImage").objectReferenceValue = rarity;
        so.FindProperty("_detailButton").objectReferenceValue = detailButton;
        so.FindProperty("_plusButton").objectReferenceValue = plus;
        so.FindProperty("_minusButton").objectReferenceValue = minus;
        so.FindProperty("_removeButton").objectReferenceValue = remove;
        so.FindProperty("_flashImage").objectReferenceValue = flash;
        so.ApplyModifiedPropertiesWithoutUndo();
        rect.gameObject.SetActive(false);
        return view;
    }

    /// <summary>
    /// コスト分布の棒グラフ。棒は下端を基準に伸ばす
    /// </summary>
    private static DeckCostCurveView CreateCostCurve(Transform parent)
    {
        var rect = CreateRect("CostCurve", parent);
        SetRect(rect, new Vector2(1f, 1f), new Vector2(-140, -95), new Vector2(250, 170));
        var view = rect.gameObject.AddComponent<DeckCostCurveView>();
        var label = CreateText("Label", rect, "コスト分布", 16, SubText, TextAnchor.MiddleCenter);
        SetRect(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -8), new Vector2(250, 20));
        var baseLine = CreateImage("Base", rect, Brown);
        SetRect(baseLine.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -46), new Vector2(250, 2));
        baseLine.raycastTarget = false;

        var bars = new Object[CostColumnCount];
        var counts = new Object[CostColumnCount];
        for (int i = 0; i < CostColumnCount; i++)
        {
            var x = -108 + i * 36;
            var bar = CreateImage($"Bar{i}", rect, Gold);
            SetRect(bar.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, -45), new Vector2(24, 4));
            bar.rectTransform.pivot = new Vector2(0.5f, 0f);
            bar.raycastTarget = false;
            bars[i] = bar.rectTransform;
            var count = CreateText($"Count{i}", rect, "", 16, Color.white, TextAnchor.MiddleCenter);
            SetRect(count.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, 56), new Vector2(34, 20));
            counts[i] = count;
            var costLabel = CreateText($"Label{i}", rect, i == CostColumnCount - 1 ? $"{i}+" : i.ToString(), 16, SubText, TextAnchor.MiddleCenter);
            SetRect(costLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, -60), new Vector2(34, 20));
        }

        var so = new SerializedObject(view);
        SetArray(so.FindProperty("_bars"), bars);
        SetArray(so.FindProperty("_countTexts"), counts);
        so.FindProperty("_maxBarHeight").floatValue = 90f;
        so.ApplyModifiedPropertiesWithoutUndo();
        return view;
    }

    /// <summary>
    /// カードの詳細。左に大きなカード、右に名前・説明・枚数と増減ボタン
    /// </summary>
    private static DeckCardDetailView CreateDetail(Transform parent)
    {
        var bg = CreateImage("CardDetail", parent, new Color(0f, 0f, 0f, 0.7f));
        Stretch(bg.rectTransform);
        var group = bg.gameObject.AddComponent<CanvasGroup>();
        var bgButton = bg.gameObject.AddComponent<Button>();
        bgButton.targetGraphic = bg;
        bgButton.transition = Selectable.Transition.None;
        var view = bg.gameObject.AddComponent<DeckCardDetailView>();

        var window = CreateImage("Window", bg.transform, Gold, RoundSprite);
        SetRect(window.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(960, 600));
        var inner = CreateImage("Inner", window.transform, new Color(0.086f, 0.063f, 0.086f), RoundSprite);
        Stretch(inner.rectTransform, 6);
        var faceHolder = CreateRect("FaceHolder", window.transform);
        SetRect(faceHolder, new Vector2(0.5f, 0.5f), new Vector2(-250, 0), CardSize);
        faceHolder.localScale = Vector3.one * 1.8f;
        var face = CreateFace(faceHolder);

        var name = CreateText("NameText", window.transform, "", 46, Gold, TextAnchor.MiddleLeft);
        SetRect(name.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(190, 215), new Vector2(500, 64));
        AddShadow(name.gameObject);
        var info = CreateText("InfoText", window.transform, "", 22, SubText, TextAnchor.MiddleLeft);
        SetRect(info.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(190, 160), new Vector2(500, 32));
        var line = CreateImage("Line", window.transform, Brown);
        SetRect(line.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(190, 132), new Vector2(500, 2));
        var description = CreateText("DescriptionText", window.transform, "", 30, Color.white, TextAnchor.UpperLeft);
        SetRect(description.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(190, 30), new Vector2(500, 180));
        var count = CreateText("CountText", window.transform, "", 28, Gold, TextAnchor.MiddleLeft);
        SetRect(count.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(190, -110), new Vector2(500, 40));
        var minus = CreateButton("MinusButton", window.transform, "− 外す", 28, ButtonGray, out _);
        SetRect(minus.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(40, -200), new Vector2(200, 70));
        var plus = CreateButton("PlusButton", window.transform, "＋ 追加", 28, ButtonGreen, out _);
        SetRect(plus.image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(270, -200), new Vector2(220, 70));
        var close = CreateButton("CloseButton", window.transform, "×", 36, ButtonGray, out _);
        SetRect(close.image.rectTransform, new Vector2(1f, 1f), new Vector2(-44, -44), new Vector2(60, 60));

        var so = new SerializedObject(view);
        so.FindProperty("_group").objectReferenceValue = group;
        so.FindProperty("_window").objectReferenceValue = window.rectTransform;
        so.FindProperty("_face").objectReferenceValue = face;
        so.FindProperty("_nameText").objectReferenceValue = name;
        so.FindProperty("_infoText").objectReferenceValue = info;
        so.FindProperty("_descriptionText").objectReferenceValue = description;
        so.FindProperty("_countText").objectReferenceValue = count;
        so.FindProperty("_plusButton").objectReferenceValue = plus;
        so.FindProperty("_minusButton").objectReferenceValue = minus;
        so.FindProperty("_closeButton").objectReferenceValue = close;
        so.FindProperty("_backgroundButton").objectReferenceValue = bgButton;
        so.ApplyModifiedPropertiesWithoutUndo();
        bg.gameObject.SetActive(false);
        return view;
    }

    /// <summary>
    /// パネルの中に縦スクロールの一覧を作る
    /// </summary>
    /// <param name="top">パネル上端からの位置(負の値)</param>
    /// <param name="isGrid">trueならカードのタイルを格子状に、falseなら行を縦に並べる</param>
    /// <returns>中身を並べるContent</returns>
    private static RectTransform CreateScroll(Transform panel, float top, bool isGrid)
    {
        var viewport = CreateRect(isGrid ? "TileViewport" : "RowViewport", panel);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(isGrid ? 20 : 16, 16);
        viewport.offsetMax = new Vector2(isGrid ? -20 : -16, top);
        viewport.gameObject.AddComponent<RectMask2D>();
        //ドラッグでスクロールできるよう透明なImageを置く
        viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        var content = CreateRect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.sizeDelta = Vector2.zero;
        content.anchoredPosition = Vector2.zero;
        if (isGrid)
        {
            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CardSize.x, 336);
            grid.spacing = new Vector2(32, 22);
            //カードが少なくても左上から詰めて並べる(5列分の幅が中央に来るよう左に余白を入れる)
            grid.padding = new RectOffset(21, 0, 10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.childAlignment = TextAnchor.UpperLeft;
        }
        else
        {
            var list = content.gameObject.AddComponent<VerticalLayoutGroup>();
            list.spacing = 8;
            list.padding = new RectOffset(0, 0, 4, 4);
            list.childControlWidth = true;
            list.childControlHeight = true;
            list.childForceExpandWidth = true;
            list.childForceExpandHeight = false;
        }
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = panel.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;
        return content;
    }

    private static InputField CreateInputField(string name, Transform parent, string placeholder)
    {
        var bg = CreateImage(name, parent, new Color(0.16f, 0.13f, 0.15f), RoundSprite);
        var input = bg.gameObject.AddComponent<InputField>();
        input.targetGraphic = bg;
        var text = CreateText("Text", bg.transform, "", 24, Color.white, TextAnchor.MiddleLeft);
        Stretch(text.rectTransform, 12);
        text.supportRichText = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        var placeholderText = CreateText("Placeholder", bg.transform, placeholder, 22, new Color(1f, 1f, 1f, 0.4f), TextAnchor.MiddleLeft);
        Stretch(placeholderText.rectTransform, 12);
        input.textComponent = text;
        input.placeholder = placeholderText;
        input.lineType = InputField.LineType.SingleLine;
        return input;
    }

    private static void SetArray(SerializedProperty prop, Object[] values)
    {
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void SetBestFit(Text text, int minSize)
    {
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = minSize;
        text.resizeTextMaxSize = text.fontSize;
    }
}

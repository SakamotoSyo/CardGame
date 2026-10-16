//日本語対応
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステージ選択画面のPrefabと設定アセットを自動生成するエディタ拡張
/// Canvasの設定はタイトル画面のPrefabから引き継ぐ
/// </summary>
public static class StageSelectScreenPrefabGenerator
{
    private const string PrefabPath = "Assets/InGame/Prefab/ScreenPrefab/PrefabStageSelectScreen.prefab";
    private const string SettingsPath = "Assets/InGame/Data/StageSelect/StageMapSettings.asset";
    private const string TitlePrefabPath = "Assets/InGame/Prefab/ScreenPrefab/PrefabTitleScreen.prefab";
    private const string FontPath = "Assets/InGame/Font/ReggaeOne-Regular.ttf";

    private static readonly Color Gold = new Color(1f, 0.8f, 0.35f);
    private static readonly Color Ink = new Color(0.07f, 0.05f, 0.1f);

    //盤面のレイアウト。今日の列を一番下に置き、奥の日を上に同じ大きさで並べる
    private const float TodayRowY = -335f;
    private const float RowSpacing = 310f;
    private const float ColumnSpacing = 280f;
    private static readonly Vector2 CardSize = new Vector2(240, 300);

    private static Font _font;
    private static Sprite _roundSprite;
    private static Sprite _knobSprite;

    [MenuItem("Tools/CardGame/Generate StageSelect Screen Prefab")]
    public static void Generate()
    {
        _font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        _roundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        _knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var settings = LoadOrCreateSettings();

        var root = new GameObject("PrefabStageSelectScreen");
        root.layer = 5;
        var screen = root.AddComponent<StageSelectScreen>();
        var canvas = CreateCanvas(root.transform).transform;

        //背景(何もないところのタップで選択解除)
        var bg = CreateImage("Background", canvas, new Color(0.05f, 0.035f, 0.045f));
        Stretch(bg.rectTransform);
        var bgButton = bg.gameObject.AddComponent<Button>();
        bgButton.transition = Selectable.Transition.None;

        //石の台のような盤面
        var table = CreateImage("Table", canvas, new Color(0.15f, 0.115f, 0.1f));
        SetRect(table.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1480, 1080));
        table.raycastTarget = false;
        var tableLight = CreateImage("TableLight", table.transform, new Color(0.55f, 0.4f, 0.28f, 0.22f), _knobSprite);
        SetRect(tableLight.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(1500, 1100));
        tableLight.raycastTarget = false;
        foreach (var side in new[] { -1, 1 })
        {
            var edge = CreateImage(side < 0 ? "EdgeL" : "EdgeR", table.transform, new Color(0.42f, 0.32f, 0.18f));
            SetRect(edge.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(side * 740, 0), new Vector2(8, 1080));
            edge.raycastTarget = false;
        }

        //カードの列(盤面ごとスクロールさせる)
        var board = CreateRect("Board", canvas);
        SetRect(board, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1480, 1080));
        var rows = new RectTransform[StageMapGenerator.VisibleDays];
        for (int i = rows.Length - 1; i >= 0; i--)
        {
            rows[i] = CreateRect($"Row{i}", board);
            SetRect(rows[i], new Vector2(0.5f, 0.5f), new Vector2(0, TodayRowY + RowSpacing * i), new Vector2(1480, CardSize.y));
        }
        var nodeTemplate = CreateNode(rows[0]);

        //カードの説明
        var tooltipLayer = CreateRect("TooltipLayer", canvas);
        Stretch(tooltipLayer);
        var tooltipFrame = CreateImage("Tooltip", tooltipLayer, Gold, _roundSprite);
        SetRect(tooltipFrame.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 210));
        tooltipFrame.raycastTarget = false;
        var tooltipGroup = tooltipFrame.gameObject.AddComponent<CanvasGroup>();
        tooltipGroup.blocksRaycasts = false;
        var pointer = CreateImage("Pointer", tooltipFrame.transform, Gold);
        SetRect(pointer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, -2), new Vector2(30, 30));
        pointer.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        var tooltipInner = CreateImage("Inner", tooltipFrame.transform, new Color(0.09f, 0.06f, 0.07f, 0.97f), _roundSprite);
        Stretch(tooltipInner.rectTransform, 4);
        var accent = CreateImage("Accent", tooltipFrame.transform, Color.gray);
        SetRect(accent.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -10), new Vector2(560, 6));
        var tooltipName = CreateText("NameText", tooltipFrame.transform, "", 40, Gold, TextAnchor.MiddleCenter);
        SetRect(tooltipName.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -44), new Vector2(560, 50));
        AddShadow(tooltipName.gameObject);
        var tooltipDesc = CreateText("DescriptionText", tooltipFrame.transform, "", 24, new Color(0.92f, 0.9f, 0.86f), TextAnchor.UpperCenter);
        SetRect(tooltipDesc.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -132), new Vector2(560, 110));
        tooltipDesc.lineSpacing = 1.05f;

        //決定したカードを拡大するレイヤーと、光の粒・フラッシュ
        var boardDim = CreateImage("BoardDim", canvas, new Color(0, 0, 0, 0));
        Stretch(boardDim.rectTransform);
        boardDim.raycastTarget = false;
        var focusLayer = CreateRect("FocusLayer", canvas);
        Stretch(focusLayer);
        var fxLayer = CreateRect("FxLayer", canvas);
        Stretch(fxLayer);
        var effects = fxLayer.gameObject.AddComponent<StageSelectEffects>();
        var spark = CreateImage("Spark", fxLayer, Color.white, _knobSprite);
        spark.raycastTarget = false;
        var flash = CreateImage("Flash", fxLayer, new Color(1, 1, 1, 0));
        Stretch(flash.rectTransform);
        flash.raycastTarget = false;

        //ヘッダー
        var header = CreateImage("Header", canvas, new Color(0.03f, 0.02f, 0.03f, 0.92f));
        SetRect(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(1920, 100));
        var headerLine = CreateImage("Line", header.transform, new Color(0.6f, 0.45f, 0.2f));
        SetRect(headerLine.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 1), new Vector2(1920, 3));
        var backButton = CreateButton("BackButton", header.transform, "＜ タイトル", 28, new Color(0.25f, 0.2f, 0.25f), out _);
        SetRect((RectTransform)backButton.transform, new Vector2(0f, 0.5f), new Vector2(140, 0), new Vector2(230, 66));
        var dayText = CreateText("DayText", header.transform, "DAY 1", 50, Gold, TextAnchor.MiddleCenter);
        SetRect(dayText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 12), new Vector2(500, 64));
        AddShadow(dayText.gameObject);
        var bossText = CreateText("BossText", header.transform, "", 24, new Color(1f, 0.55f, 0.5f), TextAnchor.MiddleCenter);
        SetRect(bossText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(500, 30));
        var cycleText = CreateText("CycleText", header.transform, "第1周", 28, Color.white, TextAnchor.MiddleCenter);
        SetRect(cycleText.rectTransform, new Vector2(1f, 0.5f), new Vector2(-600, 0), new Vector2(160, 60));
        var hpText = CreateText("HpText", header.transform, "HP --", 28, new Color(1f, 0.6f, 0.7f), TextAnchor.MiddleCenter);
        SetRect(hpText.rectTransform, new Vector2(1f, 0.5f), new Vector2(-370, 0), new Vector2(260, 60));
        var goldText = CreateText("GoldText", header.transform, "0 G", 28, Gold, TextAnchor.MiddleCenter);
        SetRect(goldText.rectTransform, new Vector2(1f, 0.5f), new Vector2(-130, 0), new Vector2(200, 60));

        var hint = CreateText("HintText", canvas, "", 22, new Color(1, 1, 1, 0.75f), TextAnchor.MiddleCenter);
        SetRect(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 14), new Vector2(1400, 28));
        AddShadow(hint.gameObject);

        //ポップアップ
        var popup = CreateImage("Popup", canvas, new Color(0f, 0f, 0f, 0.2f));
        Stretch(popup.rectTransform);
        var popupGroup = popup.gameObject.AddComponent<CanvasGroup>();
        var popupFrame = CreateImage("Frame", popup.transform, Gold, _roundSprite);
        SetRect(popupFrame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -390), new Vector2(820, 250));
        var popupInner = CreateImage("Inner", popupFrame.transform, Ink, _roundSprite);
        Stretch(popupInner.rectTransform, 6);
        var popupTitle = CreateText("TitleText", popupFrame.transform, "", 44, Gold, TextAnchor.MiddleCenter);
        SetRect(popupTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -42), new Vector2(760, 56));
        var popupBody = CreateText("BodyText", popupFrame.transform, "", 28, Color.white, TextAnchor.MiddleCenter);
        SetRect(popupBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 8), new Vector2(760, 80));
        var popupOk = CreateButton("OkButton", popupFrame.transform, "OK", 36, new Color(0.7f, 0.12f, 0.12f), out _);
        SetRect((RectTransform)popupOk.transform, new Vector2(0.5f, 0f), new Vector2(0, 44), new Vector2(220, 64));

        //暗転
        var fade = CreateImage("Fade", canvas, Color.black);
        Stretch(fade.rectTransform);

        //LifetimeScope
        var scopeObj = new GameObject("LifeTimeScope");
        scopeObj.transform.SetParent(root.transform, false);
        var scope = scopeObj.AddComponent<StageSelectLifeTimeScope>();
        scope.parentReference = VContainer.Unity.ParentReference.Create<ApplicationCompositionRoot>();
        var scopeSo = new SerializedObject(scope);
        scopeSo.FindProperty("_stageMapSettings").objectReferenceValue = settings;
        var inject = scopeSo.FindProperty("autoInjectGameObjects");
        inject.arraySize = 1;
        inject.GetArrayElementAtIndex(0).objectReferenceValue = root;
        scopeSo.ApplyModifiedPropertiesWithoutUndo();

        //参照の割り当て
        var fxSo = new SerializedObject(effects);
        fxSo.FindProperty("_layer").objectReferenceValue = fxLayer;
        fxSo.FindProperty("_sparkTemplate").objectReferenceValue = spark;
        fxSo.FindProperty("_flashImage").objectReferenceValue = flash;
        fxSo.ApplyModifiedPropertiesWithoutUndo();

        var so = new SerializedObject(screen);
        so.FindProperty("_board").objectReferenceValue = board;
        SetArray(so.FindProperty("_rows"), rows);
        so.FindProperty("_nodeTemplate").objectReferenceValue = nodeTemplate;
        so.FindProperty("_columnSpacing").floatValue = ColumnSpacing;
        so.FindProperty("_rowSpacing").floatValue = RowSpacing;
        so.FindProperty("_boardBackgroundButton").objectReferenceValue = bgButton;
        so.FindProperty("_backButton").objectReferenceValue = backButton;
        so.FindProperty("_dayText").objectReferenceValue = dayText;
        so.FindProperty("_bossText").objectReferenceValue = bossText;
        so.FindProperty("_cycleText").objectReferenceValue = cycleText;
        so.FindProperty("_hpText").objectReferenceValue = hpText;
        so.FindProperty("_goldText").objectReferenceValue = goldText;
        so.FindProperty("_hintText").objectReferenceValue = hint;
        so.FindProperty("_tooltip").objectReferenceValue = tooltipFrame.rectTransform;
        so.FindProperty("_tooltipGroup").objectReferenceValue = tooltipGroup;
        so.FindProperty("_tooltipAccent").objectReferenceValue = accent;
        so.FindProperty("_tooltipNameText").objectReferenceValue = tooltipName;
        so.FindProperty("_tooltipDescriptionText").objectReferenceValue = tooltipDesc;
        so.FindProperty("_effects").objectReferenceValue = effects;
        so.FindProperty("_focusLayer").objectReferenceValue = focusLayer;
        so.FindProperty("_fadeImage").objectReferenceValue = fade;
        so.FindProperty("_boardDimImage").objectReferenceValue = boardDim;
        so.FindProperty("_popup").objectReferenceValue = popupGroup;
        so.FindProperty("_popupTitleText").objectReferenceValue = popupTitle;
        so.FindProperty("_popupBodyText").objectReferenceValue = popupBody;
        so.FindProperty("_popupOkButton").objectReferenceValue = popupOk;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log($"ステージ選択画面のPrefabを生成しました: {PrefabPath}");
    }

    [MenuItem("Tools/CardGame/Reset Dungeon Run")]
    public static void ResetRun()
    {
        var path = Path.Combine(Application.persistentDataPath, DungeonRunRepository.FileName);
        if (File.Exists(path)) File.Delete(path);
        Debug.Log("進行中のランを削除しました");
    }

    private static StageNodeView CreateNode(Transform parent)
    {
        var nodeRect = CreateRect("NodeTemplate", parent);
        SetRect(nodeRect, new Vector2(0.5f, 0.5f), Vector2.zero, CardSize);
        var hit = nodeRect.gameObject.AddComponent<Image>();
        hit.color = new Color(0, 0, 0, 0);
        nodeRect.gameObject.AddComponent<CanvasGroup>();
        var view = nodeRect.gameObject.AddComponent<StageNodeView>();

        //盤面のくぼみ
        var slot = CreateImage("Slot", nodeRect, new Color(0f, 0f, 0f, 0.45f), _roundSprite);
        SetRect(slot.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, CardSize + new Vector2(28, 28));
        slot.raycastTarget = false;

        var body = CreateRect("Body", nodeRect);
        SetRect(body, new Vector2(0.5f, 0.5f), Vector2.zero, CardSize);

        var glow = CreateImage("Glow", body, new Color(1f, 0.85f, 0.45f, 0f), _knobSprite);
        SetRect(glow.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, CardSize + new Vector2(130, 140));
        glow.raycastTarget = false;

        //縁取りのある枠
        var frame = CreateImage("Frame", body, new Color(0.2f, 0.16f, 0.13f), _roundSprite);
        Stretch(frame.rectTransform);
        var goldLine = CreateImage("GoldLine", body, new Color(0.78f, 0.62f, 0.32f), _roundSprite);
        Stretch(goldLine.rectTransform, 5);
        var inner = CreateImage("Inner", body, new Color(0.1f, 0.08f, 0.07f), _roundSprite);
        Stretch(inner.rectTransform, 8);

        //絵の部分(種類ごとの色に光を重ねる)
        var art = CreateImage("Art", body, Color.gray, _roundSprite);
        art.rectTransform.anchorMin = Vector2.zero;
        art.rectTransform.anchorMax = Vector2.one;
        art.rectTransform.offsetMin = new Vector2(12, 74);
        art.rectTransform.offsetMax = new Vector2(-12, -12);
        var shine = CreateImage("Shine", art.transform, new Color(1f, 1f, 0.9f, 0.4f), _knobSprite);
        SetRect(shine.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240, 240));
        var iconText = CreateText("IconText", body, "戦", 120, Color.white, TextAnchor.MiddleCenter);
        SetRect(iconText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 32), new Vector2(220, 170));
        var iconOutline = iconText.gameObject.AddComponent<Outline>();
        iconOutline.effectColor = new Color(0, 0, 0, 0.6f);
        iconOutline.effectDistance = new Vector2(3, -3);
        AddShadow(iconText.gameObject);
        var iconImage = CreateImage("IconImage", body, Color.white);
        SetRect(iconImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 32), new Vector2(170, 170));
        iconImage.preserveAspect = true;

        //名前の札
        var plate = CreateImage("NamePlate", body, new Color(0.05f, 0.04f, 0.04f, 0.95f));
        SetRect(plate.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 42), new Vector2(216, 54));
        var plateOutline = plate.gameObject.AddComponent<Outline>();
        plateOutline.effectColor = new Color(0.78f, 0.62f, 0.32f, 0.9f);
        plateOutline.effectDistance = new Vector2(2, -2);
        var nameText = CreateText("NameText", plate.transform, "一般戦闘", 30, new Color(1f, 0.93f, 0.78f), TextAnchor.MiddleCenter);
        Stretch(nameText.rectTransform);

        var dim = CreateImage("Dim", body, new Color(0, 0, 0, 0), _roundSprite);
        Stretch(dim.rectTransform);
        dim.raycastTarget = false;

        var planned = CreateImage("PlannedMark", body, Gold, _roundSprite);
        SetRect(planned.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(130, 44));
        var plannedText = CreateText("Text", planned.transform, "予定", 28, Ink, TextAnchor.MiddleCenter);
        Stretch(plannedText.rectTransform);

        var so = new SerializedObject(view);
        so.FindProperty("_body").objectReferenceValue = body;
        so.FindProperty("_slot").objectReferenceValue = slot.gameObject;
        so.FindProperty("_glowImage").objectReferenceValue = glow;
        so.FindProperty("_artImage").objectReferenceValue = art;
        so.FindProperty("_iconText").objectReferenceValue = iconText;
        so.FindProperty("_iconImage").objectReferenceValue = iconImage;
        so.FindProperty("_nameText").objectReferenceValue = nameText;
        so.FindProperty("_dimImage").objectReferenceValue = dim;
        so.FindProperty("_plannedMark").objectReferenceValue = planned.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();
        return view;
    }

    /// <summary>
    /// 設定アセットが無ければ、仕様に沿った初期値で作成する
    /// </summary>
    private static StageMapSettings LoadOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<StageMapSettings>(SettingsPath);
        if (settings != null) return settings;

        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
        settings = ScriptableObject.CreateInstance<StageMapSettings>();
        settings.Weights = new List<StageWeight>
        {
            new StageWeight { Type = StageType.Battle, Weight = 45, MinDay = 1 },
            new StageWeight { Type = StageType.Event, Weight = 15, MinDay = 2 },
            new StageWeight { Type = StageType.Rest, Weight = 12, MinDay = 2 },
            new StageWeight { Type = StageType.Shop, Weight = 12, MinDay = 3 },
            new StageWeight { Type = StageType.Elite, Weight = 12, MinDay = 4 },
            new StageWeight { Type = StageType.Treasure, Weight = 4, MinDay = 3 },
        };
        settings.CardInfos = new List<StageCardInfo>
        {
            Info(StageType.Battle, "一般戦闘", "戦", new Color(0.55f, 0.22f, 0.2f),
                "通常の敵と戦う。\n報酬: ゴールド＋カード報酬(3枚から1枚)"),
            Info(StageType.Elite, "精鋭戦闘", "精", new Color(0.62f, 0.1f, 0.32f),
                "強化された敵と戦う。高リスク・高リターン。\n報酬: ゴールド＋カード報酬＋レリック"),
            Info(StageType.Shop, "商人", "商", new Color(0.2f, 0.5f, 0.32f),
                "ゴールドでカードやアイテムを買う。\n同じ品は買うたびに値上がりする。"),
            Info(StageType.Rest, "休息", "休", new Color(0.2f, 0.36f, 0.62f),
                "休む: HPを回復(HPは自然回復しない)\n鍛える: カード1枚を強化\n整理: デッキからカード1枚を削除"),
            Info(StageType.Treasure, "宝箱", "宝", new Color(0.78f, 0.6f, 0.15f),
                "戦闘なしでレリックを獲得(3つから1つ)。\nめったに出ないレアなカード。"),
            Info(StageType.Event, "イベント", "？", new Color(0.45f, 0.25f, 0.62f),
                "ランダムなイベントが起こる。\n選択によっては戦闘に発展することも。"),
            Info(StageType.Boss, "ボス戦", "魔", new Color(0.3f, 0.05f, 0.08f),
                "一定日数ごとに現れる強敵。\n報酬: ゴールド＋カード報酬＋ボスレリック\n倒すと敵が強くなり次の周へ。"),
        };
        AssetDatabase.CreateAsset(settings, SettingsPath);
        return settings;
    }

    private static StageCardInfo Info(StageType type, string name, string icon, Color color, string description)
    {
        return new StageCardInfo { Type = type, Name = name, Icon = icon, Color = color, Description = description };
    }

    /// <summary>
    /// タイトル画面と同じ設定のCanvasを作る
    /// </summary>
    private static GameObject CreateCanvas(Transform parent)
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

    private static void SetArray(SerializedProperty prop, Object[] values)
    {
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.layer = 5;
        obj.transform.SetParent(parent, false);
        return (RectTransform)obj.transform;
    }

    private static Image CreateImage(string name, Transform parent, Color color, Sprite sprite = null)
    {
        var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
        image.color = color;
        image.sprite = sprite;
        if (sprite != null && sprite == _roundSprite) image.type = Image.Type.Sliced;
        return image;
    }

    private static Text CreateText(string name, Transform parent, string text, int size, Color color, TextAnchor anchor)
    {
        var t = CreateRect(name, parent).gameObject.AddComponent<Text>();
        t.font = _font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private static Button CreateButton(string name, Transform parent, string label, int size, Color color, out Text labelText)
    {
        var image = CreateImage(name, parent, color, _roundSprite);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        labelText = CreateText("Text", image.transform, label, size, Color.white, TextAnchor.MiddleCenter);
        Stretch(labelText.rectTransform);
        return button;
    }

    private static void AddShadow(GameObject obj)
    {
        var shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(3, -3);
    }

    private static void Stretch(RectTransform rect, float inset = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
    }
}

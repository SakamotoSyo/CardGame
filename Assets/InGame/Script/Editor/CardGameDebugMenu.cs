//日本語対応
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// 動作確認用のメニュー
/// </summary>
public static class CardGameDebugMenu
{
    /// <summary>
    /// 戦闘中のプレイヤーのHPを0にして、敗北(ランの終了とガチャ石の受け取り)を確かめる
    /// </summary>
    [MenuItem("Tools/CardGame/Debug/Defeat Player")]
    public static void DefeatPlayer()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play中の戦闘でだけ使えます");
            return;
        }
        if (!TryResolve<IPlayerStatus>(out var status)) return;
        status.CurrentHp.Value = 0;
        Debug.Log("プレイヤーのHPを0にしました");
    }

    /// <summary>
    /// Play中のApplicationCompositionRootから登録済みのものを取り出す
    /// </summary>
    public static bool TryResolve<T>(out T value)
    {
        value = default;
        var root = LifetimeScope.Find<ApplicationCompositionRoot>();
        if (root == null || root.Container == null)
        {
            Debug.LogWarning("ApplicationCompositionRootが見つかりません。タイトル画面が出てから使ってください");
            return false;
        }
        value = root.Container.Resolve<T>();
        return true;
    }
}

/// <summary>
/// ガチャ石を増やす・0にするデバッグ用のウィンドウ
/// Play中はゲーム内のWalletRepositoryを通して変える(画面の表示もすぐ変わる)。停止中はセーブファイルを直接書き換える
/// </summary>
public class GachaStoneDebugWindow : EditorWindow
{
    private int _amount = 30;
    /// <summary>Play中に増減している途中ならtrue。連打で重ならないようにする</summary>
    private bool _isChanging;

    private static string FilePath => Path.Combine(Application.persistentDataPath, WalletRepository.FileName);

    [MenuItem("Tools/CardGame/Debug/Gacha Stone")]
    public static void Open()
    {
        GetWindow<GachaStoneDebugWindow>("ガチャ石");
    }

    private void OnInspectorUpdate()
    {
        //Play中の増減や、ゲーム内での消費を表示に反映する
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(Application.isPlaying ? "Play中：ゲーム内の所持数を変えます" : "停止中：セーブファイルを書き換えます", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("所持数", GetCurrent()?.ToString() ?? "(読み込み中)");
        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(_isChanging))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var preset in new[] { 3, 10, 30, 100 })
                {
                    if (GUILayout.Button($"+{preset}")) Add(preset);
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                _amount = Mathf.Max(0, EditorGUILayout.IntField("増やす数", _amount));
                if (GUILayout.Button("増やす", GUILayout.Width(80))) Add(_amount);
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("0にする")) Clear();
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(FilePath, MessageType.None);
        }
    }

    private int? GetCurrent()
    {
        if (Application.isPlaying)
        {
            var root = LifetimeScope.Find<ApplicationCompositionRoot>();
            if (root == null || root.Container == null) return null;
            return root.Container.Resolve<IWalletRepository>().GachaStone;
        }
        return LoadFile().GachaStone;
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        if (Application.isPlaying)
        {
            if (!CardGameDebugMenu.TryResolve<IWalletRepository>(out var wallet)) return;
            Change(wallet.AddStone(amount), $"ガチャ石を{amount}個増やしました").Forget();
            return;
        }
        var data = LoadFile();
        data.GachaStone += amount;
        SaveFile(data);
        Debug.Log($"ガチャ石を{amount}個増やしました(現在{data.GachaStone}個)");
    }

    public void Clear()
    {
        if (Application.isPlaying)
        {
            if (!CardGameDebugMenu.TryResolve<IWalletRepository>(out var wallet)) return;
            Change(wallet.TrySpendStone(wallet.GachaStone), "ガチャ石を0にしました").Forget();
            return;
        }
        SaveFile(new WalletData());
        Debug.Log("ガチャ石を0にしました");
    }

    private async UniTaskVoid Change(UniTask<bool> change, string message)
    {
        _isChanging = true;
        try
        {
            if (await change) Debug.Log(message);
            else Debug.LogWarning("ガチャ石を変更できませんでした");
        }
        finally
        {
            _isChanging = false;
        }
    }

    private static WalletData LoadFile()
    {
        if (!File.Exists(FilePath)) return new WalletData();
        return JsonUtility.FromJson<WalletData>(File.ReadAllText(FilePath)) ?? new WalletData();
    }

    private static void SaveFile(WalletData data)
    {
        File.WriteAllText(FilePath, JsonUtility.ToJson(data));
    }
}

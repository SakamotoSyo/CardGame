//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class ScreenContainer : MonoBehaviour
{
    private static Dictionary<string, ScreenContainer> _screens = new Dictionary<string, ScreenContainer>();

    [SerializeField] private string _containerName;
    [SerializeField] private FadeSystem _fadeAnim;

    private Stack<GameObject> _objStack = new Stack<GameObject>();
    private Transform _transform;

    private void Awake()
    {
        _screens.Add(_containerName, this);
    }

    private void OnDestroy()
    {
        _screens.Remove(_containerName);
    }

    /// <summary>
    /// Screenの名前が一致するものを検索します
    /// </summary>
    /// <param name="screenName"></param>
    public static ScreenContainer Find(string screenName)
    {
        _screens.TryGetValue(screenName, out ScreenContainer container);
        return container;
    }

    public async UniTask Push(string resouseKey)
    {
        var prefab = await AssetLoader.LoadAssetAsync<GameObject>(resouseKey);
        var insObj = Instantiate(prefab, transform);
        var prefabLifeCycle = insObj.GetComponent<IScreenLifeTimeCycle>();

        await prefabLifeCycle.Initialize();
        await prefabLifeCycle.WillPushEnter();
        //ここに画面遷移のAnimationの処理を入れたい
        await _fadeAnim.FadeIn();
        prefabLifeCycle.DidPopEnter();
        _objStack.Push(insObj);
    }

    public async UniTask Pop(int popCount)
    {
        //画面遷移
        if (popCount > _objStack.Count)
        {
            throw new InvalidOperationException("指定の数以上のPopするUIがありません");
        }
        await _fadeAnim.FadeOut();
        Destroy(_objStack.Pop());
    }
}

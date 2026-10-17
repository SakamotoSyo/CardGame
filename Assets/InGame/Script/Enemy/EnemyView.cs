//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UniRx;
using DG.Tweening;
using UnityEngine.Assertions;
using System;

public class EnemyView : ActorViewBase
{
    public IObservable<Unit> AttackAnimEnd => _attackAnimEnd;

    [SerializeField] private Image _charactorImage;
    [SerializeField] private List<AttackImageData> _attackImageData = new List<AttackImageData>();
    [SerializeField] private Transform _attackIconInsPos;
    [SerializeField] private GameObject _attackIconPrefab;

    private readonly Subject<Unit> _attackAnimEnd = new();
    private List<IconScript> _iconScriptList = new();

    /// <summary>
    /// Enemyの行動をIconとして生成する
    /// </summary>
    /// <param name="effectData"></param>
    public void AttackDecisionView(EffectMaster effectData)
    {
        var icon = Instantiate(_attackIconPrefab, _attackIconInsPos).GetComponent<IconScript>();
        _iconScriptList.Add(icon);
        icon.SetEffectPower(effectData.GetTotalPower());
        icon.SetImage(GetIconSprite(effectData.EffectType));
    }

    /// <summary>
    /// 行動を終わった後Animationを読んで該当のアイコンを削除する
    /// </summary>
    /// <param name="effectData"></param>
    public async UniTask DeleteIcon()
    {
        Assert.IsTrue(_iconScriptList.Count != 0);
        await _iconScriptList[0].SelectIcon();
        Destroy(_iconScriptList[0].gameObject);
        _iconScriptList.RemoveAt(0);
        _attackAnimEnd.OnNext(new());
    }

    /// <summary>
    /// 倒されたときに行動アイコンを消してフェードアウトする
    /// </summary>
    public void Die()
    {
        foreach (var icon in _iconScriptList)
        {
            if (icon != null) Destroy(icon.gameObject);
        }
        _iconScriptList.Clear();

        //並びが崩れないよう非表示にはせず、透明にしてタップを受け付けなくする
        if (!TryGetComponent<CanvasGroup>(out var group)) group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.DOFade(0f, 0.4f).SetLink(gameObject);
    }

    public Sprite GetIconSprite(EffectType typeImage)
    {
        return _attackImageData.Find(x => x.EffectType == typeImage).Sprite;
    }

    public void OutLineActive() 
    {
        _charactorImage.color = new Color(_charactorImage.color.r, _charactorImage.color.g
                                          , _charactorImage.color.b, 0.2f);
    }

    public void OutLineNonActive() 
    {
        _charactorImage.color = new Color(_charactorImage.color.r, _charactorImage.color.g
                                  , _charactorImage.color.b, 1f);
    }

    [System.Serializable]
    public class AttackImageData 
    {
        public EffectType EffectType;
        public Sprite Sprite;
    }
}

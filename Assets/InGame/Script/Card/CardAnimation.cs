using UnityEngine;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

[Serializable]
public class CardAnimation 
{
    [SerializeField] private Animator _anim;
    [AnimationParameter]
    [SerializeField] private string _selectParm;
    [SerializeField] private Vector3 _ThrowPos;

    /// <summary>
    /// selectAnimation‚ğÄ¶‚³‚¹‚é‚©‚Ç‚¤‚©
    /// </summary>
    /// <param name="play">Animation‚ğÄ¶‚³‚¹‚é‚©‚Ç‚¤‚©</param>
    public void SelectAnim(bool play) 
    {
        _anim.SetBool(_selectParm, play);
    }

    public async UniTask ThrowAnim(Transform cardTransform, CancellationToken token)
    {
        await DOTween.To(() => cardTransform.localPosition,
             x => cardTransform.localPosition = x,
             _ThrowPos, 0.6f).ToUniTask(cancellationToken: token);
    }
}

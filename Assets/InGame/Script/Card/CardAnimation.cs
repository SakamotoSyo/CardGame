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

    /// <summary>
    /// selectAnimation‚ğÄ¶‚³‚¹‚é‚©‚Ç‚¤‚©
    /// </summary>
    /// <param name="play">Animation‚ğÄ¶‚³‚¹‚é‚©‚Ç‚¤‚©</param>
    public void SelectAnim(bool play) 
    {
        _anim.SetBool(_selectParm, play);
    }
}

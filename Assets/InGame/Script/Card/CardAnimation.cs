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
    /// selectAnimation���Đ������邩�ǂ���
    /// </summary>
    /// <param name="play">Animation���Đ������邩�ǂ���</param>
    public void SelectAnim(bool play) 
    {
        _anim.SetBool(_selectParm, play);
    }
}

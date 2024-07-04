//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IScreenLifeTimeCycle
{
    public UniTask Initialize();
    public UniTask WillPushEnter();
    public UniTask WillPushExit();
    public UniTask WillPopEnter();
    public UniTask WillPopExit();

    public void DidPushEnter();
    public void DidPushExit();

    public void DidPopEnter();
    public void DidPopExit();

}

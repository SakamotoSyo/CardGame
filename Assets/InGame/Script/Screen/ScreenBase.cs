//日本語対応
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBase : MonoBehaviour, IScreenLifeTimeCycle
{
    public virtual void DidPopEnter()
    {
       
    }

    public virtual void DidPopExit()
    {
       
    }

    public virtual void DidPushEnter()
    {
        
    }

    public virtual void DidPushExit()
    {
        
    }

    public virtual UniTask Initialize()
    {
        return UniTask.CompletedTask;
    }

    public virtual UniTask WillPopEnter()
    {
        return UniTask.CompletedTask;
    }

    public virtual UniTask WillPopExit()
    {
        return UniTask.CompletedTask;
    }

    public virtual UniTask WillPushEnter()
    {
        return UniTask.CompletedTask;
    }

    public virtual UniTask WillPushExit()
    {
        return UniTask.CompletedTask;
    }
}

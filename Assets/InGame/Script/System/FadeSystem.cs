//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class FadeSystem : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    public async UniTask FadeIn() 
    {
        transform.SetAsLastSibling();
        var token = this.GetCancellationTokenOnDestroy();
        _anim.Play("FadeIn");
        await UniTask.WaitUntil(() => _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98, 
            PlayerLoopTiming.Update, token);
    }

    public async UniTask FadeOut() 
    {
        transform.SetAsLastSibling();
        var token = this.GetCancellationTokenOnDestroy();
        _anim.Play("FadeOut");
        await UniTask.WaitUntil(() => _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98,
            PlayerLoopTiming.Update, token);
    }
}

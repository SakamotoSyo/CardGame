using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using State = StateMachine<BattleTurnController>.State;

public class BattleStateView : MonoBehaviour
{
    [SerializeField] private Text _debugText;
    [SerializeField] private Text _trunText;
    [SerializeField] private GameObject _stopButtonObj;
    [SerializeField] private Animator _turnAnim;

    public void DebugTurnText(string st)
    {
        //_debugText.text = st;
    }

    public void TurnAnim(State state)
    {
        _turnAnim.Play("Default");
        if (state.ToString() == "PlayerTurnState")
        {
            Debug.Log("プレイヤー");
            _trunText.text = "プレイヤーのターン";
            _turnAnim.SetTrigger("TurnAnim");
        }
        else if (state.ToString() == "EnemyTurnState")
        {
            _trunText.text = "エネミーのターン";
            _turnAnim.SetTrigger("TurnAnim");
        }
    }

    public async UniTask TrunAnimAsync(State state) 
    {
        var token = this.GetCancellationTokenOnDestroy();
        _turnAnim.Play("Default");

        if (state.ToString() == "PlayerTurnState")
        {
            Debug.Log("プレイヤー");
            _trunText.text = "プレイヤーのターン";
            _turnAnim.SetTrigger("TurnAnim");
            await UniTask.WaitUntil(() => _turnAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f, cancellationToken: token);
        }
        else if (state.ToString() == "EnemyTurnState")
        {
            _trunText.text = "エネミーのターン";
            _turnAnim.SetTrigger("TurnAnim");
            await UniTask.WaitUntil(() => _turnAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f, cancellationToken: token);
        }
    }

    /// <summary>
    /// ボタンを停止させるかどうか
    /// </summary>
    /// <param name="b"></param>
    public void StopButton(bool b)
    {
        //_stopButtonObj.SetActive(b);
    }
}

using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;
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

    /// <summary>
    /// ボタンを停止させるかどうか
    /// </summary>
    /// <param name="b"></param>
    public void StopButton(bool b)
    {
        //_stopButtonObj.SetActive(b);
    }
}

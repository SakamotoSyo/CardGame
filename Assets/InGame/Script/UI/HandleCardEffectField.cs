//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandleCardEffectField : MonoBehaviour
{
    [SerializeField]
    private UIOutline _outline;
    private BattleEnviroment _env;
    private CardController _cardCon;
    private CardView _cardView;

    public void SetUp(BattleEnviroment env)
    {
        _env = env;

    }

    private void Update()
    {
        MouseUpEvent();
    }

    private void MouseUpEvent()
    {
        if (Input.GetMouseButtonUp(0) && _cardCon != null)
        {
            var cardData = _cardCon.CardData.Value;
            if (_env.PlayerStatus.UseCost(cardData.Cost))
            {
                _cardCon.CardData.Value.EffectExecute(_env, TargetType.Enemy);
                _cardView.EffectActivated();
            }
            else
            {
                Debug.Log("コストが足りません");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<CardController>(out var cardController))
        {
            _outline.enabled = true;
            _cardCon = cardController;
            _cardView = collision.GetComponent<CardView>();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        EnemyReflectEffect();
    }


    /// <summary>
    /// カードの効果を反映させる敵を選ぶ
    /// </summary>
    private void EnemyReflectEffect()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var saveDistance = 99999f;
        var saveIdx = 0;

        for (int i = 0; i < _env.EnemyViewList.Count; i++)
        {
            var dis = Vector3.SqrMagnitude(mousePos - _env.EnemyViewList[i].gameObject.transform.position);

            if (saveDistance > dis)
            {
                saveDistance = dis;
                saveIdx = i;
            }
        }

        for (int i = 0; i < _env.EnemyViewList.Count; i++)
        {
            if (saveIdx == i)
            {
                _env.EnemyViewList[i].OutLineActive();
            }
            else
            {
                _env.EnemyViewList[i].OutLineNonActive();
            }
        }

        _env.TargetEnemy = saveIdx;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        for (int i = 0; i < _env.EnemyViewList.Count; i++)
        {
            _env.EnemyViewList[i].OutLineNonActive();
        }
        _outline.enabled = false;
        _cardCon = null;
    }
}

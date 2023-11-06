//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleCardEffectField : MonoBehaviour
{
    [SerializeField]
    private UIOutline _outline;
    private CardEnviroment _env;
    private CardController _cardCon;

    public void SetUp(CardEnviroment env) 
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
                _cardCon.CardData.Value.EffectExecute(_env);
                _cardCon.ThrowCard();
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
        }   
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _outline.enabled = false;
        _cardCon = null;
    }
}

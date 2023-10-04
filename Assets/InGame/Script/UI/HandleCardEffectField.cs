//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleCardEffectField : MonoBehaviour
{
    private IPlayerStatus _playerStatus;
    private CardController _cardCon;

    private void SetUp(IPlayerStatus playerStatus) 
    {
        _playerStatus = playerStatus;
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
            if (_playerStatus.UseCost(cardData.Cost)) 
            {

            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (TryGetComponent<CardController>(out var cardController)) 
        {
            _cardCon = cardController;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        
    }

}

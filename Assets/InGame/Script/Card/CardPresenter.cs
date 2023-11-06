using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CardPresenter : MonoBehaviour
{
    [SerializeField] private CardController _cardController;
    [SerializeField] private CardView _cardView;

    void Start()
    {
        _cardController.CardData.Subscribe(value => _cardView.SetInfo(value));
    }
}

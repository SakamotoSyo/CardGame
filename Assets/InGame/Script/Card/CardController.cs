using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UniRx;
using System;
using System.Threading;


public class CardController : MonoBehaviour
{
    public IReactiveProperty<CardData> CardData => _cardData;
    private ReactiveProperty<CardData> _cardData = new ReactiveProperty<CardData>();
    private CancellationToken _cancellationToken;

    private void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    /// <summary>
    /// ƒJ[ƒh‚ğÌ‚Ä‚éˆ—‚Ì—¬‚ê
    /// </summary>
    public void ThrowCard()
    {
        _cardData.Value = null;
    }

    public void SetCardData(CardData card)
    {
        _cardData.Value = card;
    }

    private void OnDestroy()
    {
        _cardData?.Dispose();
    }
}

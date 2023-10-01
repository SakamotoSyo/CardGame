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
    public CardData CardBaseClass => _cardBaseClass.Value;
    public IObservable<CardData> CardBaseClassOb => _cardBaseClass;
    private ReactiveProperty<CardData> _cardBaseClass = new ReactiveProperty<CardData>();
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
        Destroy(gameObject.transform.parent.gameObject);
    }

    public void SetCardBaseClass(CardData card)
    {
        _cardBaseClass.Value = card;
    }
}

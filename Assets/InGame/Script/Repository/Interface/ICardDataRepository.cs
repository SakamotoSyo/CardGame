//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardDataRepository : IRepository
{
    public CardData FindById(int id);
}

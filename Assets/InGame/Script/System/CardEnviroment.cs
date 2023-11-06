//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードの効果発動に使用する環境
/// </summary>
public class CardEnviroment
{
    public CardEnviroment(IPlayerStatus playerStatus) 
    {
        PlayerStatus = playerStatus;
    }

    public IPlayerStatus PlayerStatus;
    public EnemyStatus EnemyStatus;
}

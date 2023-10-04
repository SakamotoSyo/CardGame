//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードの効果発動に使用する環境
/// </summary>
public class Enviroment
{
    public Enviroment(IPlayerStatus playerStatus, EnemyStatus enemyStatus) 
    {
        PlayerStatus = playerStatus;
        EnemyStatus = enemyStatus;
    }

    public IPlayerStatus PlayerStatus;
    public EnemyStatus EnemyStatus;
}

//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnviroment
{
    public BattleEnviroment(IPlayerStatus playerStatus) 
    {
        PlayerStatus = playerStatus;
    }

    public IPlayerStatus PlayerStatus;
    public List<EnemyStatus> EnemyStatusList;
    public List<EnemyView> EnemyViewList = new List<EnemyView>();
    public int TargetEnemy;
}

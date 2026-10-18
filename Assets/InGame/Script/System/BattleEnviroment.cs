//日本語対応
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public bool IsAllEnemyDead => EnemyStatusList.All(e => e.IsDead);

    /// <summary>
    /// 攻撃対象の敵を返す。対象が倒れていたら生きている敵に切り替え、全滅していたらnull
    /// </summary>
    public EnemyStatus GetTargetEnemy()
    {
        if (0 <= TargetEnemy && TargetEnemy < EnemyStatusList.Count && !EnemyStatusList[TargetEnemy].IsDead)
        {
            return EnemyStatusList[TargetEnemy];
        }

        TargetEnemy = EnemyStatusList.FindIndex(e => !e.IsDead);
        return TargetEnemy < 0 ? null : EnemyStatusList[TargetEnemy];
    }
}

//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyDataRepository : IRepository
{
    public EnemyData FindById(int id);
    public EnemyData[] FindHierarchyManifestation(int currentHierarchy);
}

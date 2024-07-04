//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DungeonMapData
{
    public DungeonMapData(int[,] mapData) 
    {
        MapData = mapData;
    }

    public int[,] MapData;
}

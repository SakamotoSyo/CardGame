using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

/// <summary>
/// ステージを選択する際にカードにImageなどを当てはめるクラス
/// </summary>
public class StageMapView : MonoBehaviour
{
    public event Action<StageType, MapPosData> OnCellClick;

    [Tooltip("マップのListの何番目を適応させるか")]
    [SerializeField] private int _mapListSelectNum;
    [Tooltip("ステージのSpriteを格納する")]
    [SerializeField] private Sprite[] _spriteArray;
    [Tooltip("ステージ名を入力する")]
    [SerializeField] private string[] _stageInfo;

    private MapPosData _mapPos;

    public void SetUp(MapPosData mapPosition)
    {
        var cardArray = GetComponentsInChildren<StageSelectCard>();
        _mapPos = mapPosition;
        SetCardView(cardArray);
    }

    /// <summary>
    /// ダンジョンのマップを取得しカードの見た目を設定する
    /// </summary>
    /// <param name="cardArray"></param>
    /// <param name="cardInfoText"></param>
    private void SetCardView(StageSelectCard[] cardArray)
    {
        int[,] dungeonMapData = GenerationDungeonMap.DungeonMapData;
        var splitNum = (cardArray.Length - 1) / 2;

        for (int i = 0; i < cardArray.Length; i++)
        {
            //範囲外じゃなかったら
            if (ArrayOutOfRangeCheck(dungeonMapData, i - (splitNum - _mapPos.X), _mapPos.Y + _mapListSelectNum))
            {
                var surroundingsNum = dungeonMapData[_mapPos.Y + _mapListSelectNum, i - (splitNum - _mapPos.X)];

                cardArray[i].SetView(_spriteArray[surroundingsNum - 1], _stageInfo[surroundingsNum - 1]);
                //一番前のカード列ではない場合continueする
                if (_mapListSelectNum != 0) continue;
                //カードに自分がどのステージのカードなのかの情報を渡す
                cardArray[i].SetStageType((StageType)surroundingsNum);
                cardArray[i].SetCardMapPos(i - (splitNum - _mapPos.X), _mapPos.Y + 1);
                cardArray[i].OnClick += OnCellClick;
            }
        }
    }

    /// <summary>
    /// 配列の範囲外かどうかCheckする
    /// </summary>
    /// <param name="array"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private bool ArrayOutOfRangeCheck(int[,] array, int x, int y) 
    {
        if (array.GetLength(0) <= y || array.GetLength(1) <= x || x < 0 || y < 0) 
        {
            return false;
        }

        if (array[y, x] == 0) 
        {
            return false;
        }

        return true;
    }
}
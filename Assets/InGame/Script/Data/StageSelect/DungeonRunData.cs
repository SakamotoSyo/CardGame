//日本語対応
using System;
using System.Collections.Generic;

/// <summary>
/// 1回の挑戦(ラン)の進行状況。ゲームオーバーで破棄される
/// </summary>
[Serializable]
public sealed class DungeonRunData
{
    /// <summary>今日の日数(1始まり)</summary>
    public int Day = 1;
    /// <summary>前日に選んだカードの列。今日はこの列とその左右から選ぶ</summary>
    public int Column;
    /// <summary>最後に選んだ行動。戦闘画面などが一般戦闘/精鋭/ボスの区別に使う</summary>
    public StageType ActiveStage;
    /// <summary>日ごとのカードの並び。Rows[Day - 1] が今日の列</summary>
    public List<StageMapRow> Rows = new List<StageMapRow>();
    /// <summary>予定ルートとして印を付けたカード</summary>
    public List<StageMapPos> PlannedCells = new List<StageMapPos>();
    /// <summary>今日の戦闘に出発したが、まだ勝利していない。中断しても日が進まないよう、勝つまでは翌日にしない</summary>
    public bool HasPendingStage;
    /// <summary>出発した戦闘の列</summary>
    public int PendingColumn;
    /// <summary>出発した戦闘に勝利した。次にステージ選択画面を開いたときに翌日へ進む</summary>
    public bool IsPendingStageCleared;

    public void SetPendingStage(int column)
    {
        HasPendingStage = true;
        PendingColumn = column;
        IsPendingStageCleared = false;
    }

    /// <summary>戦闘に勝利したときに戦闘側から呼ぶ</summary>
    public void ClearPendingStage()
    {
        if (HasPendingStage) IsPendingStageCleared = true;
    }

    public bool IsPlanned(int day, int column)
    {
        return PlannedCells.Exists(p => p.Day == day && p.Column == column);
    }

    public void TogglePlanned(int day, int column)
    {
        var removed = PlannedCells.RemoveAll(p => p.Day == day && p.Column == column);
        if (removed == 0) PlannedCells.Add(new StageMapPos(day, column));
    }
}

[Serializable]
public sealed class StageMapRow
{
    public StageType[] Cells;

    public StageMapRow(StageType[] cells)
    {
        Cells = cells;
    }
}

[Serializable]
public struct StageMapPos
{
    public int Day;
    public int Column;

    public StageMapPos(int day, int column)
    {
        Day = day;
        Column = column;
    }
}

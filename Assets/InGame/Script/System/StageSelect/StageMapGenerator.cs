//日本語対応
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 日ごとのカードの並びを生成し、選べるカードを判定する
/// </summary>
public sealed class StageMapGenerator
{
    /// <summary>今日を含めて何日分を画面に並べるか(一番奥の日は画面外にあり、日送りのスクロールで見えてくる)</summary>
    public const int VisibleDays = 4;

    private readonly StageMapSettings _settings;

    public StageMapGenerator(StageMapSettings settings)
    {
        _settings = settings;
    }

    public int Width => _settings.Width;

    public DungeonRunData CreateNewRun()
    {
        var run = new DungeonRunData
        {
            RunId = System.Guid.NewGuid().ToString("N"),
            Day = 1,
            Column = Mathf.Clamp(_settings.StartColumn, 0, _settings.Width - 1),
        };
        EnsureRows(run);
        return run;
    }

    /// <summary>
    /// 表示する日数分のマップが生成されていなければ追加で生成する
    /// </summary>
    public void EnsureRows(DungeonRunData run)
    {
        while (run.Rows.Count < run.Day + VisibleDays - 1)
        {
            run.Rows.Add(GenerateRow(run.Rows.Count + 1));
        }
    }

    /// <summary>
    /// 保存されていたランが今の設定で表示できるか(設定の変更やセーブの破損で横幅や位置が合わなくなっていないか)
    /// </summary>
    public bool IsValid(DungeonRunData run)
    {
        if (run.Day < 1 || run.Rows == null || run.PlannedCells == null) return false;
        if (run.Column < 0 || run.Column >= Width) return false;
        if (run.HasPendingStage && (run.PendingColumn < 0 || run.PendingColumn >= Width)) return false;
        foreach (var row in run.Rows)
        {
            if (row == null || row.Cells == null || row.Cells.Length != Width) return false;
        }
        return true;
    }

    public StageType GetCell(DungeonRunData run, int day, int column)
    {
        return run.Rows[day - 1].Cells[column];
    }

    /// <summary>
    /// 今日選べるカードか(前日に選んだ列の真上とその左右。戦闘の途中で中断していたらその戦闘だけ)
    /// </summary>
    public bool IsSelectable(DungeonRunData run, int column)
    {
        if (run.HasPendingStage) return column == run.PendingColumn;
        return Mathf.Abs(column - run.Column) <= 1;
    }

    /// <summary>
    /// 今日fromColumnを選んだとき、targetDayのtargetColumnへたどり着けるか
    /// </summary>
    public bool IsReachable(DungeonRunData run, int fromColumn, int targetDay, int targetColumn)
    {
        var days = targetDay - run.Day;
        return Mathf.Abs(targetColumn - fromColumn) <= days;
    }

    /// <summary>
    /// 今日のカードを選んで翌日へ進める
    /// </summary>
    public StageType Advance(DungeonRunData run, int column)
    {
        var type = GetCell(run, run.Day, column);
        run.PlannedCells.RemoveAll(p => p.Day <= run.Day);
        run.HasPendingStage = false;
        run.IsPendingStageCleared = false;
        run.Column = column;
        run.Day++;
        EnsureRows(run);
        return type;
    }

    /// <summary>何周目か(ボスを倒すごとに増える)</summary>
    public int GetCycle(int day)
    {
        return (day - 1) / _settings.BossInterval + 1;
    }

    /// <summary>次のボス戦まであと何日か(今日がボス戦なら0)</summary>
    public int GetDaysUntilBoss(int day)
    {
        return (_settings.BossInterval - day % _settings.BossInterval) % _settings.BossInterval;
    }

    private StageMapRow GenerateRow(int day)
    {
        var cells = new StageType[_settings.Width];
        var daysUntilBoss = GetDaysUntilBoss(day);

        for (int x = 0; x < cells.Length; x++)
        {
            if (daysUntilBoss == 0)
            {
                cells[x] = StageType.Boss;
            }
            else if (daysUntilBoss == 1)
            {
                //ボス戦の前日は回復・強化の準備日にする
                cells[x] = StageType.Rest;
            }
            else if (day == 1)
            {
                //初日は通常戦闘から始める
                cells[x] = StageType.Battle;
            }
            else
            {
                cells[x] = Draw(day);
            }
        }
        return new StageMapRow(cells);
    }

    private StageType Draw(int day)
    {
        var candidates = new List<StageWeight>();
        float total = 0;
        foreach (var weight in _settings.Weights)
        {
            if (weight.MinDay > day || weight.Weight <= 0) continue;
            candidates.Add(weight);
            total += weight.Weight;
        }
        if (candidates.Count == 0) return StageType.Battle;

        var random = Random.value * total;
        foreach (var candidate in candidates)
        {
            if (random < candidate.Weight) return candidate.Type;
            random -= candidate.Weight;
        }
        return candidates[candidates.Count - 1].Type;
    }
}

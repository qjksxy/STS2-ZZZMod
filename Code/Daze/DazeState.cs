using STS2RitsuLib.Utils;

namespace ZZZMod.Code.Daze;

/// <summary>
///     单个怪物的失衡状态（正向计数）。
///     初始为 0，每次攻击 +N，满后下回合进入失衡（受伤 +50%），
///     失衡持续一回合后重置为 0。
///
///     时间线：
///     T   玩家攻击 → 失衡值 +N
///     T   累积至满 → PendingDaze = true
///     T+1 怪物回合开始 → 进入失衡状态（IsDazed = true）
///     T+2 怪物回合开始 → 恢复，重置为 0
/// </summary>
public sealed class DazeState
{
    public int CurrentValue { get; set; }
    public int MaxValue { get; set; }
    public bool PendingDaze { get; set; }
    public bool IsDazed { get; set; }

    public float FillRatio => MaxValue > 0
        ? Math.Clamp((float)CurrentValue / MaxValue, 0f, 1f)
        : 0f;

    public bool IsFull => MaxValue > 0 && CurrentValue >= MaxValue;

    /// <summary>
    ///     累积失衡值。返回 true 表示刚好达到上限。
    /// </summary>
    public bool AddDaze(int amount)
    {
        if (amount <= 0 || IsFull || PendingDaze || IsDazed) return false;
        CurrentValue = Math.Min(CurrentValue + amount, MaxValue);
        if (IsFull)
        {
            PendingDaze = true;
            return true;
        }
        return false;
    }

    /// <summary>
    ///     怪物回合开始时调用，推进状态机。
    /// </summary>
    public DazeTurnAction TickTurnStart()
    {
        if (PendingDaze)
        {
            IsDazed = true;
            PendingDaze = false;
            return DazeTurnAction.ApplyDebuff;
        }

        if (IsDazed)
        {
            IsDazed = false;
            CurrentValue = 0;
            return DazeTurnAction.RemoveDebuff;
        }

        return DazeTurnAction.None;
    }
}

public enum DazeTurnAction
{
    None,
    ApplyDebuff,
    RemoveDebuff,
}

/// <summary>
///     全局失衡状态存储。
/// </summary>
public static class DazeStore
{
    private static readonly AttachedState<MegaCrit.Sts2.Core.Entities.Creatures.Creature, DazeState> States =
        new(() => new());

    public static DazeState Get(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature)
    {
        var state = States.GetOrCreate(creature);
        if (state.MaxValue <= 0)
            state.MaxValue = DazeSystem.CalcMaxDaze(creature);
        return state;
    }

    public static bool TryGet(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature, out DazeState state) =>
        States.TryGetValue(creature, out state!);

    public static void ClearAll() => States.Clear();
}

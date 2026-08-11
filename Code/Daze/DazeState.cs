using STS2RitsuLib.Utils;

namespace ZZZMod.Code.Daze;

/// <summary>
///     单个怪物的失衡状态数据（倒计时模式）。
///     初始为满值，每次攻击 -1，归 0 后下一回合触发失衡。
/// </summary>
public sealed class DazeState
{
    public int CurrentValue { get; set; }
    public int MaxValue { get; set; } = DazeSystem.DefaultMaxDaze;
    public bool IsDazed { get; set; }
    public bool PendingDaze { get; set; }

    public float FillRatio => MaxValue > 0
        ? Math.Clamp((float)CurrentValue / MaxValue, 0f, 1f)
        : 0f;

    public bool IsEmpty => CurrentValue <= 0;

    /// <summary>
    ///     初始化为满值（战斗开始时调用）。
    /// </summary>
    public void InitToMax()
    {
        CurrentValue = MaxValue;
        IsDazed = false;
        PendingDaze = false;
    }

    /// <summary>
    ///     减少失衡值（每次攻击 -1）。返回 true 表示刚好归零。
    /// </summary>
    public bool ReduceDaze(int amount)
    {
        if (amount <= 0 || IsEmpty) return false;
        CurrentValue = Math.Max(CurrentValue - amount, 0);
        if (IsEmpty && !PendingDaze && !IsDazed)
        {
            PendingDaze = true;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        CurrentValue = MaxValue;
        IsDazed = false;
        PendingDaze = false;
    }

    /// <summary>
    ///     回合开始时调用：PendingDaze → IsDazed，然后重置为满值。
    /// </summary>
    public void TickTurnStart()
    {
        if (PendingDaze)
        {
            IsDazed = true;
            PendingDaze = false;
            CurrentValue = MaxValue;
        }
    }
}

/// <summary>
///     全局失衡状态存储，按 Creature 实例附加（ConditionalWeakTable，自动 GC）。
/// </summary>
public static class DazeStore
{
    private static readonly AttachedState<MegaCrit.Sts2.Core.Entities.Creatures.Creature, DazeState> States =
        new(() => new());

    public static DazeState Get(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature)
    {
        var state = States.GetOrCreate(creature);
        // 首次访问时自动初始化为满值
        if (state.CurrentValue == 0 && state.MaxValue > 0 && !state.IsDazed && !state.PendingDaze)
            state.InitToMax();
        return state;
    }

    public static bool TryGet(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature, out DazeState state) =>
        States.TryGetValue(creature, out state!);

    public static void ClearAll() => States.Clear();
}

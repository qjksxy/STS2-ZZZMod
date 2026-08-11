using STS2RitsuLib.Utils;

namespace ZZZMod.Code.Daze;

/// <summary>
///     单个怪物的失衡状态。
///     倒计时模式：初始满值，每次攻击 -N，归 0 后下回合进入失衡（受伤 +50%），
///     失衡持续一回合后自动恢复满值。
///
///     时间线：
///     T 玩家攻击 → 归 0（PendingDaze = true）
///     T+1 怪物回合开始 → 进入失衡状态（IsDazed = true, DebuffApplied = true）
///     T+1 玩家回合 → 怪物受伤 +50%
///     T+2 怪物回合开始 → 恢复（移除 debuff，重置满值）
/// </summary>
public sealed class DazeState
{
    public int CurrentValue { get; set; }
    public int MaxValue { get; set; }
    public bool PendingDaze { get; set; }
    public bool IsDazed { get; set; }
    public bool DebuffApplied { get; set; }

    public float FillRatio => MaxValue > 0
        ? Math.Clamp((float)CurrentValue / MaxValue, 0f, 1f)
        : 0f;

    public bool IsEmpty => CurrentValue <= 0;

    public void InitToMax()
    {
        CurrentValue = MaxValue;
        PendingDaze = false;
        IsDazed = false;
        DebuffApplied = false;
    }

    /// <summary>
    ///     减少失衡值。返回 true 表示刚好归零。
    /// </summary>
    public bool ReduceDaze(int amount)
    {
        if (amount <= 0 || IsEmpty || PendingDaze || IsDazed) return false;
        CurrentValue = Math.Max(CurrentValue - amount, 0);
        if (IsEmpty)
        {
            PendingDaze = true;
            return true;
        }
        return false;
    }

    /// <summary>
    ///     怪物回合开始时调用，推进状态机。
    ///     返回当前应执行的动作。
    /// </summary>
    public DazeTurnAction TickTurnStart()
    {
        if (PendingDaze && !DebuffApplied)
        {
            // T+1：进入失衡状态，施加 debuff
            IsDazed = true;
            PendingDaze = false;
            DebuffApplied = true;
            return DazeTurnAction.ApplyDebuff;
        }

        if (IsDazed && DebuffApplied)
        {
            // T+2：恢复，移除 debuff，重置满值
            IsDazed = false;
            DebuffApplied = false;
            CurrentValue = MaxValue;
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
        if (state.CurrentValue <= 0 && !state.PendingDaze && !state.IsDazed)
            state.InitToMax();
        return state;
    }

    public static bool TryGet(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature, out DazeState state) =>
        States.TryGetValue(creature, out state!);

    public static void ClearAll() => States.Clear();
}

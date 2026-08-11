using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using STS2RitsuLib;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡系统核心入口。负责生命周期订阅和补丁注册。
/// </summary>
public static class DazeSystem
{
    public const int DefaultMaxDaze = 6;

    public static void Init()
    {
        // 生命周期：战斗开始/结束时清空失衡状态
        RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(_ => DazeStore.ClearAll());
        RitsuLibFramework.SubscribeLifecycle<CombatEndedEvent>(_ => DazeStore.ClearAll());

        // 生命周期：敌方回合开始时触发失衡效果
        RitsuLibFramework.SubscribeLifecycle<SideTurnStartingEvent>(OnSideTurnStarting);
    }

    private static async void OnSideTurnStarting(SideTurnStartingEvent evt)
    {
        if (evt.Side != CombatSide.Enemy) return;
        var combatState = evt.CombatState;
        if (combatState == null) return;

        foreach (var creature in combatState.Creatures)
        {
            if (creature.Side != CombatSide.Enemy) continue;
            if (!creature.IsAlive) continue;
            if (!DazeStore.TryGet(creature, out var daze)) continue;

            daze.TickTurnStart();

            if (daze.IsDazed)
            {
                if (creature.Monster?.NextMove != null)
                    await CreatureCmd.Stun(creature, creature.Monster.NextMove.Id);
                daze.IsDazed = false;
                Entry.Logger.Debug($"[Daze] {creature} 进入失衡状态");
            }
        }
    }
}

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡系统核心入口。
/// </summary>
public static class DazeSystem
{
    /// <summary>
    ///     计算怪物的失衡条上限：maxHP / 8，最低 4。
    /// </summary>
    public static int CalcMaxDaze(Creature creature)
    {
        return Math.Max(4, creature.MaxHp / 12);
    }

    public static void Init()
    {
        RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(_ => DazeStore.ClearAll());
        RitsuLibFramework.SubscribeLifecycle<CombatEndedEvent>(_ => DazeStore.ClearAll());
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

            var action = daze.TickTurnStart();

            switch (action)
            {
                case DazeTurnAction.ApplyDebuff:
                    await PowerCmd.Apply<DazePower>(new ThrowingPlayerChoiceContext(), creature, 1, creature, null);
                    Entry.Logger.Debug($"[Daze] {creature} 进入失衡状态（受伤 +50%）");
                    break;

                case DazeTurnAction.RemoveDebuff:
                    var existing = creature.GetPower<DazePower>();
                    if (existing != null)
                        await PowerCmd.Remove(existing);
                    Entry.Logger.Debug($"[Daze] {creature} 恢复，失衡条重置");
                    break;
            }
        }
    }
}

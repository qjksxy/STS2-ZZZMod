using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡系统核心入口。
///     基础失衡易伤通过 HashSet 跟踪（不受人工制品影响）。
///     额外失衡易伤（DazeVulnerablePower）通过 Power 系统（可被人工作品抵消）。
/// </summary>
public static class DazeSystem
{
    // ── 配置常量 ──
    /// <summary>失衡值下限，确保低血量敌人也有足够的失衡容量</summary>
    internal const int MinMaxDaze = 4;

    /// <summary>失衡值计算系数：MaxHp / DazeCoefficient = 失衡上限</summary>
    internal const int DazeCoefficient = 12;

    /// <summary>失衡状态伤害加成倍率（基础）</summary>
    internal const decimal DazeDamageMultiplier = 1.5m;

    /// <summary>失衡状态伤害加成倍率（有失衡易伤 Power 时）</summary>
    internal const decimal DazeVulnerableDamageMultiplier = 2.0m;

    internal static readonly HashSet<Creature> DazedCreatures = new();

    public static int CalcMaxDaze(Creature creature)
    {
        return Math.Max(MinMaxDaze, creature.MaxHp / DazeCoefficient);
    }

    public static void Init()
    {
        RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(_ =>
        {
            DazeStore.ClearAll();
            DazedCreatures.Clear();
        });

        RitsuLibFramework.SubscribeLifecycle<CombatEndedEvent>(_ =>
        {
            DazeStore.ClearAll();
            DazedCreatures.Clear();
        });

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
                    DazedCreatures.Add(creature);
                    Entry.Logger.Debug($"[Daze] {creature} 进入失衡状态（受伤 +50%）");
                    break;

                case DazeTurnAction.RemoveDebuff:
                    DazedCreatures.Remove(creature);
                    // 移除醉花月云转施加的额外失衡易伤（可被人工作品抵消的部分）
                    var vuln = creature.GetPower<DazeVulnerablePower>();
                    if (vuln != null)
                        await PowerCmd.Remove(vuln);
                    Entry.Logger.Debug($"[Daze] {creature} 恢复，失衡条重置");
                    break;
            }
        }
    }

    public static bool IsDazed(Creature creature) => DazedCreatures.Contains(creature);
}

using MegaCrit.Sts2.Core.Combat;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ZZZMod.Code.Daze;

/// <summary>
///     全局攻击命中钩子：玩家攻击怪物时累积失衡值。
///     通过 [RegisterSingleton] 注册为战斗单例模型，
///     RitsuLib 的 ModelHookListenerDispatcher 会自动发现。
/// </summary>
[RegisterSingleton]
public sealed class DazeHitListener : HookedSingletonModel, IAttackHitHookListener
{
    public DazeHitListener() : base(HookType.Combat) { }

    public Task AfterAttackHit(AttackHitContext context)
    {
        foreach (var result in context.Results)
        {
            var target = result.Receiver;
            if (target == null) continue;
            if (target.Side != CombatSide.Enemy) continue;
            if (!target.IsAlive) continue;

            // 每次攻击命中固定减少 1 点失衡值（倒计时）
            DazeStore.Get(target).ReduceDaze(1);
        }

        return Task.CompletedTask;
    }
}

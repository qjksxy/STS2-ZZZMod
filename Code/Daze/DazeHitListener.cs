using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ZZZMod.Code.Daze;

/// <summary>
///     攻击命中钩子：每次攻击增加怪物失衡值。
///     默认 +1，特定卡牌可通过 IDazeCardSource.DazeAmount 自定义。
/// </summary>
[RegisterSingleton]
public sealed class DazeHitListener : HookedSingletonModel, IAttackHitHookListener
{
    public DazeHitListener() : base(HookType.Combat) { }

    public Task AfterAttackHit(AttackHitContext context)
    {
        var dazeAmount = 1;
        if (context.CardSource is IDazeCardSource dazeCard)
            dazeAmount = dazeCard.DazeAmount;

        foreach (var result in context.Results)
        {
            var target = result.Receiver;
            if (target == null) continue;
            if (target.Side != CombatSide.Enemy) continue;
            if (!target.IsAlive) continue;

            DazeStore.Get(target).AddDaze(dazeAmount);
        }

        return Task.CompletedTask;
    }
}

public interface IDazeCardSource
{
    int DazeAmount { get; }
}

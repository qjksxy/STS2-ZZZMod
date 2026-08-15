using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.AttackHits;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using ZZZMod.Code.Decibel;

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

            var daze = DazeStore.Get(target);
            var reachedMax = daze.AddDaze(dazeAmount);

            // 失衡条满时获得 Decibel（失衡上限的一半）
            if (reachedMax)
                DecibelSystem.GainDecibel(daze.MaxValue / 2);
        }

        return Task.CompletedTask;
    }
}

public interface IDazeCardSource
{
    int DazeAmount { get; }
}

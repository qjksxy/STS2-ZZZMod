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
    // ── 配置常量 ──
    /// <summary>默认每次攻击命中的失衡值增量</summary>
    internal const int DefaultDazePerHit = 1;

    /// <summary>失衡条满时获得的喧响值比例（失衡上限的 1/2）</summary>
    internal const int DecibelGainOnDazeFullDivisor = 2;

    public DazeHitListener() : base(HookType.Combat) { }

    public Task AfterAttackHit(AttackHitContext context)
    {
        var dazeAmount = DefaultDazePerHit;
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
                DecibelSystem.GainDecibel(daze.MaxValue / DecibelGainOnDazeFullDivisor);
        }

        return Task.CompletedTask;
    }
}

public interface IDazeCardSource
{
    int DazeAmount { get; }
}

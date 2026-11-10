using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Daze;

namespace ZZZMod.Code.Powers;

/// <summary>
///     不投降就捣乱 —— 由「不投降就捣乱」施加给目标的标记能力。
///     目标每次进入失衡状态后，失去1层力量。不可叠加，不随回合衰减。
/// </summary>
[RegisterPower]
public sealed class SurrenderOrSufferTheMischiefPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/surrender_or_suffer_the_mischief.png",
        BigIconPath: "res://ZZZMod/images/powers/surrender_or_suffer_the_mischief_big.png"
    );

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 只在敌人回合开始时检查
        if (side != CombatSide.Enemy) return;
        if (!participants.Contains(Owner)) return;
        if (!Owner.IsAlive) return;

        // 目标进入失衡状态（IsDazed，或失衡值已满即将进入 PendingDaze）时触发一次。
        // 两种情况在敌人回合开始互斥且正好覆盖一次触发，与失衡状态机的执行顺序无关。
        if (!DazeStore.TryGet(Owner, out var daze)) return;
        if (!daze.IsDazed && !daze.PendingDaze) return;

        // 失去1层力量：即使目标当前没有力量，也直接扣减为负值，与「彻骨磔罚」保持一致。
        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(), Owner, -1, Owner, null);
    }
}

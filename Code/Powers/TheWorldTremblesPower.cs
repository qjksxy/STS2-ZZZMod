using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     「撼天动地」配套能力：本回合你受到的每次伤害减少 {Amount} 点。
///     「流失生命」（Unpowered）不受任何伤害减免影响，不触发本能力。
///     持续到玩家下回合开始（覆盖敌方回合的攻击）。
/// </summary>
[RegisterPower]
public sealed class TheWorldTremblesPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/the_world_trembles.png",
        BigIconPath: "res://ZZZMod/images/powers/the_world_trembles_big.png"
    );

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner) return 0m;
        if (Amount <= 0) return 0m;
        // 「流失生命」不受任何伤害减免影响
        if (props.HasFlag(ValueProp.Unpowered)) return 0m;
        // 每次伤害减少 Amount 点（不低于0）
        return -Math.Min(amount, Amount);
    }

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 本回合限定：玩家下回合开始时移除（覆盖敌方回合的攻击）
        if (side != Owner.Side || !participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}

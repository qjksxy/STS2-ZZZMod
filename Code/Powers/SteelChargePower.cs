using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     钢能 —— 由「铁萼雨幕」施加，「落霞风暴」每回合开始时叠加1层。
///     打出攻击牌时，每有10点格挡，造成的伤害提升1点（伤害加成随层数叠算，不随回合衰减）。
/// </summary>
[RegisterPower]
public sealed class SteelChargePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/steel_charge.png",
        BigIconPath: "res://ZZZMod/images/powers/steel_charge_big.png"
    );

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        // 只修正玩家自己打出的攻击伤害
        if (Owner != dealer) return 0m;
        if (!props.IsPoweredAttack()) return 0m;

        // 每有10点格挡提升1点伤害，加成随钢能层数叠算
        return (Owner.Block / 10) * Amount;
    }
}

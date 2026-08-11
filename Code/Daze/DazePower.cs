using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡状态 —— 怪物受到的伤害提升 50%（类似易伤）。
///     由 DazeSystem 在怪物回合开始时施加/移除。
/// </summary>
[RegisterPower]
public sealed class DazePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/daze.png",
        BigIconPath: "res://ZZZMod/images/powers/daze_big.png"
    );

    // 受伤 +50%，与 VulnerablePower 机制一致
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        return target == Owner ? 1.5m : 1m;
    }
}

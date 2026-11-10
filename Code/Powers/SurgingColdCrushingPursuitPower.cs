using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Cards.Common;

namespace ZZZMod.Code.Powers;

/// <summary>
///     「溯寒追碾」配套临时能力：本回合内临时获得力量（原版力量，回合结束自动移除）。
///     基于 RitsuLib 的 ModTemporaryAppliedPowerTemplate 实现。
/// </summary>
[RegisterPower]
public sealed class SurgingColdCrushingPursuitPower
    : ModTemporaryAppliedPowerTemplate<SurgingColdCrushingPursuit, StrengthPower>
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/surging_cold_crushing_pursuit.png",
        BigIconPath: "res://ZZZMod/images/powers/surging_cold_crushing_pursuit_big.png"
    );
}

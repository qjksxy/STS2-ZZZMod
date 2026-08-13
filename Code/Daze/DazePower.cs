using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡状态 —— 纯视觉标记（图标显示）。
///     实际伤害修正由 DazeDamageModifierPatch 通过 Harmony 实现，不经过 Power 系统，
///     因此不受人工制品（抵消负面效果）影响。
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
}

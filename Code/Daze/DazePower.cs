using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡状态 —— 怪物失衡值满后施加的 debuff 标记。
///     仅作为视觉标记（图标显示），实际眩晕效果由 DazeSystem 在 SideTurnStartingEvent 中处理。
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

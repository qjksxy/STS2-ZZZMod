using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡易伤 —— 由醉花月云转触发的视觉标记。
///     使用标准 Power 系统，可被人工制品正常抵消。
///     怪物失衡恢复时由 DazeSystem 自动移除。
///     实际伤害倍率由 DazeDamageModifier 统一处理（检测此 Power 存在性）。
/// </summary>
[RegisterPower]
public sealed class DazeVulnerablePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/daze_vulnerable.png",
        BigIconPath: "res://ZZZMod/images/powers/daze_vulnerable_big.png"
    );
}

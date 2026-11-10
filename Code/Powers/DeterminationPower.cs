using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     决意 —— 由「最高马力星光」「摇曳步伐」施加并由其消耗。
///     纯计数层数，本身无任何效果。
/// </summary>
[RegisterPower]
public sealed class DeterminationPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/determination.png",
        BigIconPath: "res://ZZZMod/images/powers/determination_big.png"
    );
}

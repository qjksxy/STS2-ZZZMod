using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     「应援能量」（Fandom Power）：计数层数，无效果，由卡牌效果消耗
///     （Cards.csv 行 64 备注原文：应援能量（Fandom Power）：计数层数，无效果，由卡牌效果消耗。）。
/// </summary>
[RegisterPower]
public sealed class FandomPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/fandom_power.png",
        BigIconPath: "res://ZZZMod/images/powers/fandom_power_big.png"
    );
}

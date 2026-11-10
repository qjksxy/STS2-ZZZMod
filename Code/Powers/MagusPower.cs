using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     鬼火 —— 由「与火共舞」施加。
///     触发连携时，额外打出 1 张连携牌。
///     效果本体在 ChainSystem 中实现（同一张出牌最多额外触发 1 张，防无限循环）；
///     不可叠加。
/// </summary>
[RegisterPower]
public sealed class MagusPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/magus.png",
        BigIconPath: "res://ZZZMod/images/powers/magus_big.png"
    );
}

using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     剑仪 —— 由「极光突刺」施加。
///     至多累积3层。打出「极光突刺」时，依据不同层数获得增益：
///     1层：降低目标2点失衡上限（至多降低至4）；2层：抽1张牌；3层：回复1点能量。
///     层数上限与增益触发均在「极光突刺」的 OnPlay 中处理，本能力仅作层数显示。
/// </summary>
[RegisterPower]
public sealed class BladeEtiquettePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/blade_etiquette.png",
        BigIconPath: "res://ZZZMod/images/powers/blade_etiquette_big.png"
    );
}

using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Pools;

public class ZZZRelicPool : TypeListRelicPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://ZZZMod/images/character/energy.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://ZZZMod/images/character/energy_big.png";

    public override string EnergyColorName => "zzz_black";
}
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Pools;

public class ZZZPotionPool : TypeListPotionPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://ZZZMod/images/energy_test.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://ZZZMod/images/energy_test_big.png";

    public override string EnergyColorName => "Black";
}
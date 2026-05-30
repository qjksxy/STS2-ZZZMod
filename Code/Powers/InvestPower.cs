using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     投资 —— 每回合结束获得1层力量和1层虚弱。
///     可堆叠，层数越高每回合获得的力量和虚弱越多。
/// </summary>
[RegisterPower]
public sealed class InvestPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile =>
        new(
            "res://ZZZMod/images/powers/invest_power.png",
            "res://ZZZMod/images/powers/invest_power_big.png"
        );

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side) return;
        if (Amount <= 0) return;

        await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
        await PowerCmd.Apply<WeakPower>(Owner, Amount, Owner, null);
    }
}

using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     割弦 —— 由「割弦」卡牌施加。
///     本回合受到攻击时，给予攻击者1层易伤。玩家回合开始时移除。
/// </summary>
[RegisterPower]
public sealed class RazorWirePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/razor_wire.png",
        BigIconPath: "res://ZZZMod/images/powers/razor_wire_big.png"
    );

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 只在拥有者受到攻击时触发
        if (target != Owner) return;
        if (dealer == null || dealer.Side != CombatSide.Enemy) return;
        if (!dealer.IsAlive) return;

        // 给予攻击者1层易伤
        await PowerCmd.Apply<VulnerablePower>(choiceContext, dealer, 1, Owner, null);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 玩家回合开始时移除（"本回合"效果）
        if (player.Creature != Owner) return;
        await PowerCmd.Remove(this);
    }
}

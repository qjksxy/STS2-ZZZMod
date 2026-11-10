using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     落霞风暴 —— 由「落霞风暴」施加的配套被动能力。
///     每回合开始时，获得1层「钢能」，并额外抽1张攻击牌（从抽牌堆直接拉出第一张攻击牌）。
/// </summary>
[RegisterPower]
public sealed class TempestOfFrostyPetalsPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/tempest_of_frosty_petals.png",
        BigIconPath: "res://ZZZMod/images/powers/tempest_of_frosty_petals_big.png"
    );

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 只在玩家回合开始时触发
        if (side != Owner.Side || !participants.Contains(Owner)) return;
        if (CombatManager.Instance.IsOverOrEnding) return;

        // 获得1层「钢能」
        await PowerCmd.Apply<SteelChargePower>(
            new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);

        // 手牌已满则不抽
        var hand = PileType.Hand.GetPile(Owner.Player);
        if (hand.Cards.Count >= CardPile.MaxCardsInHand) return;

        // 从抽牌堆中直接拉出第一张攻击牌加入手牌（其余牌留在抽牌堆）
        var drawPile = PileType.Draw.GetPile(Owner.Player);
        var attack = drawPile.Cards.FirstOrDefault(c => c.Type == CardType.Attack);
        if (attack == null) return;

        await CardPileCmd.Add(attack, PileType.Hand);
    }
}

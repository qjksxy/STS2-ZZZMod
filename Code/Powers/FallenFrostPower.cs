using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Cards.Basic;

namespace ZZZMod.Code.Powers;

/// <summary>
///     落霜 —— 由「飞雪」和「名残雪」施加。
///     纯计数能力：累积到6层时消耗6层，将一张「霜月」加入手牌。
///     技能牌+1层、施加负面+2层的被动由 LingeringSnowPower 实现。
/// </summary>
[RegisterPower]
public sealed class FallenFrostPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/fallen_frost.png",
        BigIconPath: "res://ZZZMod/images/powers/fallen_frost_big.png"
    );

    /// <summary>
    ///     外部增加层数后调用，检查是否达到6层阈值并消耗生成霜月。
    /// </summary>
    public async Task CheckAndConsume(PlayerChoiceContext choiceContext)
    {
        if (Amount >= 6)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -6, Owner, null);

            var combatState = CombatState;
            if (combatState != null && !CombatManager.Instance.IsOverOrEnding)
            {
                var card = combatState.CreateCard<Shimotsuki>(Owner.Player);
                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner.Player);
            }
        }
    }
}

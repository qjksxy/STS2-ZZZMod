using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     残心 —— 2费攻击，造成 {Damage} 点伤害，施加3层电壶。
///     喧响：抽目标电壶层数张牌。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class Zanshin() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true
), IDecibelCardSource
{
    /// <summary>喧响消耗，默认 20。</summary>
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost),
        HoverTipFactory.FromPower<ElectroQuiversPower>()
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 造成伤害
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 施加3层电壶
        await ApplyPowerTo<ElectroQuiversPower>(choiceContext, target, 3, cardPlay.Card);

        // 喧响：抽目标电壶层数张牌
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            var eq = target.GetPower<ElectroQuiversPower>();
            int stacks = eq?.Amount ?? 0;
            if (stacks > 0)
                await CardPileCmd.Draw(choiceContext, stacks, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);   // 强伤列（CSV 强费列为空，不降费用）
    }
}

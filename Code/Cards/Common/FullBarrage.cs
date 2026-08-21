using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     全弹连射 —— 1费攻击，消耗，造成 {Damage} 点伤害。
///     若此牌费用不为0，则将一张费用-1的此牌复制加入手牌。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class FullBarrage() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true
)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 若此牌费用不为0，则将一张费用-1的此牌复制加入手牌
        var currentCost = cardPlay.Card.EnergyCost.GetWithModifiers(CostModifiers.All);
        if (currentCost > 0)
        {
            var combatState = CombatState;
            if (combatState != null && !CombatManager.Instance.IsOverOrEnding)
            {
                var copy = combatState.CreateCard<FullBarrage>(Owner);
                copy.EnergyCost.SetThisCombat(currentCost - 1);
                await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}

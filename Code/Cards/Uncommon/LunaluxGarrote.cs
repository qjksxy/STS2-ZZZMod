using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     月辉丝 —— 2费攻击，消耗弃牌堆中至多 {Magic} 张牌，然后对目标造成消耗牌数×5的伤害。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class LunaluxGarrote() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 从弃牌堆中选择至多 Magic 张牌消耗
        var selected = await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Discard.GetPile(Owner),
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 0, DynamicVars["Magic"].IntValue));

        int count = selected.Count();
        foreach (var c in selected)
        {
            await CardCmd.Exhaust(choiceContext, c);
        }

        // 对目标造成消耗牌数×5的伤害
        if (count > 0)
            await DealDamageRaw(choiceContext, count * 5, target, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

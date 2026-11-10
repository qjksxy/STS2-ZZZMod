using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     V式日轮升拳 —— 2费攻击。
///     消耗手牌中的1张牌（玩家选择），对敌人造成 {Damage} 点伤害。
///     连携（触发条件）：当玩家获得格挡时，自动打出此牌。此牌本身不获得格挡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class VFormScorchingSun() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true
), IChainCardSource
{
    /// <summary>
    ///     连携条件：上一张牌让玩家获得了格挡。
    ///     通过对比出牌前后玩家的格挡值精确判断。
    /// </summary>
    public bool CheckChainCondition(CardModel card, ChainContext context)
    {
        return context.BlockGained;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(15, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips
    {
        get
        {
            var id = ZZZModKeywords.Chain;
            var description = ModKeywordRegistry.GetDescription(id);
            yield return new HoverTip(ModKeywordRegistry.GetTitle(id), description);
        }
    }

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Chain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 让玩家从手牌中选择1张牌消耗（排除自身）
        // 连携自动打出时也能唤起选择界面（ChainSystem 已使用 BlockingPlayerChoiceContext）
        var selected = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
            (CardModel c) => c != cardPlay.Card,
            this)).FirstOrDefault();

        if (selected != null)
        {
            await CardCmd.Exhaust(choiceContext, selected);
        }

        // 造成伤害
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}

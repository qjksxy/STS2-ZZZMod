using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     特别拍照技巧 —— 1费技能。对目标施加 {Magic} 层「磨爪器」并抽1张牌。
///     连携（触发条件）：上一张牌对敌人施加负面效果时自动打出。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SpecialPhotographyTechnique() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true
), IChainCardSource
{
    /// <summary>
    ///     连携条件：上一张牌对敌人施加了新的负面效果。
    /// </summary>
    public bool CheckChainCondition(CardModel card, ChainContext context)
    {
        return context.DebuffApplied;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 3m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips
    {
        get
        {
            var id = ZZZModKeywords.Chain;
            var description = ModKeywordRegistry.GetDescription(id);
            yield return new HoverTip(ModKeywordRegistry.GetTitle(id), description);
            yield return HoverTipFactory.FromPower<ClawSharpenersPower>();
        }
    }

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Chain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 对目标施加 {Magic} 层「磨爪器」
        await ApplyPowerTo<ClawSharpenersPower>(
            choiceContext, target, DynamicVars["Magic"].IntValue, cardPlay.Card);

        // 并抽1张牌
        await CardPileCmd.Draw(choiceContext, 1m, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(2m);
    }
}

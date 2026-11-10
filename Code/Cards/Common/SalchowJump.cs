using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     萨霍夫跳 —— 1费技能，施加 {Magic} 层易伤。
///     连携：对敌人施加负面效果时自动打出。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SalchowJump() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true
), IChainCardSource
{
    /// <summary>
    ///     连携条件：上一张牌对目标敌人施加了新的 Debuff。
    ///     通过对比出牌前后目标的 Debuff 数量精确判断。
    /// </summary>
    public bool CheckChainCondition(CardModel card, ChainContext context)
    {
        return context.DebuffApplied;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 2m)
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
        await ApplyPowerTo<VulnerablePower>(choiceContext, target, DynamicVars["Magic"].IntValue, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

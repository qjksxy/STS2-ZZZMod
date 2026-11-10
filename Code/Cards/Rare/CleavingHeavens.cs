using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Rare;

/// <summary>
///     斩妄开天 —— 2费技能。获得 {Block} 点格挡。
///     连携（触发条件）：打出攻击牌时自动打出。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class CleavingHeavens() : ZZZBaseCard(
    2, CardType.Skill, CardRarity.Rare, TargetType.Self, true
), IChainCardSource
{
    /// <summary>
    ///     连携条件：上一张打出的牌是攻击牌。
    /// </summary>
    public bool CheckChainCondition(CardModel card, ChainContext context)
    {
        return context.LastPlayed.Type == CardType.Attack;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12, ValueProp.Move)
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
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }
}

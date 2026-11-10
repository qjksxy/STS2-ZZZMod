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

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     幽闪 —— 1费技能，获得 {Block} 点格挡。
///     连携（触发条件）：上一张牌对自身施加了 Buff 时，自动打出此牌。此牌本身不给予任何增益。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SpectralFlash() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self, true
), IChainCardSource
{
    /// <summary>
    ///     连携条件：上一张牌对玩家自身施加了 Buff 类型的 Power。
    ///     通过对比出牌前后玩家身上的 Buff 实例数精确判断。
    /// </summary>
    public bool CheckChainCondition(CardModel card, ChainContext context)
    {
        return context.BuffAppliedToSelf;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(9, ValueProp.Move)
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
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

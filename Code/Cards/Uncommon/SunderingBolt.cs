using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     极雷断空 —— 1费能力，触发连携时，获得 {Magic} 点格挡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SunderingBolt() : ZZZBaseCard(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 9m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips
    {
        get
        {
            var id = ZZZModKeywords.Chain;
            var description = ModKeywordRegistry.GetDescription(id);
            yield return new HoverTip(ModKeywordRegistry.GetTitle(id), description);
            yield return HoverTipFactory.FromPower<SunderingBoltPower>();
        }
    }

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Chain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyPowerSelf<SunderingBoltPower>(choiceContext, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        // CSV 强费列为空，不降费用；仅强魔 +3
        DynamicVars["Magic"].UpgradeValueBy(3m);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     与火共舞 —— 1费能力。获得 1 层「鬼火」。
///     鬼火（Magus）：触发连携时，额外打出 1 张连携牌。
///     关键词列的「连携」仅为展示，本牌不是连携卡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class DanceWithFire() : ZZZBaseCard(
    1, CardType.Power, CardRarity.Common, TargetType.Self, true)
{
    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips
    {
        get
        {
            var id = ZZZModKeywords.Chain;
            var description = ModKeywordRegistry.GetDescription(id);
            yield return new HoverTip(ModKeywordRegistry.GetTitle(id), description);
            yield return HoverTipFactory.FromPower<MagusPower>();
        }
    }

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Chain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得1层「鬼火」
        await ApplyPowerSelf<MagusPower>(choiceContext, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);   // 强费列
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Rare;

/// <summary>
///     曙色颂·转辉 —— 1费能力，获得1层「浮晖」。
///     浮晖（Floating Radiance）：触发连携时，抽1张牌并获得能量。
///     「连携」关键词仅作展示（本牌自身无连携触发条件）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class OdeToDawnRadiantTurn() : ZZZBaseCard(
    1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
{
    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<FloatingRadiancePower>();
            var id = ZZZModKeywords.Chain;
            var description = ModKeywordRegistry.GetDescription(id);
            yield return new HoverTip(ModKeywordRegistry.GetTitle(id), description);
        }
    }

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Chain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyPowerSelf<FloatingRadiancePower>(choiceContext, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);   // 强费列
    }
}

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Rare;

/// <summary>
///     投资 —— 2费稀有技能。打出时获得1层投资。
///     升级后降为1费。
/// </summary>
[RegisterCard(typeof(ColorlessCardPool))]
public class InvestCard() : ZZZBaseCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("InvestAmount", 1m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<InvestPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = DynamicVars["InvestAmount"].IntValue;
        await ApplyPowerSelf<InvestPower>(amount);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

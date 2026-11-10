using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     落霞风暴 —— 1费能力，每回合开始时获得1层「钢能」，并额外抽1张攻击牌。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class TempestOfFrostyPetals() : ZZZBaseCard(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
{
    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<SteelChargePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyPowerSelf<TempestOfFrostyPetalsPower>(choiceContext, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);   // 强费列
    }
}

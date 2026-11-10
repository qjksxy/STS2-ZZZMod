using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     协奏狙杀 —— 1费能力，打出单目标攻击牌时，额外对目标造成1点失衡。
///     若目标失衡值已满或处于失衡状态时，则额外对目标造成5点伤害。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class HarmonizingShot() : ZZZBaseCard(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true
)
{
    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<HarmonizingShotPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyPowerSelf<HarmonizingShotPower>(choiceContext, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

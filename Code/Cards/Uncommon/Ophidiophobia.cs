using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     爬行恐惧 —— 2费技能，获得 {Block} 点格挡。
///     打出此牌后，本回合每造成1点失衡，额外获得1点格挡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class Ophidiophobia() : ZZZBaseCard(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<OphidiophobiaPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);
        await ApplyPowerSelf<OphidiophobiaPower>(choiceContext, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(6);   // 强防列
    }
}

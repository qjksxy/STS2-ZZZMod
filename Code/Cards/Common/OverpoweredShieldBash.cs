using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     超强力盾击 —— 1费技能，获得 {Block} 点格挡。
///     获得1层「喧嚣直刺」。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class OverpoweredShieldBash() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self, true
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<RoaringThrustPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);
        await ApplyPowerSelf<RoaringThrustPower>(choiceContext, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }
}

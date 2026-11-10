using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     撼天动地 —— 2费技能。
///     获得 {Block} 点格挡。本回合你受到的每次伤害减少 {Magic} 点。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class TheWorldTrembles() : ZZZBaseCard(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12, ValueProp.Move),
        new DynamicVar("Magic", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);

        // 本回合你受到的每次伤害减少 {Magic} 点（由配套能力实现）
        await ApplyPowerSelf<TheWorldTremblesPower>(
            choiceContext, DynamicVars["Magic"].IntValue, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);      // 强防列
        DynamicVars["Magic"].UpgradeValueBy(1m);  // 强魔列
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     死神协奏曲 —— 0费技能。
///     下回合开始时获得 {Block} 点格挡（原版能力 BlockNextTurnPower）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SymphonyOfTheReaper() : ZZZBaseCard(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(7, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 下回合开始时获得 {Block} 点格挡
        await ApplyPowerSelf<BlockNextTurnPower>(
            choiceContext, DynamicVars.Block.IntValue, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);   // 强防列
    }
}

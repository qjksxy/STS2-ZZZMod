using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Character;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Basic;

/// <summary>
///     防御 —— 1费普通技能，获得5点格挡。
///     角色初始卡组包含4张。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
[RegisterCharacterStarterCard(typeof(ZZZCharacter), 4)]
public sealed class ZZZDefend() : ZZZBaseCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self, true)
{

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

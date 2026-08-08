using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Character;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Basic;

/// <summary>
///     打击 —— 1费普通攻击，造成6点伤害。
///     角色初始卡组包含5张。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
[RegisterCharacterStarterCard(typeof(ZZZCharacter), 5)]
public sealed class ZZZStrike() : ZZZBaseCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, true)
{

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;
        await DealDamage(choiceContext, DynamicVars.Damage, target);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}

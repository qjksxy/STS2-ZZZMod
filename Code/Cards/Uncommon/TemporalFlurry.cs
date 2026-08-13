using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     时序连击 —— 1费攻击，造成5点伤害，每次攻击使目标失衡 -3。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class TemporalFlurry() : ZZZBaseCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
{
    public override int DazeAmount => 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}

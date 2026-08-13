using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     悖论冲击 —— 2费攻击，造成12点伤害，若目标正在失衡则伤害翻倍。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class ParadoxStrike() : ZZZBaseCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        var damage = DynamicVars.Damage;
        // 目标有 DazePower（正在失衡）→ 伤害翻倍
        if (target.HasPower<DazePower>())
            await DealDamageRaw(choiceContext, damage.BaseValue * 2, target, cardPlay);
        else
            await DealDamage(choiceContext, damage, target, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
    }
}

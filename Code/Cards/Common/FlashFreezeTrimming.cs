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
///     急冻修剪法 —— 1费攻击，造成 {Damage} 点伤害。
///     下回合开始时对目标再次造成等量伤害（FrostbitePower）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class FlashFreezeTrimming() : ZZZBaseCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<FrostbitePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);
        await PowerCmd.Apply<FrostbitePower>(choiceContext, target, 1, Owner.Creature, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}

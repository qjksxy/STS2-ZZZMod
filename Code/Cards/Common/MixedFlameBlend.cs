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
///     炽焰搅拌式 —— 1费攻击，造成 {Damage} 点伤害。
///     对目标施加 {Magic} 层「灼焰」，抽 {Magic} 张牌。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class MixedFlameBlend() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("Magic", 1m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<ScorchingPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 施加灼焰
        await ApplyPowerTo<ScorchingPower>(choiceContext, target, DynamicVars["Magic"].IntValue, cardPlay.Card);

        // 抽牌
        int drawCount = DynamicVars["Magic"].IntValue;
        if (drawCount > 0)
            await CardPileCmd.Draw(choiceContext, drawCount, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

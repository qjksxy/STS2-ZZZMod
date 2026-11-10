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

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     乱琼 —— 1费攻击，对目标造成 {Damage} 点伤害。
///     下回合你的第一张攻击牌额外造成 {Magic} 点伤害。
///     强伤为 0 是设计意图（升级只提升额外伤害）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class ScatteredJade() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("Magic", 6m)   // 下回合第一张攻击牌的额外伤害
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<ScatteredJadePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);
        await ApplyPowerSelf<ScatteredJadePower>(choiceContext, DynamicVars["Magic"].IntValue, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        // 强伤=0 不升级伤害（设计意图），只升级额外伤害
        DynamicVars["Magic"].UpgradeValueBy(4m);   // 强魔列
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     毒牙 —— 1费攻击，对目标造成 {Damage} 点伤害 {Magic} 次。
///     每段命中自动累积失衡（基类默认 DazeAmount = 1）。
///     强伤为 0 是设计意图（升级只提升段数）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class VenomousBite() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move),
        new DynamicVar("Magic", 3m)   // 攻击段数
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        int times = DynamicVars["Magic"].IntValue;
        for (int i = 0; i < times; i++)
        {
            await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        // 强伤=0 不升级伤害（设计意图），只升级段数
        DynamicVars["Magic"].UpgradeValueBy(1m);   // 强魔列
    }
}

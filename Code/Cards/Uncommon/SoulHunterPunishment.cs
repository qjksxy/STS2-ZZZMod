using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     魂狩·惩戒 —— 2费攻击，对目标造成失衡值上限的伤害。
///     若目标正处于失衡状态，额外造成 {Magic} 倍伤害。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SoulHunterPunishment() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true
)
{
    // 默认每次命中+1失衡（CSV 无豁免依据，恢复模块默认口径）

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 获取目标失衡状态
        if (!DazeStore.TryGet(target, out var daze)) return;

        // 计算伤害：失衡值上限
        int baseDamage = DazeSystem.CalcMaxDaze(target);
        int totalDamage = baseDamage;

        // 若目标正处于失衡状态，额外造成 {Magic} 倍伤害
        if (daze.IsDazed)
        {
            totalDamage = (int)(baseDamage * (1 + DynamicVars["Magic"].BaseValue));
        }

        // 造成伤害（一次打出，不分段）
        await DealDamageRaw(choiceContext, totalDamage, target, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

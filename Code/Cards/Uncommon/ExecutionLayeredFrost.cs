using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     处刑式·重霜 —— 2费技能，对目标造成 {Damage} 点伤害。
///     若目标有增益，偷取其1点力量；否则对其施加1层「虚弱」。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class ExecutionLayeredFrost() : ZZZBaseCard(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        if (target.Powers.Any(p => p.Type == PowerType.Buff))
        {
            // 目标有增益：偷取其1点力量（只偷取实际存在的层数，无力量可偷则不获得）
            int steal = Math.Min(1, Math.Max(0, target.GetPower<StrengthPower>()?.Amount ?? 0));
            if (steal > 0)
            {
                await ModifyPower<StrengthPower>(choiceContext, target, -steal, cardPlay.Card);
                await ApplyPowerSelf<StrengthPower>(choiceContext, steal, cardPlay.Card);
            }
        }
        else
        {
            // 目标无增益：施加1层「虚弱」
            await ApplyPowerTo<WeakPower>(choiceContext, target, 1, cardPlay.Card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);   // 强伤列
    }
}

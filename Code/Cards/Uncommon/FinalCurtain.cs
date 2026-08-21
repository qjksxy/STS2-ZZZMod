using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     终幕演出 —— 2费攻击，目标每有1层负面状态造成5点伤害（至多10层）。
///     喧响：每层负面效果额外造成 {Magic} 点伤害。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class FinalCurtain() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true
), IDecibelCardSource
{
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 1m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 计算目标身上的负面效果层数（所有 Debuff 类型的 Power）
        int debuffCount = target.Powers
            .Where(p => p.Type == PowerType.Debuff)
            .Sum(p => Math.Max(p.Amount, 1));
        debuffCount = Math.Min(debuffCount, 10);

        // 基础伤害：每层负面状态造成5点伤害
        int baseDamage = debuffCount * 5;

        // 喧响：每层负面效果额外造成 Magic 点伤害
        int bonusDamage = 0;
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            bonusDamage = debuffCount * DynamicVars["Magic"].IntValue;
        }

        int totalDamage = baseDamage + bonusDamage;
        if (totalDamage > 0)
        {
            await DealDamageRaw(choiceContext, totalDamage, target, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(3m);
    }
}

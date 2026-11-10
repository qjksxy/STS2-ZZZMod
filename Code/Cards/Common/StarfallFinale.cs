using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     星芒终章 —— 1费攻击，对目标造成等同于自身增益总层数的伤害 {Magic} 次。
///     喧响：获得一层自身层数最少的增益。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class StarfallFinale() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true
), IDecibelCardSource
{
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 3m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 自身增益总层数：自身所有 Buff 类型 Power 的层数之和
        int totalBuffStacks = Owner.Creature.Powers
            .Where(p => p.Type == PowerType.Buff)
            .Sum(p => Math.Max(p.Amount, 0));

        // 造成等同于自身增益总层数的伤害 {Magic} 次
        if (totalBuffStacks > 0)
        {
            int hits = DynamicVars["Magic"].IntValue;
            for (int i = 0; i < hits; i++)
            {
                await DealDamageRaw(choiceContext, totalBuffStacks, target, cardPlay);
            }
        }

        // 喧响：获得一层自身层数最少的增益（并列最少时取第一个）
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            var buffs = Owner.Creature.Powers
                .Where(p => p.Type == PowerType.Buff && p.Amount > 0)
                .OrderBy(p => p.Amount)
                .ToList();
            if (buffs.Count > 0)
            {
                await PowerCmd.ModifyAmount(choiceContext, buffs[0], 1, Owner.Creature, cardPlay.Card);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);   // 强魔列
    }
}

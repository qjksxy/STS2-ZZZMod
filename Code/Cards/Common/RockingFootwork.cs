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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     摇曳步伐 —— 2费攻击，使目标流失 {Damage} 点生命，获得2层决意。
///     若当前决意层数不少于5，则消耗2层并随机打出一张手牌中的攻击牌。
///     随机打出的牌视为正常打出（不消耗费用，可触发连携）。
///     流失生命：直接扣除生命值，不视为伤害，不受任何伤害加成/减免影响，不累积失衡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class RockingFootwork() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
{
    public override int DazeAmount => 0;   // 流失生命不视为伤害，不累积失衡

    /// <summary>决意层数门槛（卡牌文本字面量：「不少于5」）。</summary>
    private const int DeterminationThreshold = 5;

    /// <summary>消耗的决意层数（卡牌文本字面量：「消耗2层」）。</summary>
    private const int DeterminationCost = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        // 流失生命值：属性与流失管线一致（Unblockable | Unpowered | Move），不吃伤害加成/减免
        new DamageVar(15, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<DeterminationPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 目标流失 Damage 点生命
        await CreatureCmd.Damage(choiceContext, target, DynamicVars.Damage.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);

        // 获得2层决意（卡牌文本字面量：「获得2层决意」）
        await ApplyPowerSelf<DeterminationPower>(choiceContext, 2, cardPlay.Card);

        // 若当前决意层数不少于5，消耗2层并随机打出一张手牌中的攻击牌
        if (GetPowerAmount<DeterminationPower>() < DeterminationThreshold) return;
        await ModifyPower<DeterminationPower>(choiceContext, Owner.Creature, -DeterminationCost, cardPlay.Card);

        // 随机挑选手牌中的1张攻击牌（排除自身；使用 run 的 RNG，禁止 System.Random）
        var chosen = Owner.RunState.Rng.CombatCardSelection.NextItem(
            PileType.Hand.GetPile(Owner).Cards
                .Where(c => c.Type == CardType.Attack && c != cardPlay.Card));
        if (chosen == null) return;

        var combatState = CombatState;
        if (combatState == null) return;

        // 出牌前快照（与 ChainSystem.OnCardPlayed 同口径）
        int debuffBefore = combatState.Enemies
            .Where(c => c.IsAlive)
            .Sum(c => c.Powers.Where(p => p.Type == PowerType.Debuff).Sum(p => p.Amount));
        int buffBefore = Owner.Creature.Powers.Count(p => p.Type == PowerType.Buff);
        int blockBefore = Owner.Creature.Block;

        // 随机打出：视为正常打出，不消耗费用（O12 口径），可触发连携
        await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), chosen, target, AutoPlayType.Default);

        // 出牌后快照
        int debuffAfter = combatState.Enemies
            .Where(c => c.IsAlive)
            .Sum(c => c.Powers.Where(p => p.Type == PowerType.Debuff).Sum(p => p.Amount));
        int buffAfter = Owner.Creature.Powers.Count(p => p.Type == PowerType.Buff);
        int blockAfter = Owner.Creature.Block;

        // 手动触发一次连携检查（随机打出的牌可作为连携的「上一张牌」）
        var context = new ChainContext
        {
            LastPlayed = chosen,
            LastTarget = target,
            DebuffCountBefore = debuffBefore,
            DebuffCountAfter = debuffAfter,
            BuffCountBefore = buffBefore,
            BuffCountAfter = buffAfter,
            BlockBefore = blockBefore,
            BlockAfter = blockAfter,
        };
        await ChainSystem.TriggerChains(Owner, context);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);   // 强伤列
    }
}

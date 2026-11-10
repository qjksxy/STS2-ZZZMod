using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     最高马力星光 —— 1费攻击，获得1层决意。
///     随机使敌人流失 {Damage} 点生命，攻击次数为决意层数。
///     连携：连续打出2张攻击牌。
///     流失生命：直接扣除生命值，不视为伤害，不受任何伤害加成/减免影响，不累积失衡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class FullThrottleStarlight() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true
), IChainCardSource
{
    public override int DazeAmount => 0;   // 流失生命不视为伤害，不累积失衡

    /// <summary>
    ///     连携条件：连续打出2张攻击牌（照抄 PleaseDoNotResist）。
    /// </summary>
    public bool CheckChainCondition(CardModel card, ChainContext context)
    {
        var lastPlayed = context.LastPlayed;
        if (lastPlayed.Type != CardType.Attack) return false;
        var combatState = card.CombatState;
        if (combatState == null) return false;
        var combatManager = CombatManager.Instance;
        if (combatManager == null) return false;
        var history = combatManager.History;
        if (history == null) return false;
        var recentPlays = history.CardPlaysFinished
            .Where(e => e.HappenedThisTurn(combatState)
                     && e.CardPlay.Player == card.Owner
                     && !e.CardPlay.IsAutoPlay)
            .TakeLast(2).ToList();
        if (recentPlays.Count < 2) return false;
        return recentPlays.All(e => e.CardPlay.Card.Type == CardType.Attack);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        // 流失生命值：属性与流失管线一致（Unblockable | Unpowered | Move），不吃伤害加成/减免
        new DamageVar(6, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips
    {
        get
        {
            var id = ZZZModKeywords.Chain;
            var description = ModKeywordRegistry.GetDescription(id);
            yield return new HoverTip(ModKeywordRegistry.GetTitle(id), description);
            yield return HoverTipFactory.FromPower<DeterminationPower>();
        }
    }

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Chain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得1层决意（卡牌文本字面量：「获得1层决意」）
        await ApplyPowerSelf<DeterminationPower>(choiceContext, 1, cardPlay.Card);

        // 攻击次数为决意层数（含刚获得的1层）
        int times = GetPowerAmount<DeterminationPower>();

        var combatState = CombatState;
        if (combatState == null) return;

        for (int i = 0; i < times; i++)
        {
            // 随机一名存活敌人流失 Damage 点生命（每次独立随机）
            var enemy = Owner.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
            if (enemy == null) continue;

            await CreatureCmd.Damage(choiceContext, enemy, DynamicVars.Damage.BaseValue,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);   // 强伤列
    }
}

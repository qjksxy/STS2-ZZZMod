#if !STS2_AT_LEAST_0_104_0
using CombatStateLike = MegaCrit.Sts2.Core.Combat.CombatState;
#else
using CombatStateLike = MegaCrit.Sts2.Core.Combat.ICombatState;
#endif
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;
using STS2RitsuLib.Scaffolding.Cards.HandGlow;

namespace ZZZMod.Code.Chain;

/// <summary>
///     连携系统核心入口。
///     监听每次出牌，检查手牌中的连携卡是否满足条件，满足则自动打出。
/// </summary>
public static class ChainSystem
{
    public static void Init()
    {
        RitsuLibFramework.SubscribeLifecycle<CardPlayedEvent>(OnCardPlayed);
        RegisterChainCardGlows();
    }

    /// <summary>
    ///     为所有实现 <see cref="IChainCardSource" /> 的卡牌注册手牌金色高亮：
    ///     当连携条件即将满足时（差一步），手牌中的该卡发光提示。
    /// </summary>
    private static void RegisterChainCardGlows()
    {
        try
        {
            var assembly = typeof(ChainSystem).Assembly;
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (!typeof(CardModel).IsAssignableFrom(type)) continue;
                if (!typeof(IChainCardSource).IsAssignableFrom(type)) continue;

                // 连携卡在手牌中且条件满足时发金光
                ModCardHandGlowRegistry.Register(type, ModCardHandGlowRules.Gold(IsChainConditionMet));
            }
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Chain] 注册手牌发光规则时出错: {ex.Message}");
        }
    }

    /// <summary>
    ///     检查手牌中的连携卡是否当前条件已满足（用于发光判断）。
    ///     由于发光判断时没有"刚打出的牌"上下文，这里用 History 查询最近一张牌。
    /// </summary>
    private static bool IsChainConditionMet(CardModel card)
    {
        if (card is not IChainCardSource lx) return false;
        if (card.Pile?.Type != PileType.Hand) return false;

        var combatState = card.CombatState;
        if (combatState == null) return false;

        // 检查 CombatManager 和 History 是否可用
        var combatManager = CombatManager.Instance;
        if (combatManager == null) return false;
        if (!combatManager.IsInProgress) return false;
        var history = combatManager.History;
        if (history == null) return false;

        // 获取本回合最后一张打出的牌
        var lastEntry = history.CardPlaysFinished
            .LastOrDefault(e => e.HappenedThisTurn(combatState)
                             && e.CardPlay.Player == card.Owner
                             && !e.CardPlay.IsAutoPlay);

        if (lastEntry == null) return false;

        try
        {
            return lx.CheckChainCondition(card, lastEntry.CardPlay.Card);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     每次有牌打出后触发，检查手牌中的连携卡。
    ///     注意：CardPlayedEvent 在 AfterCardPlayed 之后触发，此时当前牌已完成结算。
    /// </summary>
    private static async void OnCardPlayed(CardPlayedEvent evt)
    {
        try
        {
            var cardPlay = evt.CardPlay;
            if (cardPlay == null) return;

            // 连携触发的牌不应再次触发连携（防无限循环）
            if (cardPlay.IsAutoPlay) return;

            var player = cardPlay.Player;
            if (player == null) return;

            var lastPlayed = cardPlay.Card;
            if (lastPlayed == null) return;

            // 战斗已结束或正在结束，不再触发连携
            var combatManager = CombatManager.Instance;
            if (combatManager == null || combatManager.IsOverOrEnding) return;
            if (!combatManager.IsInProgress) return;

            var hand = PileType.Hand.GetPile(player).Cards;
            if (hand.Count == 0) return;

            // 预检查：如果连携卡需要敌人目标但没有存活敌人，直接跳过
            bool hasAliveEnemy = evt.CombatState.Creatures
                .Any(c => c.Side == CombatSide.Enemy && c.IsAlive);

            // 遍历手牌，找第一张满足条件的连携卡
            foreach (var card in hand)
            {
                if (card is not IChainCardSource lx) continue;

                bool conditionMet;
                try
                {
                    conditionMet = lx.CheckChainCondition(card, lastPlayed);
                }
                catch
                {
                    continue;
                }

                if (!conditionMet) continue;

                // 攻击类卡牌需要有存活的敌人目标
                if (card.TargetType is TargetType.AnyEnemy or TargetType.AllEnemies
                    && !hasAliveEnemy)
                {
                    continue;
                }

                Entry.Logger.Debug($"[Chain] 条件满足，自动打出: {card.Id.Entry}");

                // 选择目标：攻击类随机选敌，其他类型无目标
                Creature? target = null;
                if (card.TargetType is TargetType.AnyEnemy or TargetType.AllEnemies)
                {
                    target = player.RunState.Rng.CombatTargets.NextItem(
                        evt.CombatState.Creatures.Where(c => c.Side == CombatSide.Enemy && c.IsAlive));
                }

                await CardCmd.AutoPlay(
                    new ThrowingPlayerChoiceContext(),
                    card,
                    target,
                    AutoPlayType.Default);

                // 连携卡打出后，如果战斗可能已结束（所有敌人被击杀），
                // 需要手动触发胜利判定，否则战斗会卡住。
                // CheckWinCondition 是回合流程的检查点，不会在 AutoPlay 后自动调用。
                if (combatManager.IsInProgress)
                {
                    await combatManager.CheckWinCondition();
                }

                // 一次只触发一张连携
                break;
            }
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Chain] OnCardPlayed 出错: {ex.Message}");
        }
    }
}

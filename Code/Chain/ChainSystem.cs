// zzz-mod 固定面向当前游戏 API（CardModel.CombatState 为 ICombatState），不再做旧版兼容分支。
using CombatStateLike = MegaCrit.Sts2.Core.Combat.ICombatState;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;
using STS2RitsuLib.Scaffolding.Cards.HandGlow;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Chain;

/// <summary>
///     连携系统核心入口。
///     监听出牌前后的事件，采集上下文快照，检查手牌中的连携卡是否满足条件，满足则自动打出。
/// </summary>
public static class ChainSystem
{
    /// <summary>出牌前快照：所有存活敌人的总 Debuff 层数。</summary>
    private static int _totalDebuffsBefore = -1;

    /// <summary>出牌前快照：玩家身上的 Buff 实例数。</summary>
    private static int _playerBuffCountBefore = -1;

    /// <summary>出牌前快照：玩家的格挡值。</summary>
    private static int _playerBlockBefore = -1;

    /// <summary>本回合连携触发次数（供能力监听，如 VoidstrikePower）。</summary>
    public static int ChainTriggerCountThisTurn { get; private set; }

    /// <summary>
    ///     连携触发事件。连携卡自动打出后触发，供能力监听。
    ///     参数：触发的连携卡牌。
    /// </summary>
    public static event Action<CardModel>? ChainTriggered;

    public static void Init()
    {
        RitsuLibFramework.SubscribeLifecycle<SideTurnStartingEvent>(OnSideTurnStarting);
        RitsuLibFramework.SubscribeLifecycle<CardPlayingEvent>(OnCardPlaying);
        RitsuLibFramework.SubscribeLifecycle<CardPlayedEvent>(OnCardPlayed);
        RegisterChainCardGlows();
    }

    private static void OnSideTurnStarting(SideTurnStartingEvent evt)
    {
        // 玩家回合开始时重置连携触发计数
        if (evt.Side == CombatSide.Player)
            ChainTriggerCountThisTurn = 0;
    }

    /// <summary>
    ///     出牌前触发，记录快照：敌人 Debuff 总层数、玩家 Buff 数量、玩家格挡值。
    /// </summary>
    private static void OnCardPlaying(CardPlayingEvent evt)
    {
        var cardPlay = evt.CardPlay;
        if (cardPlay == null) return;
        if (cardPlay.IsAutoPlay) return;

        var player = cardPlay.Player;
        if (player?.Creature == null) return;

        // 记录所有存活敌人的总 Debuff 层数
        _totalDebuffsBefore = evt.CombatState.Creatures
            .Where(c => c.Side == CombatSide.Enemy && c.IsAlive)
            .Sum(c => c.Powers.Where(p => p.Type == PowerType.Debuff).Sum(p => p.Amount));

        // 记录玩家身上的 Buff 实例数
        _playerBuffCountBefore = player.Creature.Powers
            .Count(p => p.Type == PowerType.Buff);

        // 记录玩家当前格挡值
        _playerBlockBefore = player.Creature.Block;
    }

    /// <summary>
    ///     为所有实现 <see cref="IChainCardSource" /> 的卡牌注册手牌金色高亮。
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

                ModCardHandGlowRegistry.Register(type, ModCardHandGlowRules.Gold(IsChainConditionMet));
            }
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[Chain] 注册手牌发光规则时出错: {ex}");
        }
    }

    /// <summary>
    ///     检查手牌中的连携卡是否当前条件已满足（用于发光判断）。
    ///     使用 History 查询最近一张牌构造 ChainContext。
    /// </summary>
    private static bool IsChainConditionMet(CardModel card)
    {
        if (card is not IChainCardSource lx) return false;
        if (card.Pile?.Type != PileType.Hand) return false;

        var combatState = card.CombatState;
        if (combatState == null) return false;

        var combatManager = CombatManager.Instance;
        if (combatManager == null) return false;
        if (!combatManager.IsInProgress) return false;
        var history = combatManager.History;
        if (history == null) return false;

        var lastEntry = history.CardPlaysFinished
            .LastOrDefault(e => e.HappenedThisTurn(combatState)
                             && e.CardPlay.Player == card.Owner
                             && !e.CardPlay.IsAutoPlay);

        if (lastEntry == null) return false;

        var lastTarget = lastEntry.CardPlay.Target;
        int totalDebuffs = combatState.Creatures
            .Where(c => c.Side == CombatSide.Enemy && c.IsAlive)
            .Sum(c => c.Powers.Where(p => p.Type == PowerType.Debuff).Sum(p => p.Amount));

        var context = new ChainContext
        {
            LastPlayed = lastEntry.CardPlay.Card,
            LastTarget = lastTarget,
            DebuffCountBefore = -1,  // 发光判断时无法获取出牌前快照
            DebuffCountAfter = totalDebuffs,
        };

        try
        {
            return lx.CheckChainCondition(card, context);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Chain] 检查连携条件时出错 ({card.Id.Entry}): {ex.Message}");
            return false;
        }
    }

    /// <summary>
    ///     出牌后触发，检查手牌中的连携卡。
    /// </summary>
    private static async void OnCardPlayed(CardPlayedEvent evt)
    {
        try
        {
            var cardPlay = evt.CardPlay;
            if (cardPlay == null) return;
            if (cardPlay.IsAutoPlay) return;

            var player = cardPlay.Player;
            if (player == null) return;

            var lastPlayed = cardPlay.Card;
            if (lastPlayed == null) return;

            var combatManager = CombatManager.Instance;
            if (combatManager == null || combatManager.IsOverOrEnding) return;
            if (!combatManager.IsInProgress) return;

            var hand = PileType.Hand.GetPile(player).Cards;
            if (hand.Count == 0) return;

            // 构造 ChainContext：结合出牌前快照和出牌后状态
            var lastTarget = cardPlay.Target;

            // 出牌后：敌人总 Debuff 层数
            int totalDebuffsAfter = evt.CombatState.Creatures
                .Where(c => c.Side == CombatSide.Enemy && c.IsAlive)
                .Sum(c => c.Powers.Where(p => p.Type == PowerType.Debuff).Sum(p => p.Amount));

            // 出牌后：玩家 Buff 实例数
            int playerBuffCountAfter = player.Creature.Powers
                .Count(p => p.Type == PowerType.Buff);

            // 出牌后：玩家格挡值
            int playerBlockAfter = player.Creature.Block;

            var context = new ChainContext
            {
                LastPlayed = lastPlayed,
                LastTarget = lastTarget,
                DebuffCountBefore = _totalDebuffsBefore,
                DebuffCountAfter = totalDebuffsAfter,
                BuffCountBefore = _playerBuffCountBefore,
                BuffCountAfter = playerBuffCountAfter,
                BlockBefore = _playerBlockBefore,
                BlockAfter = playerBlockAfter,
            };

            // 重置快照
            _totalDebuffsBefore = -1;
            _playerBuffCountBefore = -1;
            _playerBlockBefore = -1;

            bool hasAliveEnemy = evt.CombatState.Creatures
                .Any(c => c.Side == CombatSide.Enemy && c.IsAlive);

            // 遍历手牌，找第一张满足条件的连携卡并自动打出
            bool played = await TryPlayOneChain(player, context, evt.CombatState, hasAliveEnemy, hand);

            // 鬼火（Magus）：触发连携时，额外打出1张连携牌。
            // 同一张出牌最多额外触发1张（防无限循环）；额外打出的连携同样视为一次连携触发（触发连携事件）。
            if (played && player.Creature?.GetPower<MagusPower>() != null)
            {
                await TryPlayOneChain(player, context, evt.CombatState, hasAliveEnemy,
                    PileType.Hand.GetPile(player).Cards);
            }
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[Chain] OnCardPlayed 严重错误: {ex}");
        }
    }

    /// <summary>
    ///     手动触发一次连携检查。
    ///     供「摇曳步伐」这类"视为正常打出、可触发连携"的免费出牌使用。
    ///     调用方自行构造 ChainContext（LastPlayed = 实际打出的那张牌）。
    /// </summary>
    public static async Task TriggerChains(Player player, ChainContext context)
    {
        try
        {
            var combatManager = CombatManager.Instance;
            if (combatManager == null || combatManager.IsOverOrEnding) return;
            if (!combatManager.IsInProgress) return;

            var combatState = context.LastPlayed.CombatState;
            if (combatState == null) return;

            var hand = PileType.Hand.GetPile(player).Cards;
            if (hand.Count == 0) return;

            bool hasAliveEnemy = combatState.Creatures
                .Any(c => c.Side == CombatSide.Enemy && c.IsAlive);

            bool played = await TryPlayOneChain(player, context, combatState, hasAliveEnemy, hand);

            if (played && player.Creature?.GetPower<MagusPower>() != null)
            {
                await TryPlayOneChain(player, context, combatState, hasAliveEnemy,
                    PileType.Hand.GetPile(player).Cards);
            }
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[Chain] 手动触发连携检查严重错误: {ex}");
        }
    }

    /// <summary>
    ///     从手牌中找出第一张满足条件的连携卡并自动打出。
    ///     返回是否成功打出。打出后触发 ChainTriggered 事件并计数（每次打出计为一次连携触发）。
    /// </summary>
    private static async Task<bool> TryPlayOneChain(
        Player player,
        ChainContext context,
        CombatStateLike combatState,
        bool hasAliveEnemy,
        IEnumerable<CardModel> hand)
    {
        foreach (var card in hand)
        {
            if (card is not IChainCardSource lx) continue;

            bool conditionMet;
            try
            {
                conditionMet = lx.CheckChainCondition(card, context);
            }
            catch (Exception ex)
            {
                Entry.Logger.Warn($"[Chain] 检查连携条件时出错 ({card.Id.Entry}): {ex.Message}");
                continue;
            }

            if (!conditionMet) continue;

            if (card.TargetType is TargetType.AnyEnemy or TargetType.AllEnemies
                && !hasAliveEnemy)
            {
                continue;
            }

            Entry.Logger.Debug($"[Chain] 条件满足，自动打出: {card.Id.Entry}");

            Creature? target = null;
            if (card.TargetType is TargetType.AnyEnemy or TargetType.AllEnemies)
            {
                target = player.RunState.Rng.CombatTargets.NextItem(
                    combatState.Creatures.Where(c => c.Side == CombatSide.Enemy && c.IsAlive));
            }

            // 使用 BlockingPlayerChoiceContext 以支持需要玩家交互的卡牌效果（如选择手牌）
            await CardCmd.AutoPlay(
                new BlockingPlayerChoiceContext(),
                card,
                target,
                AutoPlayType.Default);

            // 触发连携事件（供能力监听）
            ChainTriggerCountThisTurn++;
            try
            {
                ChainTriggered?.Invoke(card);
            }
            catch (Exception ex)
            {
                Entry.Logger.Warn($"[Chain] 触发连携事件时出错: {ex.Message}");
            }

            var combatManager = CombatManager.Instance;
            if (combatManager != null && combatManager.IsInProgress)
            {
                await combatManager.CheckWinCondition();
            }

            return true;
        }

        return false;
    }
}

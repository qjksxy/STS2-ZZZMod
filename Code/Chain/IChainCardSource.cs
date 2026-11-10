using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace ZZZMod.Code.Chain;

/// <summary>
///     连携系统在出牌前后采集的上下文数据。
///     用于精确判断各种连携触发条件。
/// </summary>
public sealed class ChainContext
{
    /// <summary>上一张打出的卡牌。</summary>
    public required CardModel LastPlayed { get; init; }

    /// <summary>上一张牌的实际目标（可能为 null）。</summary>
    public required Creature? LastTarget { get; init; }

    // ── Debuff 快照（敌人）──

    /// <summary>出牌前：所有存活敌人的总 Debuff 层数。-1 表示无数据。</summary>
    public int DebuffCountBefore { get; init; } = -1;

    /// <summary>出牌后：所有存活敌人的总 Debuff 层数。-1 表示无数据。</summary>
    public int DebuffCountAfter { get; init; } = -1;

    /// <summary>上一张牌是否对敌人施加了新的 Debuff。</summary>
    public bool DebuffApplied => DebuffCountBefore >= 0
                              && DebuffCountAfter > DebuffCountBefore;

    // ── Buff 快照（玩家自身）──

    /// <summary>出牌前：玩家身上的 Buff 实例数。-1 表示无数据。</summary>
    public int BuffCountBefore { get; init; } = -1;

    /// <summary>出牌后：玩家身上的 Buff 实例数。-1 表示无数据。</summary>
    public int BuffCountAfter { get; init; } = -1;

    /// <summary>上一张牌是否对玩家自身施加了新的 Buff。</summary>
    public bool BuffAppliedToSelf => BuffCountBefore >= 0
                                  && BuffCountAfter > BuffCountBefore;

    // ── 格挡快照（玩家自身）──

    /// <summary>出牌前：玩家的格挡值。-1 表示无数据。</summary>
    public int BlockBefore { get; init; } = -1;

    /// <summary>出牌后：玩家的格挡值。-1 表示无数据。</summary>
    public int BlockAfter { get; init; } = -1;

    /// <summary>上一张牌是否让玩家获得了格挡。</summary>
    public bool BlockGained => BlockBefore >= 0
                            && BlockAfter > BlockBefore;
}

/// <summary>
///     连携卡牌接口。
///     实现此接口的卡牌在手牌中时，当满足连携条件会自动打出。
///     条件检查在每次有牌打出后触发。
/// </summary>
public interface IChainCardSource
{
    /// <summary>
    ///     检查当前是否满足连携条件。
    /// </summary>
    /// <param name="card">当前连携卡牌实例（手牌中的这张牌）。</param>
    /// <param name="context">出牌前后采集的上下文数据。</param>
    /// <returns>true 表示条件满足，应自动打出。</returns>
    bool CheckChainCondition(CardModel card, ChainContext context);
}

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace ZZZMod.Code.Chain;

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
    /// <param name="lastPlayed">刚刚打出的那张牌（触发检查的牌）。</param>
    /// <returns>true 表示条件满足，应自动打出。</returns>
    bool CheckChainCondition(CardModel card, CardModel lastPlayed);
}

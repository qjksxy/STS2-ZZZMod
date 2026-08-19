using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     节拍刃 —— 1费攻击，造成 {Damage} 点伤害。
///     连携：连续打出2张攻击牌时，自动从手牌打出（不消耗费用）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class BeatBlade() : ZZZBaseCard(
    1,                    // 费用
    CardType.Attack,      // 类型
    CardRarity.Common,    // 稀有度
    TargetType.AnyEnemy,  // 目标：单体敌人
    true                  // 显示在图鉴
), IChainCardSource
{
    /// <summary>
    ///     连携条件：本回合连续打出2张攻击牌。
    ///     检查最近2张牌的类型是否都是 Attack。
    /// </summary>
    public bool CheckChainCondition(CardModel card, CardModel lastPlayed)
    {
        // 刚打出的牌必须是攻击牌（否则"连续"中断）
        if (lastPlayed.Type != CardType.Attack) return false;

        var combatState = card.CombatState;
        if (combatState == null) return false;

        // 检查 CombatManager 和 History 是否可用
        var combatManager = CombatManager.Instance;
        if (combatManager == null) return false;
        var history = combatManager.History;
        if (history == null) return false;

        // 获取本回合最近2张非自动打出的牌
        var recentPlays = history.CardPlaysFinished
            .Where(e => e.HappenedThisTurn(combatState)
                     && e.CardPlay.Player == card.Owner
                     && !e.CardPlay.IsAutoPlay)
            .TakeLast(2)
            .ToList();

        // 不足2张，条件不满足
        if (recentPlays.Count < 2) return false;

        // 检查最近2张是否全是攻击牌
        return recentPlays.All(e => e.CardPlay.Card.Type == CardType.Attack);
    }

    // ── 动态变量 ──
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move)
    ];

    // ── 悬浮提示 ──
    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips
    {
        get
        {
            var id = ZZZModKeywords.Chain;
            var description = ModKeywordRegistry.GetDescription(id);
            yield return new HoverTip(ModKeywordRegistry.GetTitle(id), description);
        }
    }

    // ── 自定义关键词 ──
    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Chain];

    // ── 打出效果 ──
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);
    }

    // ── 升级效果 ──
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}

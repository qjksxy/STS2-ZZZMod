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
///     和弦护盾 —— 1费技能，获得 {Block} 点格挡。
///     连携：连续打出3张技能牌时，自动从手牌打出（不消耗费用）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class ChordShield() : ZZZBaseCard(
    1,                    // 费用
    CardType.Skill,       // 类型
    CardRarity.Common,    // 稀有度
    TargetType.Self,      // 目标：自身
    true                  // 显示在图鉴
), IChainCardSource
{
    /// <summary>
    ///     连携条件：本回合连续打出3张技能牌。
    ///     检查最近3张牌的类型是否都是 Skill。
    /// </summary>
    public bool CheckChainCondition(CardModel card, CardModel lastPlayed)
    {
        // 刚打出的牌必须是技能牌（否则"连续"中断）
        if (lastPlayed.Type != CardType.Skill) return false;

        var combatState = card.CombatState;
        if (combatState == null) return false;

        // 检查 CombatManager 和 History 是否可用
        var combatManager = CombatManager.Instance;
        if (combatManager == null) return false;
        var history = combatManager.History;
        if (history == null) return false;

        // 获取本回合最近3张非自动打出的牌
        var recentPlays = history.CardPlaysFinished
            .Where(e => e.HappenedThisTurn(combatState)
                     && e.CardPlay.Player == card.Owner
                     && !e.CardPlay.IsAutoPlay)
            .TakeLast(3)
            .ToList();

        // 不足3张，条件不满足
        if (recentPlays.Count < 3) return false;

        // 检查最近3张是否全是技能牌
        return recentPlays.All(e => e.CardPlay.Card.Type == CardType.Skill);
    }

    // ── 动态变量 ──
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(10, ValueProp.Move)
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

    // ── 打出效果（手动打出或连携自动打出都走这里）──
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);
    }

    // ── 升级效果 ──
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }
}

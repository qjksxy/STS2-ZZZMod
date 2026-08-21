using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     萨霍夫跳 —— 1费技能，施加 {Magic} 层易伤。
///     连携：对敌人施加1层负面效果时自动打出。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SalchowJump() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true
), IChainCardSource
{
    /// <summary>
    ///     连携条件：上一张牌的目标是敌人，且该敌人身上有 Debuff 类型的能力。
    ///     这近似判断"对敌人施加了负面效果"。
    /// </summary>
    public bool CheckChainCondition(CardModel card, CardModel lastPlayed)
    {
        // 上一张牌必须有敌人目标
        if (lastPlayed.TargetType != TargetType.AnyEnemy
            && lastPlayed.TargetType != TargetType.AllEnemies)
            return false;

        var combatState = card.CombatState;
        if (combatState == null) return false;

        var combatManager = CombatManager.Instance;
        if (combatManager == null) return false;
        var history = combatManager.History;
        if (history == null) return false;

        // 获取上一张非自动打出的牌
        var lastEntry = history.CardPlaysFinished
            .LastOrDefault(e => e.HappenedThisTurn(combatState)
                             && e.CardPlay.Player == card.Owner
                             && !e.CardPlay.IsAutoPlay);

        if (lastEntry == null) return false;

        // 检查上一张牌的目标是否是敌人
        var target = lastEntry.CardPlay.Target;
        if (target == null || target.Side != CombatSide.Enemy || !target.IsAlive)
            return false;

        // 检查目标敌人身上是否有 Debuff 类型的能力（说明刚施加了负面效果）
        return target.Powers.Any(p => p.Type == PowerType.Debuff);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 2m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips
    {
        get
        {
            var id = ZZZModKeywords.Chain;
            var description = ModKeywordRegistry.GetDescription(id);
            yield return new HoverTip(ModKeywordRegistry.GetTitle(id), description);
        }
    }

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Chain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 施加易伤
        await ApplyPowerTo<VulnerablePower>(choiceContext, target, DynamicVars["Magic"].IntValue, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

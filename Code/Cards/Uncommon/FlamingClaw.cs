using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     燎身爪 —— 1费攻击。
///     对目标造成 {Damage} 点伤害并获得 {Magic} 层「虎威」。
///     连携（触发条件）：连续打出2张技能牌。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class FlamingClaw() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true
), IChainCardSource
{
    /// <summary>
    ///     连携条件：连续打出2张技能牌。
    ///     本回合玩家最后2次手动出牌均为技能牌。
    /// </summary>
    public bool CheckChainCondition(CardModel card, ChainContext context)
    {
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
        return recentPlays.All(e => e.CardPlay.Card.Type == CardType.Skill);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("Magic", 2m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips
    {
        get
        {
            var id = ZZZModKeywords.Chain;
            var description = ModKeywordRegistry.GetDescription(id);
            yield return new HoverTip(ModKeywordRegistry.GetTitle(id), description);
            yield return HoverTipFactory.FromPower<HuWeiPower>();
        }
    }

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Chain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 获得 {Magic} 层「虎威」
        await ApplyPowerSelf<HuWeiPower>(choiceContext, DynamicVars["Magic"].IntValue, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);     // 强伤列
        DynamicVars["Magic"].UpgradeValueBy(1m);  // 强魔列
    }
}

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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     送客！ —— 2费攻击。对目标造成 {Damage} 点伤害并施加2层「虚弱」。
///     若此牌由连携打出，额外造成 {Magic} 点伤害并获得能量。
///     连携（触发条件）：回合内打出3张技能牌时自动打出。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class GetLost() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true
), IChainCardSource
{
    /// <summary>连携条件所需的手动技能牌数量（卡牌文本字面量：「回合内打出3张技能牌」）。</summary>
    private const int SkillPlaysRequired = 3;

    /// <summary>「虚弱」层数（卡牌文本字面量：「施加2层『虚弱』」）。</summary>
    private const int WeakStacks = 2;

    /// <summary>
    ///     连携条件：本回合内已打出3张技能牌（手动打出，不含连携/自动打出）。
    /// </summary>
    public bool CheckChainCondition(CardModel card, ChainContext context)
    {
        var combatState = card.CombatState;
        if (combatState == null) return false;
        var combatManager = CombatManager.Instance;
        if (combatManager == null) return false;
        var history = combatManager.History;
        if (history == null) return false;

        var skillCount = history.CardPlaysFinished
            .Count(e => e.HappenedThisTurn(combatState)
                     && e.CardPlay.Player == card.Owner
                     && !e.CardPlay.IsAutoPlay
                     && e.CardPlay.Card.Type == CardType.Skill);
        return skillCount >= SkillPlaysRequired;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("Magic", 4m)
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

        // 若此牌由连携打出，额外伤害并入一次结算
        decimal damage = DynamicVars.Damage.BaseValue
                       + (cardPlay.IsAutoPlay ? DynamicVars["Magic"].BaseValue : 0m);
        await DealDamageRaw(choiceContext, damage, target, cardPlay);

        // 施加2层「虚弱」
        await ApplyPowerTo<WeakPower>(choiceContext, target, WeakStacks, cardPlay.Card);

        // 若此牌由连携打出，获得能量（1点）
        if (cardPlay.IsAutoPlay)
            await GainEnergy(1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Magic"].UpgradeValueBy(2m);
    }
}

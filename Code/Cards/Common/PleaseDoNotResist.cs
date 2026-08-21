using MegaCrit.Sts2.Core.Combat;
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

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     请勿抵抗 —— 2费攻击，造成 {Damage} 点伤害。
///     连携：连续打出2张攻击牌时自动打出。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class PleaseDoNotResist() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true
), IChainCardSource
{
    public bool CheckChainCondition(CardModel card, CardModel lastPlayed)
    {
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
        new DamageVar(10, ValueProp.Move)
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
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}

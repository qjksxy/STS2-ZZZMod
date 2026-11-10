using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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
///     聆听呼啸 —— 2费群体攻击，对敌方全体造成 {Damage} 点伤害。
///     每击杀一名敌人，获得能量（1点）。
///     连携：回合内打出3张攻击牌。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class HeedTheTempest() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies, true
), IChainCardSource
{
    /// <summary>
    ///     连携条件：回合内已打出3张攻击牌（含本次触发的这张）。
    /// </summary>
    public bool CheckChainCondition(CardModel card, ChainContext context)
    {
        var combatState = card.CombatState;
        if (combatState == null) return false;

        var combatManager = CombatManager.Instance;
        if (combatManager == null) return false;

        var history = combatManager.History;
        if (history == null) return false;

        int attackPlays = history.CardPlaysFinished
            .Count(e => e.HappenedThisTurn(combatState)
                     && e.CardPlay.Player == card.Owner
                     && !e.CardPlay.IsAutoPlay
                     && e.CardPlay.Card.Type == CardType.Attack);

        return attackPlays >= 3;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move)
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
        var combatState = CombatState;
        if (combatState == null) return;

        // 记录打出前存活敌人数
        int aliveBefore = combatState.Enemies.Count(c => c.IsAlive);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 打出后存活数之差 = 击杀数，每击杀一名敌人获得能量（1点）
        int aliveAfter = combatState.Enemies.Count(c => c.IsAlive);
        int kills = aliveBefore - aliveAfter;

        await GainEnergy(kills);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);   // 强伤列
    }
}

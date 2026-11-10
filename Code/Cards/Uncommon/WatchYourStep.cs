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

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     小心脚下 —— 1费攻击。
///     对目标造成 {Damage} 点伤害，对一名随机敌人追加一次 {Magic} 点伤害。
///     连携（触发条件）：获得格挡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class WatchYourStep() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true
), IChainCardSource
{
    /// <summary>
    ///     连携条件：上一张牌让玩家获得了格挡。
    /// </summary>
    public bool CheckChainCondition(CardModel card, ChainContext context)
    {
        return context.BlockGained;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
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

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 对一名随机敌人追加一次 {Magic} 点伤害（可为原目标）
        var combatState = CombatState;
        if (combatState == null) return;
        var enemies = combatState.Creatures
            .Where(c => c.Side == CombatSide.Enemy && c.IsAlive)
            .ToList();
        if (enemies.Count == 0) return;

        var extraTarget = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
        await DealDamageRaw(choiceContext, DynamicVars["Magic"].BaseValue, extraTarget, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);     // 强伤列
        DynamicVars["Magic"].UpgradeValueBy(2m);  // 强魔列
    }
}

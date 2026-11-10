using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     通通砸碎！ —— 1费技能。获得 {Block} 点格挡。
///     喧响：对敌方全体施加5层「磨爪器」。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SmashItAll() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self, true
), IDecibelCardSource
{
    public int DecibelCost => DecibelData.DefaultCost;

    /// <summary>「磨爪器」层数（卡牌文本字面量：「施加5层『磨爪器』」）。</summary>
    private const int ClawSharpenersStacks = 5;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(9, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost),
        HoverTipFactory.FromPower<ClawSharpenersPower>()
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);

        // 喧响：对敌方全体施加5层「磨爪器」
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            var combatState = CombatState;
            if (combatState == null) return;

            foreach (var enemy in combatState.Creatures
                         .Where(c => c.Side == CombatSide.Enemy && c.IsAlive))
            {
                await ApplyPowerTo<ClawSharpenersPower>(
                    choiceContext, enemy, ClawSharpenersStacks, cardPlay.Card);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

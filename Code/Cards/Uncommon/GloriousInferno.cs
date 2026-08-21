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

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     纵享盛焰 —— 2费技能，获得 {Block} 点格挡。
///     喧响：对敌方全体施加3层「灼焰」。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class GloriousInferno() : ZZZBaseCard(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true
), IDecibelCardSource
{
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(9, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost),
        HoverTipFactory.FromPower<ScorchingPower>()
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得格挡
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);

        // 喧响：对敌方全体施加3层灼焰
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            var combatState = CombatState;
            if (combatState != null)
            {
                foreach (var enemy in combatState.Enemies)
                {
                    if (!enemy.IsAlive) continue;
                    await ApplyPowerTo<ScorchingPower>(choiceContext, enemy, 3, cardPlay.Card);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

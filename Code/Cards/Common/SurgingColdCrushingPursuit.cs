using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     溯寒追碾 —— 0费技能，消耗。
///     失去 2 点生命。获得 {Magic} 层临时力量（原版力量，回合结束自动移除）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SurgingColdCrushingPursuit() : ZZZBaseCard(
    0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
{
    /// <summary>「失去2点生命」的固定值（卡牌文本字面量）。</summary>
    private const int SelfHpLoss = 2;

    /// <summary>流失生命属性：不视为伤害，不受任何伤害加成/减免影响。</summary>
    private static readonly ValueProp HpLossProps =
        ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 3m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<SurgingColdCrushingPursuitPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 失去2点生命（流失生命语义，卡牌文本字面量）
        await CreatureCmd.Damage(choiceContext, Owner.Creature, SelfHpLoss, HpLossProps, this, cardPlay);

        // 获得 {Magic} 层临时力量
        await ApplyPowerSelf<SurgingColdCrushingPursuitPower>(
            choiceContext, DynamicVars["Magic"].IntValue, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);   // 强魔列
    }
}

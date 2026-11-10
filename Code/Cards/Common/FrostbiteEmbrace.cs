using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     霜寒拥覆 —— 1费攻击，喧响。
///     失去 2 点生命。目标流失 {Damage} 点生命。
///     [gold]喧响[/gold]：获得 {Magic} 层再生（原版能力）。
///     流失生命：直接扣除生命值，不视为伤害，不受任何伤害加成/减免影响。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class FrostbiteEmbrace() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true
), IDecibelCardSource
{
    /// <summary>喧响消耗（IDecibelCardSource 必须显式声明）。</summary>
    public int DecibelCost => DecibelData.DefaultCost;

    /// <summary>「失去2点生命」的固定值（卡牌文本字面量）。</summary>
    private const int SelfHpLoss = 2;

    /// <summary>流失生命属性：不视为伤害，不受任何伤害加成/减免影响。</summary>
    private static readonly ValueProp HpLossProps =
        ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move;

    /// <summary>流失生命不视为攻击伤害，不累积失衡。</summary>
    public override int DazeAmount => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(20m, HpLossProps),
        new DynamicVar("Magic", 2m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // ① 自身失去2点生命（流失生命语义，卡牌文本字面量）
        await CreatureCmd.Damage(choiceContext, Owner.Creature, SelfHpLoss, HpLossProps, this, cardPlay);

        // ② 目标流失 {Damage} 点生命
        await CreatureCmd.Damage(choiceContext, target, DynamicVars.Damage.BaseValue, HpLossProps, this, cardPlay);

        // ③ 喧响：获得 {Magic} 层再生（原版能力）
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
            await ApplyPowerSelf<RegenPower>(choiceContext, DynamicVars["Magic"].IntValue, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);     // 强伤列
        DynamicVars["Magic"].UpgradeValueBy(1m);  // 强魔列
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     摧岳 —— 1费攻击。
///     使目标流失 {Damage} 点生命。目标每有 5 点格挡，额外流失 {Magic} 点生命。
///     流失生命：直接扣除生命值，不视为伤害，不受任何伤害加成/减免影响。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class CrushingPeaks() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
{
    /// <summary>「每有5点格挡」的除数（卡牌文本字面量）。</summary>
    private const int BlockPerExtraLoss = 5;

    /// <summary>流失生命属性：不视为伤害，不受任何伤害加成/减免影响。</summary>
    private static readonly ValueProp HpLossProps =
        ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move;

    /// <summary>流失生命不视为攻击伤害，不累积失衡。</summary>
    public override int DazeAmount => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("Magic", 6m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 使目标流失 {Damage} 点生命
        await CreatureCmd.Damage(choiceContext, target, DynamicVars.Damage.BaseValue, HpLossProps, this, cardPlay);

        // 目标每有5点格挡，额外流失 {Magic} 点生命（5点整除，取目标结算主效果前的格挡）
        int extraTimes = target.Block / BlockPerExtraLoss;
        if (extraTimes > 0)
        {
            decimal extraLoss = extraTimes * DynamicVars["Magic"].BaseValue;
            await CreatureCmd.Damage(choiceContext, target, extraLoss, HpLossProps, this, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);     // 强伤列
        DynamicVars["Magic"].UpgradeValueBy(2m);  // 强魔列
    }
}

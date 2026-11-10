using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     霜天 —— 2费攻击，喧响。
///     对目标造成 {Damage} 点伤害。每剩余1点能量，额外造成 {Magic} 点伤害。
///     喧响：本卡造成的伤害翻倍（基础+能量加成合计×2，CSV 已明确为「本卡造成的伤害」）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class FrozenHeavens() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true
), IDecibelCardSource
{
    /// <summary>喧响消耗（IDecibelCardSource 必须显式声明）。</summary>
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("Magic", 5m)   // 每剩余1点能量的额外伤害
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 每剩余1点能量，额外造成 {Magic} 点伤害（能量按结算时剩余值，即支付费用后）
        int remainingEnergy = Owner.PlayerCombatState?.Energy ?? 0;
        decimal total = DynamicVars.Damage.BaseValue
                        + remainingEnergy * DynamicVars["Magic"].BaseValue;

        // 喧响：本卡造成的伤害翻倍（全部伤害×2）
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
            total *= 2m;

        await DealDamageRaw(choiceContext, total, target, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);      // 强伤列
        DynamicVars["Magic"].UpgradeValueBy(2m);   // 强魔列
    }
}

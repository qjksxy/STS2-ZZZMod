using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     永冬狂宴 —— 2费攻击，造成 {Damage} 点伤害。
///     喧响：消耗 20 点喧响，额外造成 {Magic} 点伤害。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class EndlessWinter() : ZZZBaseCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true), IDecibelCardSource
{
    /// <summary>喧响消耗，默认 20。</summary>
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("Magic", 12m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 喧响：额外造成 Magic 点伤害
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
            await DealDamageRaw(choiceContext, DynamicVars["Magic"].BaseValue, target, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Magic"].UpgradeValueBy(4m);
    }
}

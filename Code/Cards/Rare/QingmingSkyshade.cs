using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Rare;

/// <summary>
///     青溟云影 —— 2费技能，造成 {Damage} 点伤害。
///     若喧响值达到上限（≥50），打出时激活喧响效果且不消耗喧响值。
///     喧响：回复 {Magic} 点生命。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class QingmingSkyshade() : ZZZBaseCard(
    2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true
), IDecibelCardSource
{
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(18, ValueProp.Move),
        new DynamicVar("Magic", 4m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 造成伤害
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 喧响效果：回复生命
        // 若喧响值达到上限，免费激活；否则正常消耗
        bool decibelActivated;
        if (DecibelSystem.GetValue() >= DecibelData.MaxValue)
        {
            // 喧响值满 → 免费激活，不消耗喧响值
            decibelActivated = true;
        }
        else
        {
            // 喧响值未满 → 尝试消耗喧响值
            decibelActivated = DecibelSystem.TrySpendDecibel(DecibelCost);
        }

        if (decibelActivated)
        {
            var healAmount = DynamicVars["Magic"].BaseValue;
            await CreatureCmd.Heal(Owner.Creature, healAmount);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["Magic"].UpgradeValueBy(2m);
    }
}

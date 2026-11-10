using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     彻骨磔罚 —— 1费技能，目标失去 {Magic} 点力量和 {Magic} 点敏捷。
///     喧响：你获得其中一半（向上取整）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class GlaciatingImpalement() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true
), IDecibelCardSource
{
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 3m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        int magic = DynamicVars["Magic"].IntValue;

        // 目标失去 Magic 点力量和 Magic 点敏捷。即使目标原本没有对应属性，
        // 也直接扣减为负值（例如 0 力量 / 1 敏捷 → -3 力量 / -2 敏捷）。
        await ApplyPowerTo<StrengthPower>(choiceContext, target, -magic, cardPlay.Card);
        await ApplyPowerTo<DexterityPower>(choiceContext, target, -magic, cardPlay.Card);

        // 喧响：你获得其中一半（向上取整）
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            int half = (magic + 1) / 2;
            await ApplyPowerSelf<StrengthPower>(choiceContext, half, cardPlay.Card);
            await ApplyPowerSelf<DexterityPower>(choiceContext, half, cardPlay.Card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);   // 强魔列
    }
}

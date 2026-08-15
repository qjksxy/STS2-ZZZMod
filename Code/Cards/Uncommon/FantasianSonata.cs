using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     《幻想式奏鸣》 —— 2费技能，获得 {Magic} 层敏捷。
///     喧响：消耗 20 点喧响，额外获得 1 层敏捷。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class FantasianSonata() : ZZZBaseCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true), IDecibelCardSource
{
    /// <summary>喧响消耗，默认 20。</summary>
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 2m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyPowerSelf<DexterityPower>(choiceContext, DynamicVars["Magic"].IntValue, cardPlay.Card);

        // 喧响：额外获得 1 层敏捷
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
            await ApplyPowerSelf<DexterityPower>(choiceContext, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

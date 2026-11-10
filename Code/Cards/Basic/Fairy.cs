using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Character;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Basic;

/// <summary>
///     Fairy —— 1费技能，获得 {Magic} 点喧响。
///     喧响：获得1点力量。
///     与打击、防御一同作为初始卡牌。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
[RegisterCharacterStarterCard(typeof(ZZZCharacter), 1)]
public sealed class Fairy() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Basic, TargetType.Self, true
), IDecibelCardSource
{
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 5m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得喧响
        DecibelSystem.GainDecibel(DynamicVars["Magic"].IntValue);

        // 喧响：消耗喧响，获得1点力量
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            await ApplyPowerSelf<StrengthPower>(choiceContext, 1, cardPlay.Card);
        }
    }

    protected override void OnUpgrade()
    {
        // 升级只提高获得的喧响：5 → 8
        DynamicVars["Magic"].UpgradeValueBy(3m);
    }
}

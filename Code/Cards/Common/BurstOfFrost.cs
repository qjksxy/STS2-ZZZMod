using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     霜进 —— 1费技能。抽 {Magic} 张牌。
///     若手牌包含至少3种不同类别，获得能量和1层敏捷。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class BurstOfFrost() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
{
    /// <summary>类别种类门槛（卡牌文本字面量：「至少3种不同类别」）。</summary>
    private const int CategoryThreshold = 3;

    /// <summary>敏捷层数（卡牌文本字面量：「1层敏捷」）。</summary>
    private const int DexterityStacks = 1;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 抽 {Magic} 张牌
        await CardPileCmd.Draw(choiceContext, DynamicVars["Magic"].IntValue, Owner);

        // 抽牌后若手牌包含至少3种不同类别，获得能量和1层敏捷
        int categories = PileType.Hand.GetPile(Owner).Cards
            .Select(c => c.Type)
            .Distinct()
            .Count();

        if (categories >= CategoryThreshold)
        {
            await GainEnergy(1);
            await ApplyPowerSelf<DexterityPower>(choiceContext, DexterityStacks, cardPlay.Card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

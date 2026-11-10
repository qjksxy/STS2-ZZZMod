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

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     飞鸟鸣颂 —— 2费技能，获得 {Block} 点格挡。
///     喧响：目标失去3点力量。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SoaringBirdsSong() : ZZZBaseCard(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true
), IDecibelCardSource
{
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(18, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 获得格挡
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);

        // 喧响：目标（所选敌人）失去3点力量（字面量）。
        // 即使目标原本没有力量，也直接扣减为负值，与「彻骨磔罚」保持一致。
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            await ApplyPowerTo<StrengthPower>(choiceContext, target, -3, cardPlay.Card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(6);
    }
}

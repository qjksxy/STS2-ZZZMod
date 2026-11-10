using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     下山猛虎 —— 1费技能。
///     获得 {Block} 点格挡和 1 层「虎威」。本回合每打出 1 张技能牌，额外获得 1 层「虎威」。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class MountainDescendingSavageTiger() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
{
    /// <summary>「获得1层虎威」的层数（卡牌文本字面量）。</summary>
    private const int BaseHuWeiStacks = 1;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<HuWeiPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);

        // 获得1层「虎威」（卡牌文本字面量）
        await ApplyPowerSelf<HuWeiPower>(choiceContext, BaseHuWeiStacks, cardPlay.Card);

        // 本回合每打出1张技能牌，额外获得1层「虎威」（由配套能力实现）
        await ApplyPowerSelf<MountainDescendingSavageTigerPower>(choiceContext, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);   // 强防列
    }
}

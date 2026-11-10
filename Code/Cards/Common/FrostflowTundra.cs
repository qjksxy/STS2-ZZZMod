using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     流霜冻土 —— 1费技能。获得 {Block} 点格挡。
///     若手牌包含至少3种不同类别，额外获得 {Magic} 点格挡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class FrostflowTundra() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
{
    /// <summary>类别种类门槛（卡牌文本字面量：「至少3种不同类别」）。</summary>
    private const int CategoryThreshold = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8, ValueProp.Move),
        new DynamicVar("Magic", 4m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);

        // 若手牌包含至少3种不同类别，额外获得 {Magic} 点格挡
        int categories = PileType.Hand.GetPile(Owner).Cards
            .Select(c => c.Type)
            .Distinct()
            .Count();

        if (categories >= CategoryThreshold)
            await GainBlockRaw(DynamicVars["Magic"].BaseValue, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
        DynamicVars["Magic"].UpgradeValueBy(4m);
    }
}

using System.Collections.Generic;
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
///     可爱地雷飞天撞 —— 2费技能。获得 {Block} 点格挡。
///     若你已有格挡值，你的格挡值额外+50%（向下取整）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class AdorableExplosiveImpact() : ZZZBaseCard(
    2, CardType.Skill, CardRarity.Common, TargetType.Self, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(14, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 「若你已有格挡值」：打出前（获得基础格挡前）已有格挡
        bool hadBlock = Owner.Creature.Block > 0;

        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);

        // 格挡值额外+50%（整除即向下取整，按已定口径）
        if (hadBlock)
        {
            int bonus = Owner.Creature.Block / 2;
            if (bonus > 0)
                await GainBlockRaw(bonus, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }
}

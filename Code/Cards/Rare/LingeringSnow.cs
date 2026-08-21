using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Rare;

/// <summary>
///     名残雪 —— 2费能力，获得3层落霜。
///     同时施加被动能力：每打出技能牌+1层落霜，每对敌人施加负面效果+2层落霜。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class LingeringSnow() : ZZZBaseCard(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
{
    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<FallenFrostPower>(),
        HoverTipFactory.FromPower<LingeringSnowPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 施加落霜能力（计数+满6层生成霜月）
        await ApplyPowerSelf<FallenFrostPower>(choiceContext, 3, cardPlay.Card);

        // 检查是否达到6层阈值（首次施加3层时不太可能，但防御性检查）
        var fallenFrost = Owner?.Creature?.GetPower<FallenFrostPower>();
        if (fallenFrost != null)
            await fallenFrost.CheckAndConsume(choiceContext);

        // 施加名残雪被动能力（技能牌+1、负面+2）
        await ApplyPowerSelf<LingeringSnowPower>(choiceContext, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

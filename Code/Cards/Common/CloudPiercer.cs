using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     穿云 —— 1费攻击，若目标电壶层数少于3则施加 {Magic} 层电壶，
///     否则移除所有电壶并造成层数×5的伤害。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class CloudPiercer() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 4m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<ElectroQuiversPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        var eq = target.GetPower<ElectroQuiversPower>();
        int currentStacks = eq?.Amount ?? 0;

        if (currentStacks < 3)
        {
            // 施加电壶
            await ApplyPowerTo<ElectroQuiversPower>(
                choiceContext, target, DynamicVars["Magic"].IntValue, cardPlay.Card);
        }
        else
        {
            // 移除所有电壶，造成层数×5伤害
            if (eq != null)
                await PowerCmd.Remove(eq);

            await DealDamageRaw(choiceContext, currentStacks * 5, target, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Rare;

/// <summary>
///     时光碎裂 —— 2费技能，直接将目标失衡值归零，立即触发失衡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class ShatterTime() : ZZZBaseCard(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        var daze = DazeStore.Get(target);
        daze.CurrentValue = 0;
        daze.PendingDaze = true;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

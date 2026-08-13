using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Rare;

/// <summary>
///     时光碎裂 —— 2费技能，对目标造成999点失衡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class ShatterTime() : ZZZBaseCard(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;
        DazeStore.Get(target).AddDaze(999);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

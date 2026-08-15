using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     闪络 —— 1费技能，对敌人造成 {Magic} 点失衡。
///     若打出后目标失衡值未满，则将此牌返还手牌。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class FlashConnect() : ZZZBaseCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 4m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        var daze = DazeStore.Get(target);
        daze.AddDaze(DynamicVars["Magic"].IntValue);

        // 目标失衡未满 → 返还手牌（目标已满/已在失衡中则正常进弃牌堆）
        if (!daze.IsFull)
            await CardPileCmd.Add(cardPlay.Card, PileType.Hand);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(2m);
    }
}

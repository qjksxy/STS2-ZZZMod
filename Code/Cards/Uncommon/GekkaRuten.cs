using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     月华流转 —— 1费技能，若目标失衡值达到上限或处于失衡状态下，对目标施加 {Magic} 层易伤。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class GekkaRuten() : ZZZBaseCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 2m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 检查目标失衡状态
        if (DazeStore.TryGet(target, out var daze) && (daze.IsFull || daze.IsDazed))
        {
            await ApplyPowerTo<VulnerablePower>(choiceContext, target, DynamicVars["Magic"].IntValue, cardPlay.Card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

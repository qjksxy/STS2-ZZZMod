using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     流落星群 —— 1费技能，获得 {Block} 点格挡，使敌方全体当前失衡值翻倍。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class MeteorShower() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);

        // 使敌方全体当前失衡值翻倍（AddDaze 自带上限截断）
        var combatState = CombatState;
        if (combatState == null) return;

        foreach (var enemy in combatState.Enemies)
        {
            if (!enemy.IsAlive) continue;
            var daze = DazeStore.Get(enemy);
            daze.AddDaze(daze.CurrentValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);   // 强防列
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     暴君猛击 —— 1费技能，造成 {Damage} 点伤害。
///     若敌人处于格挡状态，则获得10点格挡并对其造成 {Magic} 点失衡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SavageSmash() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("Magic", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 在造成伤害之前检查敌人是否有格挡（伤害结算后格挡可能被消耗）
        bool hadBlock = target.Block > 0;

        // 造成伤害
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 若敌人在受到伤害前处于格挡状态，获得格挡并施加失衡
        if (hadBlock)
        {
            await GainBlockRaw(10, cardPlay);
            DazeStore.Get(target).AddDaze(DynamicVars["Magic"].IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

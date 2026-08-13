using MegaCrit.Sts2.Core.Commands;
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
///     八声甘州 —— 2费群体攻击，对敌方全体造成4点伤害并施加3点失衡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class EightSoundsOfGanzhou()
    : ZZZBaseCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
{
    // 禁用自动失衡，手动在 OnPlay 中施加
    public override int DazeAmount => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new DynamicVar("Magic", 3m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = CombatState;
        if (combatState == null) return;

        var damage = DynamicVars.Damage;
        var magic = DynamicVars["Magic"].IntValue;

        await DamageCmd.Attack(damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 对所有存活敌人施加失衡
        foreach (var enemy in combatState.Enemies)
        {
            if (!enemy.IsAlive) continue;
            DazeStore.Get(enemy).AddDaze(magic);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

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
///     醉花月云转 —— 1费攻击，造成6点伤害并施加3点失衡。
///     若此卡使敌人失衡值降至0，失衡易伤倍率额外+50%（总计100%）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class EnchantedMoonlitBlossoms()
    : ZZZBaseCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
{
    // 禁用自动失衡（hit listener），手动在 OnPlay 中施加
    public override int DazeAmount => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("Magic", 3m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 手动施加失衡（Magic 点）
        var magic = DynamicVars["Magic"].IntValue;
        var daze = DazeStore.Get(target);
        var reachedZero = daze.ReduceDaze(magic);

        // 若此卡使失衡值降至0，施加失衡易伤（可被人工作品抵消）
        if (reachedZero)
            await PowerCmd.Apply<DazeVulnerablePower>(choiceContext, target, 1, Owner.Creature, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

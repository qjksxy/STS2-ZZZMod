using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     裙裾浮游 —— 由「堇花悼亡」施加。
///     获得格挡时，使一名随机敌人失去1点力量。
/// </summary>
[RegisterPower]
public sealed class FlutteringFrockPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/fluttering_frock.png",
        BigIconPath: "res://ZZZMod/images/powers/fluttering_frock_big.png"
    );

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 只在玩家受到伤害时触发
        if (target != Owner) return;
        if (Amount <= 0) return;

        // 检查是否成功格挡（有格挡伤害且无穿透伤害）
        if (result.BlockedDamage > 0 && result.UnblockedDamage == 0)
        {
            // 使一名随机敌人失去1点力量
            var combatState = CombatState;
            if (combatState == null) return;

            var enemies = combatState.Enemies.Where(e => e.IsAlive).ToList();
            if (enemies.Count == 0) return;

            var randomEnemy = Owner.Player?.RunState?.Rng.CombatTargets.NextItem(enemies) ?? enemies[0];
            // 即使目标原本没有力量，也直接扣减为负值，与「彻骨磔罚」保持一致。
            await PowerCmd.Apply<StrengthPower>(choiceContext, randomEnemy, -1, Owner, null);
        }
    }
}

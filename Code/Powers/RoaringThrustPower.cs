using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Decibel;

namespace ZZZMod.Code.Powers;

/// <summary>
///     喧嚣直刺 —— 由「超强力盾击」施加。
///     回合内每次受到敌人攻击并成功格挡时，获得3点喧响值。
///     每回合减少1层。效果不随层数叠加。
/// </summary>
[RegisterPower]
public sealed class RoaringThrustPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/roaring_thrust.png",
        BigIconPath: "res://ZZZMod/images/powers/roaring_thrust_big.png"
    );

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner) return;
        if (dealer == null || dealer.Side != CombatSide.Enemy) return;
        if (Amount <= 0) return;

        // 成功格挡（有格挡伤害且无穿透伤害）
        if (result.BlockedDamage > 0 && result.UnblockedDamage == 0)
        {
            // 固定获得3点喧响，不随层数叠加
            DecibelSystem.GainDecibel(3);
        }
    }

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 每回合减少1层
        if (side != Owner.Side || !participants.Contains(Owner)) return;

        await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -1, Owner, null);

        if (Amount <= 0)
            await PowerCmd.Remove(this);
    }
}

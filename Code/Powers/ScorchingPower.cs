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

namespace ZZZMod.Code.Powers;

/// <summary>
///     灼焰 —— 由「炽焰搅拌式」和「纵享盛焰」施加。
///     效果持续期间，每受到1次攻击，则受到5点伤害。每回合减少1层。
/// </summary>
[RegisterPower]
public sealed class ScorchingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/scorching.png",
        BigIconPath: "res://ZZZMod/images/powers/scorching_big.png"
    );

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 只在拥有者受到攻击时触发（自身受到伤害时额外受到5点伤害）
        if (target != Owner) return;
        if (dealer == null || !dealer.IsAlive) return;

        // 受到5点伤害（可被格挡）
        await CreatureCmd.Damage(choiceContext, Owner, 5, ValueProp.Move, null, null, null);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 每回合减少1层
        if (side != Owner.Side || !participants.Contains(Owner)) return;

        await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -1, Owner, null);

        if (Amount <= 0)
            await PowerCmd.Remove(this);
    }
}

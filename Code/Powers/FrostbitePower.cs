using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Cards.Common;

namespace ZZZMod.Code.Powers;

/// <summary>
///     急冻 —— 由「急冻修剪法」施加。
///     敌人下回合开始时受到记录的伤害，每层触发一次，结算后移除。
///     延迟伤害不走攻击流程，因此不累积失衡值。
/// </summary>
[RegisterPower]
public sealed class FrostbitePower : ModPowerTemplate
{
    private class Data
    {
        public int DamagePerHit;
    }

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/frostbite.png",
        BigIconPath: "res://ZZZMod/images/powers/frostbite_big.png"
    );

    protected override object? InitInternalData() => new Data();

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        // 记录施放时卡牌的伤害值（后续叠加只增加层数，数值保持不变）
        if (cardSource is FlashFreezeTrimming card)
            GetInternalData<Data>().DamagePerHit = card.DynamicVars.Damage.IntValue;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side || !participants.Contains(Owner)) return;

        var damagePerHit = GetInternalData<Data>().DamagePerHit;
        for (var i = 0; i < Amount; i++)
        {
            if (!Owner.IsAlive) break;
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, damagePerHit, ValueProp.Move, Applier, null, null);
        }

        await PowerCmd.Remove(this);
    }
}

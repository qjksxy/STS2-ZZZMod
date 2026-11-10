using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     「乱琼」配套能力：下回合你的第一张攻击牌额外造成 {Amount} 点伤害。
///     实现沿用原版 VigorPower 模式：BeforeAttack 记账首张攻击、ModifyDamageAdditive 加伤、
///     AfterAttack 结算移除（修改器无副作用，对伤害预览安全）。
/// </summary>
[RegisterPower]
public sealed class ScatteredJadePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/scattered_jade.png",
        BigIconPath: "res://ZZZMod/images/powers/scattered_jade_big.png"
    );

    private class Data
    {
        public AttackCommand? CommandToModify;
        public bool Armed;
    }

    protected override object? InitInternalData() => new Data();

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 下回合起生效
        if (side != Owner.Side || !participants.Contains(Owner)) return;
        GetInternalData<Data>().Armed = true;
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        var data = GetInternalData<Data>();
        if (!data.Armed || data.CommandToModify != null) return Task.CompletedTask;
        if (command.Attacker != Owner) return Task.CompletedTask;
        if (!command.DamageProps.IsPoweredAttack()) return Task.CompletedTask;
        // 仅卡牌来源的攻击牌（遗物/能力触发的非卡牌攻击不占「第一张攻击牌」名额）
        if (command.ModelSource is not CardModel card || card.Type != CardType.Attack)
            return Task.CompletedTask;

        data.CommandToModify = command;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        var data = GetInternalData<Data>();
        if (data.CommandToModify == null) return 0m;
        if (Owner != dealer) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        if (cardSource != null && cardSource != data.CommandToModify.ModelSource) return 0m;
        return Amount;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        var data = GetInternalData<Data>();
        if (command == data.CommandToModify)
            await PowerCmd.Remove(this);
    }
}

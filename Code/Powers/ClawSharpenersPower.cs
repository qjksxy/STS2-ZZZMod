using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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
///     磨爪器 —— 由「特别拍照技巧」「通通砸碎！」施加。
///     目标每次受到攻击时额外受到 3 点伤害，每生效 1 次减少 1 层。
///     层数表示剩余触发次数。
/// </summary>
[RegisterPower]
public sealed class ClawSharpenersPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/claw_sharpeners.png",
        BigIconPath: "res://ZZZMod/images/powers/claw_sharpeners_big.png"
    );

    /// <summary>每次触发的额外伤害（能力文本字面量：「额外受到3点伤害」）。</summary>
    private const int BonusDamage = 3;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner) return;
        if (dealer == null || !dealer.IsAlive) return;
        // 仅「受到攻击」触发：非攻击/「流失生命」（Unpowered）扣血不触发
        if (!props.IsPoweredAttack()) return;
        if (Amount <= 0) return;

        // 额外受到3点伤害（可被格挡，无来源，不再次触发本能力）
        await CreatureCmd.Damage(choiceContext, Owner, BonusDamage, ValueProp.Move, null, null, null);

        // 每生效1次减少1层
        await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -1, Owner, null);
        if (Amount <= 0)
            await PowerCmd.Remove(this);
    }
}

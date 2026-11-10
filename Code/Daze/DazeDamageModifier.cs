using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡伤害修正：失衡状态受伤 ×1.5，有 DazeVulnerablePower 时 ×2.0。
///     不经过 Power 系统，不受人工制品影响。
///     DazeVulnerablePower 本身是标准 Power，可被人工作品抵消。
/// </summary>
[RegisterSingleton]
public sealed class DazeDamageModifier : HookedSingletonModel
{
    public DazeDamageModifier() : base(HookType.Combat) { }

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target == null || !DazeSystem.IsDazed(target))
            return 1m;

        // 「流失生命」等非伤害管线扣血（Unpowered）不受伤害加成影响
        if (props.HasFlag(ValueProp.Unpowered))
            return 1m;

        // 基础 1.5x，有失衡易伤 Power 时提升为 2.0x
        return target.HasPower<DazeVulnerablePower>() 
            ? DazeSystem.DazeVulnerableDamageMultiplier 
            : DazeSystem.DazeDamageMultiplier;
    }
}

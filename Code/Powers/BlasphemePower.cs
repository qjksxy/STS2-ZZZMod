using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Daze;

namespace ZZZMod.Code.Powers;

/// <summary>
///     渎神者 —— 由「渎神者」卡牌施加。
///     敌人失衡值达到上限时，获得1层力量。
/// </summary>
[RegisterPower]
public sealed class BlasphemePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/blaspheme.png",
        BigIconPath: "res://ZZZMod/images/powers/blaspheme_big.png"
    );

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy) return;
        if (Amount <= 0) return;

        // 检查所有敌人是否失衡值达到上限
        foreach (var creature in participants)
        {
            if (creature.Side != CombatSide.Enemy) continue;
            if (!creature.IsAlive) continue;

            if (DazeStore.TryGet(creature, out var daze) && daze.IsFull)
            {
                // 获得1层力量
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
                break; // 只触发一次
            }
        }
    }
}

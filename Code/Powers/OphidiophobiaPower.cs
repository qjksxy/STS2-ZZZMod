using System;
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
using ZZZMod.Code.Daze;

namespace ZZZMod.Code.Powers;

/// <summary>
///     「爬行恐惧」配套能力：打出此牌后，本回合每造成 1 点失衡，额外获得 1 点格挡。
///     监听 DazeStore.DazeAdded 事件；持续到玩家下回合开始（覆盖本回合内所有失衡来源）。
/// </summary>
[RegisterPower]
public sealed class OphidiophobiaPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/ophidiophobia.png",
        BigIconPath: "res://ZZZMod/images/powers/ophidiophobia_big.png"
    );

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        DazeStore.DazeAdded += OnDazeAdded;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        DazeStore.DazeAdded -= OnDazeAdded;
        return Task.CompletedTask;
    }

    private async void OnDazeAdded(Creature creature, int amount)
    {
        try
        {
            // 防御：战斗清理存在跳过 AfterRemoved 的路径，残留实例自行退订
            if (!Owner.Powers.Contains(this))
            {
                DazeStore.DazeAdded -= OnDazeAdded;
                return;
            }

            // 每造成1点失衡，额外获得1点格挡
            if (amount <= 0) return;
            await CreatureCmd.GainBlock(Owner, amount, ValueProp.Unpowered, null);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Powers] 爬行恐惧触发出错: {ex.Message}");
        }
    }

    public override async Task AfterSideTurnStart(
        CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        // 本回合限定：玩家下回合开始时移除
        if (side != Owner.Side || !participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}

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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Daze;

namespace ZZZMod.Code.Powers;

/// <summary>
///     虎威 —— 由「燎身爪」「下山猛虎」等卡牌施加。
///     你的回合结束时，对随机敌人施加 2 点失衡，然后失去 1 层虎威。
///     层数表示剩余触发次数（持续回合数）。
/// </summary>
[RegisterPower]
public sealed class HuWeiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/hu_wei.png",
        BigIconPath: "res://ZZZMod/images/powers/hu_wei_big.png"
    );

    /// <summary>每次触发施加的失衡点数（能力文本字面量：「施加2点失衡」）。</summary>
    private const int DazePerTrigger = 2;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        if (!participants.Contains(Owner)) return;
        if (Amount <= 0) return;

        var enemies = Owner.CombatState?.Creatures
            .Where(c => c.Side == CombatSide.Enemy && c.IsAlive)
            .ToList();

        if (enemies is { Count: > 0 })
        {
            var player = Owner.Player;
            var enemy = player?.RunState?.Rng.CombatTargets.NextItem(enemies) ?? enemies[0];
            DazeStore.Get(enemy).AddDaze(DazePerTrigger);
        }

        // 失去1层虎威
        await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -1, Owner, null);
        if (Amount <= 0)
            await PowerCmd.Remove(this);
    }
}

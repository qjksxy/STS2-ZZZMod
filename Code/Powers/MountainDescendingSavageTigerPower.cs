using System.Collections.Generic;
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

namespace ZZZMod.Code.Powers;

/// <summary>
///     「下山猛虎」配套能力：本回合每打出 1 张技能牌（非连携自动打出），额外获得 1 层虎威。
///     本回合限定（回合结束时移除）。打出「下山猛虎」本身的那一次不计数（避免与基础1层重复）。
/// </summary>
[RegisterPower]
public sealed class MountainDescendingSavageTigerPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/mountain_descending_savage_tiger.png",
        BigIconPath: "res://ZZZMod/images/powers/mountain_descending_savage_tiger_big.png"
    );

    private class Data
    {
        public bool SelfPlaySeen;
    }

    protected override object? InitInternalData() => new Data();

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner.Player) return;

        var data = GetInternalData<Data>();

        // 施放此能力的那次出牌本身不计数
        if (!data.SelfPlaySeen)
        {
            data.SelfPlaySeen = true;
            return;
        }

        if (cardPlay.IsAutoPlay) return;
        if (cardPlay.Card == null || cardPlay.Card.Type != CardType.Skill) return;

        // 额外获得1层虎威
        await PowerCmd.Apply<HuWeiPower>(choiceContext, Owner, 1, Owner, null);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}

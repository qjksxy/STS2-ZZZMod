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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Cards.Uncommon;

namespace ZZZMod.Code.Powers;

/// <summary>
///     风铃与旧约 —— 由《风铃与旧约》施加。
///     本回合结束（玩家回合末）时，若本回合已打出牌数 ≥ 门槛，则额外获得 1 层力量，随后移除。
/// </summary>
[RegisterPower]
public sealed class WindchimesAndOathsPower : ModPowerTemplate
{
    private class Data
    {
        public int Threshold;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/windchimes_and_oaths.png",
        BigIconPath: "res://ZZZMod/images/powers/windchimes_and_oaths_big.png"
    );

    protected override object? InitInternalData() => new Data();

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        // 记录施放时卡牌的触发门槛（Magic：5，升级后 4）
        if (cardSource is WindchimesAndOaths card)
            GetInternalData<Data>().Threshold = card.DynamicVars["Magic"].IntValue;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;

        // 本回合已打出的牌数（含此牌）
        var played = CombatManager.Instance.History.CardPlaysStarted
            .Count(e => e.HappenedThisTurn(CombatState) && e.CardPlay.Player == Owner.Player);

        if (played >= GetInternalData<Data>().Threshold)
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 1, Owner, null);

        await PowerCmd.Remove(this);
    }
}

using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Chain;

namespace ZZZMod.Code.Powers;

/// <summary>
///     极雷断空 —— 由「极雷断空」卡牌施加。
///     触发连携时，获得记录的格挡值。
/// </summary>
[RegisterPower]
public sealed class SunderingBoltPower : ModPowerTemplate
{
    private class Data
    {
        public int BlockPerTrigger;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/sundering_bolt.png",
        BigIconPath: "res://ZZZMod/images/powers/sundering_bolt_big.png"
    );

    protected override object? InitInternalData() => new Data();

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        // 从施放卡牌读取每次触发的格挡值
        if (cardSource != null && cardSource.DynamicVars != null)
        {
            var magic = cardSource.DynamicVars["Magic"];
            if (magic != null)
                GetInternalData<Data>().BlockPerTrigger = magic.IntValue;
        }

        // 订阅连携触发事件
        ChainSystem.ChainTriggered += OnChainTriggered;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        // 取消订阅
        ChainSystem.ChainTriggered -= OnChainTriggered;
        return Task.CompletedTask;
    }

    private async void OnChainTriggered(CardModel triggeredCard)
    {
        if (Amount <= 0) return;

        var blockPerTrigger = GetInternalData<Data>().BlockPerTrigger;
        if (blockPerTrigger <= 0) return;

        // GainBlock via CreatureCmd uses BlockVar from DynamicVars namespace.
        // We cast to decimal for the BlockVar constructor.
        await CreatureCmd.GainBlock(Owner,
            new MegaCrit.Sts2.Core.Localization.DynamicVars.BlockVar(
                (decimal)blockPerTrigger, ValueProp.Move), null);
    }
}

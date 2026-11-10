using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Chain;

namespace ZZZMod.Code.Powers;

/// <summary>
///     浮晖 —— 由「曙色颂·转辉」施加。
///     触发连携时，抽 1 张牌并获得能量（1 点）。
///     不可叠加。
/// </summary>
[RegisterPower]
public sealed class FloatingRadiancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/floating_radiance.png",
        BigIconPath: "res://ZZZMod/images/powers/floating_radiance_big.png"
    );

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
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
        try
        {
            // 防御：战斗清理存在跳过 AfterRemoved 的路径，残留实例自行退订
            if (!Owner.Powers.Contains(this))
            {
                ChainSystem.ChainTriggered -= OnChainTriggered;
                return;
            }

            var player = Owner.Player;
            if (player == null) return;

            await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, player);
            await PlayerCmd.GainEnergy(1, player);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Powers] 浮晖触发出错: {ex.Message}");
        }
    }
}

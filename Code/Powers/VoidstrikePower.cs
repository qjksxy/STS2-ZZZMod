using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Decibel;

namespace ZZZMod.Code.Powers;

/// <summary>
///     斩空掠电 —— 由「斩空掠电」卡牌施加。
///     触发连携时，获得 5 点喧响。不可叠加。
/// </summary>
[RegisterPower]
public sealed class VoidstrikePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/voidstrike.png",
        BigIconPath: "res://ZZZMod/images/powers/voidstrike_big.png"
    );

    /// <summary>每次触发获得的喧响值（卡牌文本字面量：「获得5点喧响」）。</summary>
    private const int DecibelPerTrigger = 5;

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

    private void OnChainTriggered(CardModel triggeredCard)
    {
        try
        {
            DecibelSystem.GainDecibel(DecibelPerTrigger);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Powers] 斩空掠电触发出错: {ex.Message}");
        }
    }
}

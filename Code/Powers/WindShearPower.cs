using System;
using System.Threading.Tasks;
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
///     「风切变」配套能力：你的下一张攻击牌费用-1。
///     对所有攻击牌生效显示，实际打出（非连携自动打出）1 张攻击牌后移除。
/// </summary>
[RegisterPower]
public sealed class WindShearPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/wind_shear.png",
        BigIconPath: "res://ZZZMod/images/powers/wind_shear_big.png"
    );

    public override bool TryModifyEnergyCostInCombat(
        CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Type != CardType.Attack) return false;
        // 费用-1（不低于0）
        modifiedCost = Math.Max(0m, originalCost - 1m);
        return true;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner.Player) return;
        // 连携/随机免费自动打出不消耗减费
        if (cardPlay.IsAutoPlay) return;
        if (cardPlay.Card == null || cardPlay.Card.Type != CardType.Attack) return;
        await PowerCmd.Remove(this);
    }
}

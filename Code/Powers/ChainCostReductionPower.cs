using System.Collections.Generic;
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
using ZZZMod.Code.Chain;

namespace ZZZMod.Code.Powers;

/// <summary>
///     连携减费 —— 下 {Amount} 次抽到带有连携标签的卡牌时，其费用变为 0。
///     每次触发后计数 -1，归零时自动移除。
/// </summary>
[RegisterPower]
public sealed class ChainCostReductionPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/chain_cost_reduction.png",
        BigIconPath: "res://ZZZMod/images/powers/chain_cost_reduction_big.png"
    );

    /// <summary>
    ///     当卡牌被抽到手牌时触发。
    ///     检查是否为连携卡，如果是则将费用设为 0。
    /// </summary>
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        // 只处理抽到手牌的情况
        if (!fromHandDraw) return;
        
        // 检查卡牌是否实现连携接口
        if (card is not IChainCardSource) return;
        
        // 检查层数
        if (Amount <= 0) return;

        // 将费用设为 0（本次出牌有效）
        card.EnergyCost.SetThisTurnOrUntilPlayed(0);

        // 减少层数
        if (Owner != null)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -1m, Owner, null);
        }

        // 如果层数归零，移除自身
        if (Amount <= 0)
        {
            await PowerCmd.Remove(this);
        }
    }
}

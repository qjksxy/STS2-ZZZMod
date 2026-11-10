using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     电壶 —— 由「穿云」和「残心」施加。
///     抽牌时（从牌库抽到手牌），受到3点伤害。层数不随回合减少。
/// </summary>
[RegisterPower]
public sealed class ElectroQuiversPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/electro_quivers.png",
        BigIconPath: "res://ZZZMod/images/powers/electro_quivers_big.png"
    );

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        // 只在从牌库抽到手牌时触发（不含战斗开始时的初始抽牌等）
        if (!fromHandDraw) return;
        if (Amount <= 0) return;

        // 受到3点伤害（固定值，不随层数叠加）
        await CreatureCmd.Damage(choiceContext, Owner, 3, ValueProp.Move, null, null, null);
    }
}

using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     名残雪 —— 由「名残雪」卡牌施加的配套被动能力。
///     每打出1张技能牌，获得1层落霜；每对敌人施加负面效果，获得2层落霜。
///     增加落霜层数后自动检查是否达到6层阈值。
/// </summary>
[RegisterPower]
public sealed class LingeringSnowPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/lingering_snow.png",
        BigIconPath: "res://ZZZMod/images/powers/lingering_snow_big.png"
    );

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner.Player) return;

        var fallenFrost = Owner.GetPower<FallenFrostPower>();
        if (fallenFrost == null) return;

        // 每打出1张技能牌，获得1层落霜
        if (cardPlay.Card.Type == CardType.Skill)
        {
            await PowerCmd.ModifyAmount(choiceContext, fallenFrost, 1, Owner, cardPlay.Card);
            await fallenFrost.CheckAndConsume(choiceContext);
        }

        // 每对敌人施加负面效果，获得2层落霜
        // 判定：打出的牌目标是敌人且是技能类型（技能牌常用于施加 debuff）
        if (cardPlay.Target?.Side == CombatSide.Enemy && cardPlay.Card.Type == CardType.Skill)
        {
            await PowerCmd.ModifyAmount(choiceContext, fallenFrost, 2, Owner, cardPlay.Card);
            await fallenFrost.CheckAndConsume(choiceContext);
        }
    }
}

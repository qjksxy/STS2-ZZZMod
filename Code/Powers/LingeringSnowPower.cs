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

    /// <summary>
    ///     增加落霜层数，并在 Power 不存在时自动重建。
    ///     返回操作后的 FallenFrostPower 实例（可能为 null 如果 Apply 失败）。
    /// </summary>
    private async Task<FallenFrostPower?> AddFallenFrost(
        PlayerChoiceContext choiceContext, int amount, CardModel cardSource)
    {
        var existing = Owner.GetPower<FallenFrostPower>();
        if (existing != null)
        {
            await PowerCmd.ModifyAmount(choiceContext, existing, amount, Owner, cardSource);
            await existing.CheckAndConsume(choiceContext);
            return existing;
        }
        else
        {
            // Power 已被移除（消耗至0），重新施加来恢复
            var recreated = await PowerCmd.Apply<FallenFrostPower>(
                choiceContext, Owner, amount, Owner, cardSource);
            if (recreated != null)
                await recreated.CheckAndConsume(choiceContext);
            return recreated;
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner.Player) return;

        // 每打出1张技能牌，获得1层落霜
        if (cardPlay.Card.Type == CardType.Skill)
        {
            await AddFallenFrost(choiceContext, 1, cardPlay.Card);
        }

        // 每对敌人施加负面效果，获得2层落霜
        if (cardPlay.Target?.Side == CombatSide.Enemy && cardPlay.Card.Type == CardType.Skill)
        {
            await AddFallenFrost(choiceContext, 2, cardPlay.Card);
        }
    }
}

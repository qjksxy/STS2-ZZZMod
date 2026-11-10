using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     进击，大铠甲！ —— 1费攻击，喧响。
///     若你有格挡值：消耗全部格挡值，对目标造成等同于消耗值两倍的伤害；
///     否则：获得 {Block} 点格挡。
///     [gold]喧响[/gold]：造成的伤害提升50%（仅伤害分支消耗喧响，见 CSV 备注）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class ChargeGreatArmor() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true
), IDecibelCardSource
{
    /// <summary>喧响消耗（IDecibelCardSource 必须显式声明）。</summary>
    public int DecibelCost => DecibelData.DefaultCost;

    /// <summary>基础伤害倍率：消耗格挡值 ×2（卡牌文本字面量）。</summary>
    private const int DamageMultiplier = 2;

    /// <summary>喧响强化后倍率：×3 = 提升50%，保证整数精确（卡牌文本字面量）。</summary>
    private const int DamageMultiplierWithDecibel = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(9, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        int currentBlock = Owner.Creature.Block;
        if (currentBlock > 0)
        {
            // 消耗全部格挡值
            await CreatureCmd.LoseBlock(choiceContext, Owner.Creature, currentBlock, Owner.Creature);

            // 对目标造成等同于消耗值两倍的伤害
            int multiplier = DamageMultiplier;

            // 喧响：造成的伤害提升50%（备注：仅伤害分支消耗喧响，避免无效消耗）
            if (DecibelSystem.TrySpendDecibel(DecibelCost))
                multiplier = DamageMultiplierWithDecibel;

            await DealDamageRaw(choiceContext, currentBlock * multiplier, target, cardPlay);
        }
        else
        {
            // 无格挡分支：获得 {Block} 点格挡，不消耗喧响（CSV 备注）
            await GainBlockRaw(DynamicVars.Block.BaseValue, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);   // 强防列
    }
}

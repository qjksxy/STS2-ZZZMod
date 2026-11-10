using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     绝对音准 —— 1费攻击，对目标造成 {Damage} 点伤害并获得1层「应援能量」。
///     已有 {Magic} 层「应援能量」时：消耗 {Magic} 层，获得能量并抽1张牌。
///     「获得能量」= 1 点能量（既有口径）；应援能量（Fandom Power）见 FandomPower。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class PerfectPitch() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("Magic", 3m)   // 消耗门槛
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<FandomPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 对目标造成 {Damage} 点伤害
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 获得1层「应援能量」（卡牌文本字面量：1层）
        await ApplyPowerSelf<FandomPower>(choiceContext, 1, cardPlay.Card);

        // 已有 {Magic} 层「应援能量」时（含本卡刚获得的1层，按原版先加后判惯例）：
        int threshold = DynamicVars["Magic"].IntValue;
        if (threshold <= 0 || GetPowerAmount<FandomPower>() < threshold) return;

        // 消耗 {Magic} 层，获得能量并抽1张牌
        await ModifyPower<FandomPower>(choiceContext, Owner.Creature, -threshold, cardPlay.Card);
        await GainEnergy(1);                       // 「获得能量」= 1 点能量
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);      // 强伤列
        DynamicVars["Magic"].UpgradeValueBy(-1m);  // 强魔列（-1：降低消耗门槛）
    }
}

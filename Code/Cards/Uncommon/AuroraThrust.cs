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
using ZZZMod.Code.Daze;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     极光突刺 —— 1费攻击，先获得1层「剑仪」（至多3层），
///     再依据剑仪层数触发对应增益（只触发当前层数对应的一项），最后造成伤害。
///     1层：降低目标2点失衡上限（至多降低至4）；2层：抽1张牌；3层：回复1点能量。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class AuroraThrust() : ZZZBaseCard(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<BladeEtiquettePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 1. 获得1层「剑仪」（至多累积3层）
        int stacks = GetPowerAmount<BladeEtiquettePower>();
        if (stacks < 3)
        {
            await ApplyPowerSelf<BladeEtiquettePower>(choiceContext, 1, cardPlay.Card);
            stacks++;
        }

        // 2. 依据剑仪层数触发对应增益（只触发当前层数对应的一项）
        if (stacks == 1)
        {
            // 1层：降低目标2点失衡上限（至多降低至4）
            var daze = DazeStore.Get(target);
            daze.MaxValue = Math.Max(4, daze.MaxValue - 2);
            daze.CurrentValue = Math.Min(daze.CurrentValue, daze.MaxValue);
        }
        else if (stacks == 2)
        {
            // 2层：抽1张牌
            await CardPileCmd.Draw(choiceContext, 1m, Owner);
        }
        else if (stacks >= 3)
        {
            // 3层：回复1点能量
            await GainEnergy(1);
        }

        // 3. 造成伤害（先获得剑仪，再触发伤害）
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);   // 强伤列
    }
}

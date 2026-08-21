using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Rare;

/// <summary>
///     飞雪 —— 1费攻击，造成 {Damage} 点伤害，获得 {Magic} 层落霜。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class Hisetsu() : ZZZBaseCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move),
        new DynamicVar("Magic", 2m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<FallenFrostPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 施加落霜层数
        await ApplyPowerSelf<FallenFrostPower>(choiceContext, DynamicVars["Magic"].IntValue, cardPlay.Card);

        // 检查是否达到6层阈值
        var fallenFrost = Owner?.Creature?.GetPower<FallenFrostPower>();
        if (fallenFrost != null)
            await fallenFrost.CheckAndConsume(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

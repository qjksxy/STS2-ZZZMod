using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

[RegisterCard(typeof(ZZZCardPool))]
// [RegisterCharacterStarterCard(typeof(TestCharacter), 5)]
public class TestCard() : ZZZBaseCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
{
    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(ZZZModKeywords.Overflow)
    ];
    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Overflow];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("BonusDamage", 4m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var t)) return;
        await DealDamage(choiceContext, DynamicVars.Damage, t);

        // 满盈：满血时额外造成伤害（效果由本卡自行定义，条件判定由基类处理）
        if (ShouldTriggerOverflow())
            await DealDamage(choiceContext, DynamicVars["BonusDamage"], t);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["BonusDamage"].UpgradeValueBy(2m);
    }

}
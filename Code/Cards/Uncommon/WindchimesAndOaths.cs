using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     《风铃与旧约》 —— 2费技能，获得 2 层力量。
///     若于本回合结束前打出至少 {Magic} 张牌，则额外获得 1 层力量（回合末结算）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class WindchimesAndOaths() : ZZZBaseCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 5m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<WindchimesAndOathsPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 立即获得 2 层力量（CSV 文本中的固定值）
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 2, Owner.Creature, cardPlay.Card);

        // 回合末达标判定由 WindchimesAndOathsPower 处理
        await PowerCmd.Apply<WindchimesAndOathsPower>(choiceContext, Owner.Creature, 1, Owner.Creature, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        // 升级：触发门槛 -1（5 张 → 4 张）
        DynamicVars["Magic"].UpgradeValueBy(-1m);
    }
}

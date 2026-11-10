using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;
using ZZZMod.Code.Powers;

namespace ZZZMod.Code.Cards.Uncommon;

/// <summary>
///     不投降就捣乱 —— 2费技能（消耗），为目标施加标记：
///     目标每次进入失衡状态后，失去1层力量。升级后不再消耗。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class SurrenderOrSufferTheMischief() : ZZZBaseCard(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;
        await ApplyPowerTo<SurrenderOrSufferTheMischiefPower>(choiceContext, target, 1, cardPlay.Card);
    }

    protected override void OnUpgrade()
    {
        // 升级后不再消耗
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

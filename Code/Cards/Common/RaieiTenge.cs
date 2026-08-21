using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     雷影天华 —— 1费技能，施加1层易伤。
///     喧响：若目标已有负面效果，则施加 {Magic} 层虚弱。
///     判定顺序：先检查目标是否有负面效果（在施加易伤之前），再施加易伤，最后决定是否消耗喧响施加虚弱。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class RaieiTenge() : ZZZBaseCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true), IDecibelCardSource
{
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 1m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 先检查目标是否已有负面效果（在施加易伤之前判定）
        bool hadDebuff = target.Powers.Any(p => p.Type == PowerType.Debuff);

        // 施加1层易伤（始终执行）
        await ApplyPowerTo<VulnerablePower>(choiceContext, target, 1, cardPlay.Card);

        // 喧响：若目标已有负面效果（施加易伤之前），则消耗喧响施加虚弱
        if (hadDebuff && DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            await ApplyPowerTo<WeakPower>(choiceContext, target, DynamicVars["Magic"].IntValue, cardPlay.Card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}

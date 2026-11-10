using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Rare;

/// <summary>
///     缭乱终幕 —— 1费技能，消耗手牌中的任意张牌，每消耗1张牌，对目标造成 {Magic} 点失衡。
///     任意张包含 0 张与全部（O2 口径）。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class DazzlingCurtainCall() : ZZZBaseCard(
    1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true)
{
    /// <summary>手牌选择上限（卡牌效果允许消耗任意张，含全部）。</summary>
    private const int MaxExhaustCount = 99;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Magic", 6m)   // 每消耗1张牌造成的失衡
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        // 消耗手牌中的任意张牌（0 张也合法），排除自身
        var selected = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 0, MaxExhaustCount),
            (CardModel c) => c != cardPlay.Card,
            this);

        foreach (var c in selected)
        {
            await CardCmd.Exhaust(choiceContext, c);
            // 每消耗1张牌，对目标造成 Magic 点失衡
            DazeStore.Get(target).AddDaze(DynamicVars["Magic"].IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(2m);   // 强魔列
    }
}

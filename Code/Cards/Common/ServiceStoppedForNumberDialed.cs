using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Decibel;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Common;

/// <summary>
///     拨打用户即刻停机 —— 2费攻击。对目标造成 {Damage} 点伤害。
///     喧响：额外施加2层「虚弱」、2层「易伤」和 {Magic} 点失衡。
/// </summary>
[RegisterCard(typeof(ZZZCardPool))]
public sealed class ServiceStoppedForNumberDialed() : ZZZBaseCard(
    2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true
), IDecibelCardSource
{
    public int DecibelCost => DecibelData.DefaultCost;

    /// <summary>「虚弱」「易伤」层数（卡牌文本字面量：「2层『虚弱』、2层『易伤』」）。</summary>
    private const int DebuffStacks = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("Magic", 3m)
    ];

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;

        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 喧响：额外施加2层「虚弱」、2层「易伤」和 {Magic} 点失衡
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            await ApplyPowerTo<WeakPower>(choiceContext, target, DebuffStacks, cardPlay.Card);
            await ApplyPowerTo<VulnerablePower>(choiceContext, target, DebuffStacks, cardPlay.Card);
            DazeStore.Get(target).AddDaze(DynamicVars["Magic"].IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["Magic"].UpgradeValueBy(2m);
    }
}

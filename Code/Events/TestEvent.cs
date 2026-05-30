using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Events;

[RegisterActEvent(typeof(Overgrowth))] // 指定只有密林这章生成
// [RegisterSharedEvent] // 如果需要自定义生成条件，可以注册成通用再重载isAllowed
public sealed class TestEvent : ModEventTemplate
{
    // 背景图位置
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://ZZZMod/images/events/TestEvent.png"
    );

    // 设置一些数值
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Unblockable | ValueProp.Unpowered),
        new GoldVar(60)
    ];

    // 什么时候会遇到。这里的条件是所有玩家的金币都大于等于60
    public override bool IsAllowed(IRunState runState) => runState.Players.All(p => p.Gold >= DynamicVars.Gold.BaseValue);

    // 事件开始前的逻辑。这里是禁止玩家移除药水
    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        Owner!.CanRemovePotions = false;
        return Task.CompletedTask;
    }

    // 事件结束后的逻辑。这里是允许玩家移除药水
    protected override void OnEventFinished()
    {
        Owner!.CanRemovePotions = true;
    }

    // 生成事件初始选项。这里是两个选项：失去生命值或者失去金币，然后进入选择奖励阶段
    // 与 CustomEventModel.Option(delegate, pageKey) 一致：textKey = Id.Entry + ".pages." + page + ".options." + Slugify(方法名)
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, TakeDamage, InitialOptionKey("TAKE_DAMAGE")),
        new EventOption(this, LoseGold, InitialOptionKey("LOSE_GOLD")),
    ];

    // 失去生命
    private async Task TakeDamage()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.Damage, null, null);
        ChooseRewardTypePage();
    }

    // 失去金币
    private async Task LoseGold()
    {
        await PlayerCmd.LoseGold(DynamicVars.Gold.BaseValue, Owner!, GoldLossType.Stolen);
        ChooseRewardTypePage();
    }

    // 进入事件第二阶段，两个选项：选择药水或者选择卡牌
    private void ChooseRewardTypePage()
    {
        SetEventState(L10NLookup($"{Id.Entry}.pages.CHOOSE_TYPE.description"), [
            new EventOption(this, ChoosePotions, ModOptionKey("CHOOSE_TYPE", "CHOOSE_POTIONS")),
            new EventOption(this, ChooseCards, ModOptionKey("CHOOSE_TYPE", "CHOOSE_CARDS")),
        ]);
    }

    // 选择药水奖励，然后结束事件
    private async Task ChoosePotions()
    {
        await RewardsCmd.OfferCustom(Owner!, [new PotionReward(Owner!)]);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.POTIONS_CHOSEN.description"));
    }

    // 选择卡牌奖励，然后结束事件
    private async Task ChooseCards()
    {
        await RewardsCmd.OfferCustom(Owner!, [new CardReward(CardCreationOptions.ForNonCombatWithDefaultOdds([Owner!.Character.CardPool]), 3, Owner)]);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.CARDS_CHOSEN.description"));
    }
}
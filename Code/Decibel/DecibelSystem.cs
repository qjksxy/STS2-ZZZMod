using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.RunData;
using STS2RitsuLib.Scaffolding.Cards.HandGlow;

namespace ZZZMod.Code.Decibel;

/// <summary>
///     Decibel 系统核心入口。
///     管理 Decibel 值的获得、消耗和跨战斗持久化。
/// </summary>
public static class DecibelSystem
{
    internal static DecibelData Current { get; private set; } = new();

    private static RunSavedData<DecibelSaveData>? _savedSlot;

    public static void Init()
    {
        var store = RunSavedDataStore.For(Entry.ModId);
        _savedSlot = store.Register<DecibelSaveData>(
            "decibel",
            () => new DecibelSaveData(),
            new RunSavedDataOptions
            {
                WritePolicy = RunSavedDataWritePolicy.WhenNonDefault,
            });

        RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(OnCombatStarting);
        RitsuLibFramework.SubscribeLifecycle<CombatEndedEvent>(OnCombatEnded);
        RitsuLibFramework.SubscribeLifecycle<CardPlayedEvent>(OnCardPlayed);
        RitsuLibFramework.SubscribeLifecycle<CreatureDiedEvent>(OnCreatureDied);

        RegisterDecibelCardGlows();
    }

    /// <summary>
    ///     为所有实现 <see cref="IDecibelCardSource" /> 的卡牌注册手牌金色高亮：
    ///     当喧响值足够激活额外效果时，手牌中的该卡发光提示。
    /// </summary>
    private static void RegisterDecibelCardGlows()
    {
        var assembly = typeof(DecibelSystem).Assembly;
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface) continue;
            if (!typeof(CardModel).IsAssignableFrom(type)) continue;
            if (!typeof(IDecibelCardSource).IsAssignableFrom(type)) continue;

            ModCardHandGlowRegistry.Register(type, ModCardHandGlowRules.Gold(CanActivateDecibelEffect));
        }
    }

    private static bool CanActivateDecibelEffect(CardModel card) =>
        card is IDecibelCardSource source && CanSpend(source.DecibelCost);

    private static void OnCombatStarting(CombatStartingEvent evt)
    {
        if (_savedSlot == null) return;
        var runState = evt.RunState as RunState;
        if (runState == null) return;
        var saved = _savedSlot.Get(runState);
        Current.Value = saved?.Value ?? 0;
    }

    private static void OnCombatEnded(CombatEndedEvent evt)
    {
        if (_savedSlot == null) return;
        var runState = evt.RunState as RunState;
        if (runState == null) return;
        var saved = _savedSlot.Get(runState);
        if (saved != null)
            saved.Value = Current.Value;
    }

    private static void OnCardPlayed(CardPlayedEvent evt)
    {
        var card = evt.CardPlay?.Card;
        if (card == null) return;

        var rarity = card.Rarity;
        var amount = rarity switch
        {
            CardRarity.Basic => 1,
            CardRarity.Common => 2,
            CardRarity.Uncommon => 3,
            CardRarity.Rare => 4,
            _ => 1,
        };

        GainDecibel(amount);
    }

    private static void OnCreatureDied(CreatureDiedEvent evt)
    {
        var creature = evt.Creature;
        if (creature == null || creature.Side != CombatSide.Enemy) return;
        GainDecibel(5);
    }

    public static void GainDecibel(int amount)
    {
        if (amount <= 0) return;
        var old = Current.Value;
        Current.Gain(amount);
        if (Current.Value != old)
            Entry.Logger.Debug($"[Decibel] +{amount} → {Current.Value}/{DecibelData.MaxValue}");
    }

    public static bool TrySpendDecibel(int cost) => Current.TrySpend(cost);
    public static bool CanSpend(int cost) => Current.CanSpend(cost);
    public static int GetValue() => Current.Value;
}

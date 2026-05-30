using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Cards;

public abstract class ZZZBaseCard : ModCardTemplate
{
    protected ZZZBaseCard(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true)
        : base(cost, type, rarity, target, showInCardLibrary)
    {
    }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://ZZZMod/images/cards/{GetType().Name}.png"
    );

    private static string PascalToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0 && !char.IsUpper(name[i - 1]))
                sb.Append('_');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 悬浮提示 —— 基类 + 子类分层合并
    // ═══════════════════════════════════════════════════════════════════════════
    // 角色通用 HoverTip 放在 BaseHoverTips，卡牌特有的放在 OwnAdditionalHoverTips。
    // 最终输出 = BaseHoverTips + OwnAdditionalHoverTips。

    protected virtual IEnumerable<IHoverTip> BaseHoverTips => [];
    protected virtual IEnumerable<IHoverTip> OwnAdditionalHoverTips => [];

    protected sealed override IEnumerable<IHoverTip> AdditionalHoverTips =>
        BaseHoverTips.Concat(OwnAdditionalHoverTips);

    // ═══════════════════════════════════════════════════════════════════════════
    // 自定义关键词 —— 基类 + 子类分层合并
    // ═══════════════════════════════════════════════════════════════════════════

    protected virtual IEnumerable<string> BaseKeywordIds => [];
    protected virtual IEnumerable<string> OwnKeywordIds => [];

    protected sealed override IEnumerable<string> RegisteredKeywordIds =>
        BaseKeywordIds.Concat(OwnKeywordIds);

    // 通用关键词条件判定
    protected bool HasKeyword(string qualifiedKeywordId) =>
        RegisteredKeywordIds.Contains(qualifiedKeywordId);

    protected virtual bool IsOverflowActive
    {
        get
        {
            if (Owner?.Creature == null) return false;
            return Owner.Creature.CurrentHp >= Owner.Creature.MaxHp;
        }
    }

    protected virtual bool OwnShouldGlowGold => false;

    protected sealed override bool ShouldGlowGoldInternal =>
        (HasKeyword(ZZZModKeywords.Overflow) && IsOverflowActive) || OwnShouldGlowGold;

    /// <summary>满盈是否触发。各卡牌在 OnPlay 中自行定义效果。</summary>
    protected bool ShouldTriggerOverflow() =>
        HasKeyword(ZZZModKeywords.Overflow) && IsOverflowActive;

    /// <summary>对单体目标造成伤害。</summary>
    protected async Task DealDamage(PlayerChoiceContext choiceContext, DynamicVar damageVar, Creature? target,
        string hitFx = "vfx/vfx_attack_slash")
    {
        if (target == null) return;
        await DamageCmd.Attack(damageVar.BaseValue)
            .FromCard(this).Targeting(target).WithHitFx(hitFx).Execute(choiceContext);
    }

    /// <summary>对全体敌人造成伤害。</summary>
    // protected async Task DealDamageToAll(PlayerChoiceContext choiceContext, DynamicVar damageVar,
    //     string hitFx = "vfx/vfx_attack_slash")
    // {
    //     await DamageCmd.Attack(damageVar.BaseValue)
    //         .FromCard(this).TargetingAllOpponents(CombatState).WithHitFx(hitFx).Execute(choiceContext);
    // }

    /// <summary>以固定数值对单体造成伤害。</summary>
    protected async Task DealDamageRaw(PlayerChoiceContext choiceContext, decimal amount, Creature? target,
        string hitFx = "vfx/vfx_attack_slash")
    {
        if (target == null) return;
        await DamageCmd.Attack(amount)
            .FromCard(this).Targeting(target).WithHitFx(hitFx).Execute(choiceContext);
    }

    /// <summary>以固定数值为自己获得格挡。</summary>
    protected async Task GainBlockRaw(decimal amount, CardPlay? cardPlay = null)
    {
        await CreatureCmd.GainBlock(Owner.Creature,
            new BlockVar(amount, ValueProp.Move), cardPlay);
    }

    /// <summary>为自己施加能力。</summary>
    protected async Task ApplyPowerSelf<T>(int amount) where T : PowerModel, new()
    {
        await PowerCmd.Apply<T>(Owner.Creature, amount, Owner.Creature, this);
    }

    /// <summary>为指定目标施加能力。</summary>
    protected async Task ApplyPowerTo<T>(Creature? target, int amount)
        where T : PowerModel, new()
    {
        if (target == null) return;
        await PowerCmd.Apply<T>(target, amount, Owner.Creature, this);
    }

    /// <summary>修改已有能力的层数（正值增加，负值减少）。</summary>
    protected async Task ModifyPower<T>(Creature? target, int delta) where T : PowerModel
    {
        if (target == null) return;
        var power = target.GetPower<T>();
        if (power == null) return;
        await PowerCmd.ModifyAmount(power, delta, Owner.Creature, this);
    }

    /// <summary>抽牌。</summary>
    // protected async Task DrawCards(int count)
    // {
    //     if (count > 0) await CardPileCmd.Draw(count, Owner);
    // }

    /// <summary>获得能量。</summary>
    protected async Task GainEnergy(int amount)
    {
        if (amount > 0) await PlayerCmd.GainEnergy(amount, Owner);
    }

    /// <summary>检查自己是否拥有指定 Power。</summary>
    protected bool HasPower<T>() where T : PowerModel =>
        Owner?.Creature?.GetPower<T>() != null;

    /// <summary>检查指定 Power 层数是否 >= threshold。</summary>
    protected bool HasPowerAmount<T>(int threshold = 1) where T : PowerModel
    {
        if (Owner?.Creature == null) return false;
        var p = Owner.Creature.GetPower<T>();
        return p != null && p.Amount >= threshold;
    }

    /// <summary>获取指定 Power 层数，没有则返回 0。</summary>
    protected int GetPowerAmount<T>() where T : PowerModel =>
        Owner?.Creature?.GetPower<T>()?.Amount ?? 0;

    /// <summary>安全检查：当 TargetType.AnyEnemy/AnyAlly 时确认目标非空。</summary>
    protected bool EnsureTarget(CardPlay cardPlay, out Creature target)
    {
        target = cardPlay.Target!;
        return target != null;
    }
}
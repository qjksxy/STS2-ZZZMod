using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

// ═══════════════════════════════════════════════════════════════════════════════
// STS2-RitsuLib 能力模板（教学用）
//
// 本文件演示了基于 STS2-RitsuLib 的能力开发所有核心模式。
// 直接继承 ModPowerTemplate，不依赖任何特定模组的中间基类。
//
// 阅读顺序建议：
//   1. 能力类型与堆叠方式（PowerType / PowerStackType）
//   2. 回合生命周期钩子（AfterTurnEnd / AfterPlayerTurnStart / AfterSideTurnStart）
//   3. 伤害相关钩子（AfterDamageReceived / ModifyDamageAdditive / ModifyDamageMultiplicative）
//   4. 能力层数变化钩子（AfterPowerAmountChanged）
//   5. 其他钩子（AfterCurrentHpChanged 等）
//   6. 悬浮提示（AdditionalHoverTips）
//   7. 素材路径（AssetProfile）
//   8. 动态变量（CanonicalVars）
// ═══════════════════════════════════════════════════════════════════════════════

namespace ZZZMod.Templates;

// ── 注册能力 ─────────────────────────────────────────────────────────────────
// [RegisterPower] 告知框架将此类自动注册。
// 可以指定 Inherit = true 让子类自动继承注册。
// [RegisterPower(Inherit = true)]

/// <summary>
///     模板能力 —— 涵盖 Buff/Debuff、多种生命周期钩子。
///     能力是附加在 Creature 上的持续性效果，可以有层数。
/// </summary>
public sealed class TemplatePower : ModPowerTemplate
{
    // ═══════════════════════════════════════════════════════════════════════════
    // 一、能力类型（PowerType）
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // PowerType.Buff   — 增益（绿色图标，正向效果）
    // PowerType.Debuff — 减益（红色图标，负面效果）
    //
    // 这决定了能力的视觉呈现和大多数情况下玩家对它的认知。

    public override PowerType Type => PowerType.Buff;

    // ═══════════════════════════════════════════════════════════════════════════
    // 二、堆叠方式（PowerStackType）
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // PowerStackType.Single  — 不堆叠，每层独立存在（如脆弱、虚弱）
    // PowerStackType.Counter — 层数堆叠，Amount 表示叠加次数（如力量、敏捷）
    //
    // Counter 类型的能力在多次施加时会增加层数（Amount += N），
    // Single 类型则每次施加为独立的能力实例。

    public override PowerStackType StackType => PowerStackType.Counter;

    // ═══════════════════════════════════════════════════════════════════════════
    // 三、是否允许负层数
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // 当 PowerCmd.ModifyAmount 减少层数时，若 AllowNegative = false，
    // 层数降到 0 以下时会自动移除该能力。

    public override bool AllowNegative => false;

    // ═══════════════════════════════════════════════════════════════════════════
    // 四、动态变量
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // 能力也可以有自己的 DynamicVar，在 smartDescription 中引用。
    // 例如 {Amount} 是一个内置变量，引用当前能力层数。

    // ═══════════════════════════════════════════════════════════════════════════
    // 五、悬浮提示（Hover Tips）
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // AdditionalHoverTips 可以在能力悬停时展示关联的提示。
    // 注意：能力的描述文本来自 localization/powers.json，而非代码。

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            // 示例：引用另一个能力的悬浮提示
            // yield return HoverTipFactory.FromPower<AnotherPower>();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 六、素材路径
    // ═══════════════════════════════════════════════════════════════════════════

    public override PowerAssetProfile AssetProfile =>
        new PowerAssetProfile(
            $"res://MyMod/images/powers/template_power.png",
            $"res://MyMod/images/powers/big/template_power.png"
        );

    // ═══════════════════════════════════════════════════════════════════════════
    // 七、生命周期钩子（按触发时机分类）
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // 以下是能力可以重写的所有主要钩子方法。
    // 每个钩子都有特定触发时机和适用场景。

    // ── 7.1 回合结束 ─────────────────────────────────────────────────────

    /// <summary>
    ///     在某一方（玩家/敌人）回合结束时触发。
    ///     参数 side 表示哪一方的回合刚结束。
    ///
    ///     适用场景：回合结束时的持续效果（如获得格挡、造成伤害、减少层数）。
    /// </summary>
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        // 只在自己的回合结束时响应
        if (side != Owner.Side) return;
        if (Amount <= 0) return;

        // 示例：回合结束时获得层数等量的格挡
        await CreatureCmd.GainBlock(Owner, Amount, 0, null);
    }

    // ── 7.2 玩家回合开始 ─────────────────────────────────────────────────

    /// <summary>
    ///     在玩家回合开始时触发。
    ///     参数 player 是回合开始的玩家。
    ///
    ///     适用场景：每回合开始时给玩家发牌、获得能量等。
    /// </summary>
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 示例：检查是否是自己的能力拥有者
        // if (player.Creature != Owner) return;
        //
        // // 每回合生成一张 Token 牌加入手牌
        // var tokenCard = CombatState.CreateCard<SomeTokenCard>(Owner.Player);
        // await CardPileCmd.AddGeneratedCardToCombat(tokenCard, PileType.Hand, player);
    }

    // ── 7.3 一方回合开始 ─────────────────────────────────────────────────

    /// <summary>
    ///     在某一方（玩家/敌人）回合开始时触发。
    ///     比 AfterPlayerTurnStart 更通用，也会对敌人触发。
    ///
    ///     注意：此方法没有 PlayerChoiceContext 参数！
    ///     需要执行命令时用 new ThrowingPlayerChoiceContext() 替代。
    /// </summary>
    public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        // 示例：敌人/自己回合开始时扣除能力层数
        // if (side == Owner.Side && Amount > 0)
        // {
        //     await PowerCmd.ModifyAmount(
        //         new ThrowingPlayerChoiceContext(), this, -1, Owner, null);
        //
        //     // 层数为 0 时移除
        //     if (Amount <= 0)
        //         await PowerCmd.Remove(this);
        // }
    }

    // ── 7.4 受到伤害时 ───────────────────────────────────────────────────

    /// <summary>
    ///     在拥有者受到伤害后触发。
    ///     参数：
    ///       target — 受到伤害的生物（通常是 Owner）
    ///       result — 伤害结果详情（包含 UnblockedDamage、OverkillDamage 等）
    ///       props  — 伤害值属性
    ///       dealer — 伤害来源生物
    ///       cardSource — 来源卡牌
    ///
    ///     适用场景：受伤后反击、积累计数、触发效果。
    /// </summary>
    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        // 只处理自己的能力拥有者
        if (target != Owner) return;

        // 获取实际受到的伤害（未被格挡的部分）
        var actualDamage = result.UnblockedDamage;
        if (actualDamage > 0)
        {
            // 示例：受到伤害时获得等量的力量（层数作为增益系数）
            // await PowerCmd.Apply<StrengthPower>(
            //     choiceContext, Owner, actualDamage * Amount, Owner, cardSource);
        }
    }

    // ── 7.5 伤害修正（修改承受/造成的伤害值） ──────────────────────────

    /// <summary>
    ///     修改 Owner 造成的伤害（加法修正）。
    ///     返回值为正表示增加伤害，为负表示减少伤害。
    ///
    ///     参数：
    ///       target     — 伤害目标
    ///       amount     — 原始伤害值
    ///       props      — 伤害值属性
    ///       dealer     — 伤害来源
    ///       cardSource — 来源卡牌
    /// </summary>
    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        // 示例：如果伤害来源是自己，减少 50% 伤害
        // if (dealer != Owner) return 0m;
        // return -amount * 0.5m;

        return 0m;
    }

    /// <summary>
    ///     修改 Owner 承受/造成的伤害（乘法修正）。
    ///     返回值为伤害倍率：1.0 = 不变，2.0 = 翻倍，0.5 = 减半。
    ///
    ///     参数同上，但 target 是承受伤害的生物。
    /// </summary>
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        // 示例：如果自己是伤害目标，受到的伤害翻倍
        // if (target != Owner) return 1m;
        // return 2m;

        return 1m;
    }

    // ── 7.6 能力层数变化后 ───────────────────────────────────────────────

    /// <summary>
    ///     在任意能力的层数发生变化后触发（全局监听）。
    ///     参数：
    ///       power      — 发生变化的那个能力
    ///       amount     — 变化量（正=增加，负=减少）
    ///       applier    — 施加者
    ///       cardSource — 来源卡牌
    ///
    ///     适用场景：监听某个特定能力的变化做出联动反应。
    ///     例如：压力每增加 1 层，提升骑士之剑的伤害。
    /// </summary>
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        // 示例：只监听自己的能力
        // if (power != this) return;
        //
        // // 层数增加时做某事
        // if (amount > 0 && Owner != null)
        // {
        //     // ...
        // }
    }

    // ── 7.7 生命值变化后 ─────────────────────────────────────────────────

    /// <summary>
    ///     在 Owner 的当前生命值发生变化后触发。
    ///     参数 delta 正数表示回血，负数表示掉血。
    ///
    ///     注意：此方法没有 PlayerChoiceContext 参数！
    ///     需要执行命令时用 new ThrowingPlayerChoiceContext() 替代。
    /// </summary>
    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        // 示例：生命值低于阈值时触发效果
        // if (creature != Owner) return;
        // if (Owner.CurrentHp < Owner.MaxHp * 0.5m && Amount > 0)
        // {
        //     await PowerCmd.ModifyAmount(
        //         new ThrowingPlayerChoiceContext(), this, -1, Owner, null);
        // }
    }

    // ── 7.8 战斗开始时 ───────────────────────────────────────────────────

    /// <summary>
    ///     在战斗开始时触发（所有战斗准备完成之后）。
    /// </summary>
    // public override async Task AtBattleStart(PlayerChoiceContext choiceContext) { ... }

    // ── 7.9 卡牌打出时 ───────────────────────────────────────────────────

    /// <summary>
    ///     在 Owner 打出任何卡牌后触发。
    /// </summary>
    // public override async Task AfterCardPlayed(
    //     PlayerChoiceContext choiceContext, CardPlay cardPlay) { ... }

    // ── 7.10 卡牌抽到时 ──────────────────────────────────────────────────

    /// <summary>
    ///     在 Owner 抽到卡牌时触发。
    /// </summary>
    // public override async Task AfterCardDrawn(
    //     PlayerChoiceContext choiceContext, CardModel card) { ... }

    // ── 7.11 回合内首次满足条件时 ────────────────────────────────────────

    /// <summary>
    ///     在 Owner 首次受到伤害时触发（每回合重置）。
    /// </summary>
    // public override async Task AfterFirstDamageReceivedThisTurn(
    //     PlayerChoiceContext choiceContext,
    //     Creature target, DamageResult result,
    //     ValueProp props, Creature? dealer, CardModel? cardSource) { ... }

    // ── 7.12 获得格挡时 ──────────────────────────────────────────────────

    /// <summary>
    ///     在 Owner 获得格挡后触发。
    /// </summary>
    // public override async Task AfterBlockGained(
    //     PlayerChoiceContext choiceContext, decimal amount) { ... }

    // ── 7.13 状态（易伤/虚弱等）施加/移除 ────────────────────────────────

    // public override async Task AfterVulnerableApplied(...)  { ... }
    // public override async Task AfterWeakApplied(...)       { ... }
    // public override async Task AfterFrailApplied(...)      { ... }

    // ── 7.14 能力被移除时 ───────────────────────────────────────────────

    /// <summary>
    ///     在自身被移除时触发（用于清理效果）。
    ///     基类的 OnRemove 会在 RemoveFromOwner 前调用。
    /// </summary>
    // public override void OnRemove() { ... }

    // ═══════════════════════════════════════════════════════════════════════════
    // 八、使用 ThrowingPlayerChoiceContext 的场景
    // ═══════════════════════════════════════════════════════════════════════════
    //
    // 某些钩子（如 AfterSideTurnStart、AfterCurrentHpChanged）没有标准的
    // PlayerChoiceContext 参数。当你需要在这些钩子中执行 PowerCmd、DamageCmd
    // 等需要上下文的操作时，使用 new ThrowingPlayerChoiceContext() 替代。
    //
    // ThrowingPlayerChoiceContext 不会做选择，而是直接抛出异常如果遇到需要
    // 玩家交互的命令。因此应确保只用它执行不需要玩家选择的命令。
}

// 最后，需要补全本地化文本，本地化文本在 powers.json 中。
// 通过ritsulib添加内容，其id会变成{modid}_{类别}_{原id}。例如这里的modid是ZZZ_MOD,类别是POWER。
// 文本示例：
// {
//     "ZZZ_MOD_POWER_TEST_POWER.description": "每次抽牌时，获得一点[gold]力量[/gold]。",
//     "ZZZ_MOD_POWER_TEST_POWER.smartDescription": "每次抽牌时，获得[blue]{Amount}[/blue]点[gold]力量[/gold]。",
//     "ZZZ_MOD_POWER_TEST_POWER.title": "邪火"
// }
// smartDescription可以使用{Amount}来显示当前层数。

// ═══════════════════════════════════════════════════════════════════════════════
// 附：完整钩子速查表
// ═══════════════════════════════════════════════════════════════════════════════
//
// ┌─────────────────────────────────┬─────────────┬──────────────────────────────────────┐
// │ 方法名                          │ Context?    │ 触发时机                             │
// ├─────────────────────────────────┼─────────────┼──────────────────────────────────────┤
// │ AtBattleStart                   │ 有          │ 战斗开始                             │
// │ AfterPlayerTurnStart            │ 有          │ 玩家回合开始                         │
// │ AfterSideTurnStart              │ 无 (Throw)  │ 任一方回合开始                       │
// │ AfterTurnEnd                    │ 有          │ 任一方回合结束                       │
// │ AfterCardPlayed                 │ 有          │ Owner 打出卡牌                        │
// │ AfterCardDrawn                  │ 有          │ Owner 抽到卡牌                        │
// │ AfterDamageReceived             │ 有          │ Owner 受到伤害                        │
// │ AfterFirstDamageReceivedThisTurn│ 有          │ Owner 每回合首次受伤                   │
// │ AfterBlockGained                │ 有          │ Owner 获得格挡                        │
// │ AfterPowerAmountChanged         │ 有          │ 任意能力层数变化（全局）               │
// │ AfterCurrentHpChanged           │ 无 (Throw)  │ Owner 生命值变化                       │
// │ ModifyDamageAdditive            │ —           │ Owner 造成伤害时（加法修正）            │
// │ ModifyDamageMultiplicative      │ —           │ Owner 承受/造成伤害（倍率修正）         │
// │ OnRemove                        │ —           │ 能力被移除时                          │
// └─────────────────────────────────┴─────────────┴──────────────────────────────────────┘
//
// 有 Context = 参数包含 PlayerChoiceContext，可以直接执行需要上下文的命令。
// 无 (Throw) = 没有 PlayerChoiceContext，需要用 new ThrowingPlayerChoiceContext() 替代。
// — = 同步方法，不需要上下文。
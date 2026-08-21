# 添加卡牌流程（zzz-mod）

> 所有准备实现的卡牌以 **`docs/Cards.csv` 为唯一实现依据**；本文是编写代码与本地化的步骤。
> 基于 STS2-RitsuLib 框架，可参考现有卡牌：`Shimotsuki`（消耗+群体攻击）、`EndlessWinter`（喧响攻击）、`PleaseDoNotResist`（连携攻击）、`RaieiTenge`（喧响技能）、`GekkaRuten`（失衡条件判定）。
> 已实现卡牌清单见 `docs/implemented_cards.md`，更新 Cards.csv 后对比此文件快速定位变更。

---

## 0. 卡牌设计流水线（Cards.csv）

### 0.1 前置检查（必须在实现前完成）

在写任何代码之前，**必须先对 `docs/Cards.csv` 中的每张卡牌执行以下检查**：

**第一轮：必要属性完整性**

| 列 | 要求 | 常见问题 |
|----|------|---------|
| ID | 非空，英文 Title Case | 缺失、使用了中文 |
| 名称 | 非空 | 缺失 |
| 稀有度 | 基础/普通/罕见/稀有 之一 | 缺失、使用了英文 |
| 费用 | 非空，数字 | 缺失、非数字 |
| 类别 | 攻击/技能/能力 之一 | 缺失 |

**第二轮：描述清晰度**

- 卡牌效果描述是否无歧义，能否明确理解其机制
- 效果中的数值是否都能对应到 CSV 中的 伤害/强伤/防御/强防/魔法/强魔 列
- 升级效果是否明确（为空则按 强X 列自动推导）

**第三轮：中英文命名一致性**

- `ID` 列的英文命名即为代码中的类名依据（Title Case 转 PascalCase，如 `Charged Beat` → `ChargedBeat`）
- `名称` 列的中文即为本地化 `.title` 值
- **代码中所有英文命名必须严格按照 `ID` 列给定的翻译**，不得使用拼音或自行翻译
- 检查 ID 与名称的对应关系是否合理（音译/意译一致性）

**检查结果处理：**

- 若有任何卡牌存在问题，**列出所有问题卡牌及具体问题，报告给用户后结束**，不进入实现阶段
- `Cards.csv` 是生成的文件，**不得直接修改**，任何问题必须报告给用户，由用户修改

### 0.2 实现阶段（所有卡牌通过检查后）

1. 运行 `python docs/csv_to_cards.py`，自动生成 `ZZZMod/localization/zhs/cards_generated.json`（title + description 条目）
2. 将生成的条目并入 `cards.json`（可按需添加 `[gold]` 等展示标记）
3. 按本文以下步骤编写卡牌类

**红线约定：**

- **不得擅自实现 `Cards.csv` 中没有的卡牌**。有新的卡牌设计想法，可以整理为文档记录，但不得修改 `Cards.csv` 或直接实现
- `docs/` 下的设计稿（如 `daze_system.md`、`decibel_cards_design.md`）只是**前期参考，不作为实现方案**，一切以 `Cards.csv` 为准
- `Cards.csv` 的「对应角色」列仅供卡图绘制参考，开发时忽略；玩家角色始终是法厄同（Phaethon）
- 失衡（Daze）为**正向计数**：打出卡牌累积敌人的失衡值，累积满后敌人进入失衡状态（受伤 +50%）。游戏内文本措辞后期统一规范，开发时不要纠结文案方向

---

## 1. 创建卡牌代码

在 `Code/Cards/{稀有度}/` 下创建 `.cs` 文件（稀有度目录：`Basic/` `Common/` `Uncommon/` `Rare/`）。

**路径示例：** `Code/Cards/Uncommon/YourCard.cs`

**模板（失衡攻击卡）：**

```csharp
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Cards.Uncommon;

[RegisterCard(typeof(ZZZCardPool))]
public sealed class YourCard() : ZZZBaseCard(
    1,                    // 费用
    CardType.Attack,      // 类型：Attack / Skill / Power
    CardRarity.Uncommon,  // 稀有度：Basic / Common / Uncommon / Rare
    TargetType.AnyEnemy,  // 目标：Self / AnyEnemy / AnyAlly
    true                  // 是否显示在图鉴中
)
{
    // 每次攻击命中的失衡值（默认 1，重写自定义）
    public override int DazeAmount => 2;

    // ── 动态变量（所有数值都应通过 DynamicVar 定义，避免硬编码魔法值）──
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move)
    ];

    // ── 悬浮提示（引用关联的能力、卡牌或关键词）──
    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<InvestPower>()
    ];

    // ── 打出效果 ──
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);
    }

    // ── 升级效果 ──
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}
```

**关键点：**

- 必须加 `[RegisterCard(typeof(XXXPool))]`，通过 `ModTypeDiscoveryHub` 自动注册。
- 卡池选择：角色专属池需自定义（如 `ZZZCardPool`），不要使用 `ColorlessCardPool`（无色池无法生成角色奖励）。
- 基类 `ZZZBaseCard` 自动处理卡图路径：`res://ZZZMod/images/cards/{ClassName}.png`。
- 类名用 `sealed`，与现有卡牌保持一致。

---

### 1.1 动态变量（CanonicalVars）

**所有数值都应定义为动态变量，禁止在逻辑中直接写魔法值。** 动态变量会被本地化系统自动引用，并在升级、显示等场景中统一管理。

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars => [
    new DamageVar(12, ValueProp.Move),       // 伤害变量（含伤害属性）
    new BlockVar(5, ValueProp.Move),         // 格挡变量（含格挡属性）
    new DynamicVar("Magic", 3m),             // 普通数值变量（如失衡值、喧响伤害）
    new DynamicVar("InvestAmount", 1m)       // 自定义命名变量
];
```

在 `OnPlay` 中通过 `DynamicVars["变量名"]` 取值：

```csharp
var dmg = DynamicVars.Damage;                       // 内置 Damage 变量
var block = DynamicVars.Block;                      // 内置 Block 变量
var amount = DynamicVars["Magic"].IntValue;         // 自定义变量
```

在 `OnUpgrade` 中修改变量值（升级数值 = CSV 的 强伤/强防/强魔 列）：

```csharp
DynamicVars.Damage.UpgradeValueBy(4);              // 伤害 +4（强伤）
DynamicVars.Block.UpgradeValueBy(3);               // 格挡 +3（强防）
DynamicVars["Magic"].UpgradeValueBy(1m);           // 自定义变量 +1（强魔）
EnergyCost.UpgradeBy(-1);                          // 降 1 费（强费）
```

---

### 1.2 悬浮提示（OwnAdditionalHoverTips）

卡牌涉及指定能力、Token 卡牌或关键词时，应添加悬浮提示让玩家可以悬停查看详情。

```csharp
protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
    HoverTipFactory.FromPower<YourPower>(),        // 引用指定能力
    HoverTipFactory.FromCard<Shiv>(),              // 引用指定卡牌
    HoverTipFactory.FromKeyword(CardKeyword.Exhaust) // 引用游戏关键词
];
```

> **注意：** ZZZBaseCard 已将 `AdditionalHoverTips` 密封，子类应使用 `OwnAdditionalHoverTips`。

**喧响关键词悬浮提示**要使用基类助手 `CreateDecibelHoverTip(cost)`（RitsuLib 的 `CreateHoverTip` 无法解析 `{Cost}` 占位符，该助手会注入实际消耗数值）：

```csharp
protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
    CreateDecibelHoverTip(DecibelCost)
];
```

---

### 1.3 自定义关键词（OwnKeywordIds）

如果卡牌使用了自定义关键词（如「满盈」「喧响」），需要在 `OwnKeywordIds` 中声明，并在 `OwnAdditionalHoverTips` 中挂上对应悬浮提示。

```csharp
protected override IEnumerable<string> OwnKeywordIds => [
    ZZZModKeywords.Overflow,   // 满盈
    ZZZModKeywords.Decibel     // 喧响
];
```

关键词定义在 `Code/Cards/ZZZKeywords.cs` 中，使用 `[RegisterOwnedCardKeyword]` 特性注册：

```csharp
[RegisterOwnedCardKeyword(nameof(Overflow), IconPath = "res://icon.svg")]
[RegisterOwnedCardKeyword(nameof(Decibel), IconPath = "res://icon.svg")]
public class ZZZModKeywords
{
    public static readonly string Overflow =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Overflow));
    public static readonly string Decibel =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Decibel));
}
```

> 如果卡牌不涉及自定义关键词，此项可省略。

---

### 1.4 失衡值（DazeAmount）

`ZZZBaseCard` 实现了 `IDazeCardSource`，**攻击类卡牌每次命中会自动累积目标失衡值**（由 `DazeHitListener` 处理），无需在 `OnPlay` 中手动处理：

```csharp
public override int DazeAmount => 2;   // 每次攻击命中失衡 +2（默认 1）
```

特殊场景：

- **手动施加失衡**（如技能卡、群体卡）：设 `DazeAmount => 0` 关闭自动累积，在 `OnPlay` 中手动 `DazeStore.Get(target).AddDaze(数值)`：

```csharp
public override int DazeAmount => 0;

protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    if (!EnsureTarget(cardPlay, out var target)) return;
    DazeStore.Get(target).AddDaze(DynamicVars["Magic"].IntValue);
}
```

- 失衡相关 API 见 `docs/daze_system.md`（设计参考）。

---

### 1.5 喧响卡牌（IDecibelCardSource）

喧响效果：喧响值足够时（默认消耗 20），消耗喧响激活额外效果。

```csharp
[RegisterCard(typeof(ZZZCardPool))]
public sealed class YourDecibelCard() : ZZZBaseCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true), IDecibelCardSource
{
    /// <summary>喧响消耗，默认 20（接口默认实现不能在类上直接调用，需显式声明）。</summary>
    public int DecibelCost => DecibelData.DefaultCost;

    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        CreateDecibelHoverTip(DecibelCost)   // 解析 {Cost} 并展示关键词提示
    ];

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Decibel];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);

        // 喧响：消耗 20 点喧响，额外造成 Magic 点伤害
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
            await DealDamageRaw(choiceContext, DynamicVars["Magic"].BaseValue, target, cardPlay);
    }
}
```

**要点：**

- `DecibelCost` 必须在卡牌类中**显式声明**（接口默认实现 `int DecibelCost => DecibelData.DefaultCost;` 不能通过类实例直接访问，会编译报错）
- 本地化描述中的 `[gold]喧响[/gold]` 用于文本高亮（见第 2 节）
- **手牌高亮自动生效**：`DecibelSystem.Init()` 会扫描程序集，所有实现 `IDecibelCardSource` 的卡牌在喧响值足够时自动发金光（`ModCardHandGlowRegistry`），无需额外注册

---

### 1.6 连携卡牌（IChainCardSource）

连携效果：在手牌中满足特定条件时，自动从手牌打出（不消耗费用）。

```csharp
using MegaCrit.Sts2.Core.Combat;
using STS2RitsuLib.Keywords;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Pools;

[RegisterCard(typeof(ZZZCardPool))]
public sealed class YourChainCard() : ZZZBaseCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true),
    IChainCardSource
{
    /// <summary>连携条件：连续打出2张攻击牌。</summary>
    public bool CheckChainCondition(CardModel card, CardModel lastPlayed)
    {
        if (lastPlayed.Type != CardType.Attack) return false;

        var combatState = card.CombatState;
        if (combatState == null) return false;

        var combatManager = CombatManager.Instance;
        if (combatManager == null) return false;
        var history = combatManager.History;
        if (history == null) return false;

        var recentPlays = history.CardPlaysFinished
            .Where(e => e.HappenedThisTurn(combatState)
                     && e.CardPlay.Player == card.Owner
                     && !e.CardPlay.IsAutoPlay)
            .TakeLast(2)
            .ToList();

        if (recentPlays.Count < 2) return false;
        return recentPlays.All(e => e.CardPlay.Card.Type == CardType.Attack);
    }

    // 悬浮提示：连携关键词（使用 ModKeywordRegistry 构造）
    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips
    {
        get
        {
            var id = ZZZModKeywords.Chain;
            var description = ModKeywordRegistry.GetDescription(id);
            yield return new HoverTip(ModKeywordRegistry.GetTitle(id), description);
        }
    }

    protected override IEnumerable<string> OwnKeywordIds => [ZZZModKeywords.Chain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;
        await DealDamage(choiceContext, DynamicVars.Damage, target, cardPlay);
    }
}
```

**要点：**

- 实现 `IChainCardSource` 接口，编写 `CheckChainCondition` 方法定义触发条件
- `CheckChainCondition` 中访问 `CombatManager.Instance` 和 `History` 前**必须做空检查**
- 关键词悬浮提示使用 `ModKeywordRegistry.GetDescription/GetTitle` 构造（`HoverTipFactory.FromKeyword` 仅支持游戏内建 `CardKeyword` 枚举）
- `OwnKeywordIds` 声明 `ZZZModKeywords.Chain`
- **手牌高亮自动生效**：`ChainSystem.Init()` 会扫描程序集，所有实现 `IChainCardSource` 的卡牌在条件满足时自动发金光，无需额外注册
- 连携触发的牌不会再次触发连携（`IsAutoPlay` 标志防无限循环）
- 本地化描述中连携关键词使用 `[gold]连携[/gold]` 高亮

---

### 1.7 消耗与虚无（原版内建关键词）

消耗（Exhaust）和虚无（Ethereal）是游戏原生关键词，通过 `CanonicalKeywords` 声明：

```csharp
// 消耗：打出后进入消耗堆而非弃牌堆
public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

// 虚无：回合结束时若仍在手中则消耗
public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

// 两者可组合
public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, CardKeyword.Exhaust];
```

悬浮提示使用 `HoverTipFactory.FromKeyword(CardKeyword.Exhaust)` 等。本地化描述中使用 `[gold]消耗[/gold]` / `[gold]虚无[/gold]` 高亮。

参考：`Shimotsuki`（消耗+群体攻击）

---

### 1.8 生成 Token 卡牌

能力或卡牌可以在运行时生成新的卡牌实例加入手牌。典型场景：落霜能力累积满后生成「霜月」。

```csharp
// 在 Power 中生成卡牌加入手牌
var combatState = CombatState;
if (combatState != null && !CombatManager.Instance.IsOverOrEnding)
{
    var card = combatState.CreateCard<Shimotsuki>(Owner.Player);
    await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner.Player);
}
```

**要点：**

- `CombatState.CreateCard<T>(player)` 创建卡牌实例（`T` 为卡牌类型）
- `CardPileCmd.AddGeneratedCardToCombat(card, pileType, player)` 将卡牌加入指定牌堆
- 生成前检查 `!CombatManager.Instance.IsOverOrEnding`（战斗已结束时不要生成）
- 参考：`Shiv.CreateInHand`、`FallenFrostPower`（生成霜月）、`CollisionCourse`（生成 Debris）

---

### 1.9 ZZZBaseCard 常用辅助方法

| 方法 | 签名 | 用途 |
|------|------|------|
| `ApplyPowerSelf<T>` | `(PlayerChoiceContext, int, CardModel?)` | 为自己施加能力 |
| `ApplyPowerTo<T>` | `(PlayerChoiceContext, Creature?, int, CardModel?)` | 为目标施加能力 |
| `DealDamage` | `(PlayerChoiceContext, DynamicVar, Creature?, CardPlay?, string)` | 对单体造成伤害（按变量值） |
| `DealDamageRaw` | `(PlayerChoiceContext, decimal, Creature?, CardPlay?, string)` | 以固定数值造成伤害 |
| `GainBlockRaw` | `(decimal, CardPlay?)` | 为自己获得格挡 |
| `GainEnergy` | `(int)` | 获得能量 |
| `ModifyPower<T>` | `(PlayerChoiceContext, Creature?, int, CardModel?)` | 修改能力层数 |
| `HasPower<T>` / `HasPowerAmount<T>` | `()` | 检查是否拥有指定能力 / 层数是否达标 |
| `GetPowerAmount<T>` | `()` | 获取能力层数 |
| `EnsureTarget` | `(CardPlay, out Creature)` | 安全检查目标非空 |
| `CreateDecibelHoverTip` | `(int cost)` | 创建喧响关键词悬浮提示（解析 {Cost}） |
| `ShouldTriggerOverflow` | `()` | 满盈关键词条件判定 |

---

## 2. 添加本地化文本

编辑 `ZZZMod/localization/zhs/cards.json`（或从 `cards_generated.json` 合并生成条目），按 `{ModId}_{类别}_{类名大写蛇形}` 格式添加条目。

**Key 命名规则：** `ZZZ_MOD_CARD_类名大写蛇形.key`

| Key 后缀 | 说明 | 示例 |
|-----------|------|------|
| `.title` | 卡牌名称 | `"你的卡牌"` |
| `.description` | 卡牌描述 | `"造成{Damage:diff()}点伤害。[gold]喧响[/gold]：额外造成{Magic:diff()}点伤害。"` |

**DynamicVar 引用格式：**

| 语法 | 说明 | 示例 |
|------|------|------|
| `{Damage:diff()}` | 伤害变量 | `造成{Damage:diff()}点伤害。` |
| `{Block:diff()}` | 格挡变量 | `获得{Block:diff()}点格挡。` |
| `{变量名:diff()}` | 自定义变量 | `获得{Magic:diff()}层敏捷。` |
| `[gold]文本[/gold]` | 金色高亮（关键词等） | `[gold]喧响[/gold]：额外效果` |
| `[blue]文本[/blue]` | 蓝色高亮（数值区） | — |

> 关键词描述（如喧响的「喧响值足够时，消耗{Cost}点喧响激活额外效果。」）在 `card_keywords.json` 中维护，`{Cost}` 由 `CreateDecibelHoverTip` 在运行时注入实际数值。

---

## 3. 补充卡图（可选）

将卡图放置到 `ZZZMod/images/cards/`，命名为 `{ClassName}.png`。

`ZZZBaseCard` 已默认配置此路径，无需额外代码。未提供卡图时游戏显示占位图。

---

## 4. 构建与测试

```bash
# 编译验证（开发迭代，只看编译是否通过）
dotnet build

# 部署到游戏（DLL + 导出 .pck + 复制到游戏 mods 目录）
./autobuild.bat          # 即 dotnet build -t:ExportPck
```

> `dotnet build` 的 PostBuild 复制步骤在游戏运行时可能报 `Access denied`，不影响编译结果，验证时忽略即可。

**游戏内调试（按 `~` 打开控制台）：**

| 命令 | 说明 |
|------|------|
| `card ZZZ_MOD_CARD_YOUR_CARD` | 获得指定卡牌 |
| `power ZZZ_MOD_POWER_YOUR_POWER 1 0` | 施加指定能力 |

---

## 附录：可重写的关键成员速查

| 成员 | 用途 | 示例 |
|------|------|------|
| `CanonicalVars` | 定义所有数值变量 | `new DynamicVar("Amount", 1m)` |
| `DazeAmount` | 每次攻击命中的失衡值 | `public override int DazeAmount => 2;` |
| `DecibelCost` | 喧响消耗（实现 `IDecibelCardSource`） | `public int DecibelCost => DecibelData.DefaultCost;` |
| `CheckChainCondition` | 连携条件判定（实现 `IChainCardSource`） | 参见 1.6 节 |
| `OwnAdditionalHoverTips` | 关联能力/卡牌/关键词提示 | `CreateDecibelHoverTip(DecibelCost)` |
| `OwnKeywordIds` | 声明自定义关键词 | `ZZZModKeywords.Decibel` / `ZZZModKeywords.Chain` |
| `OnPlay` | 卡牌打出逻辑 | `choiceContext`, `cardPlay` |
| `OnUpgrade` | 升级效果 | `DynamicVars.Damage.UpgradeValueBy(4)` |
| `CanonicalKeywords` | 游戏内建关键词 | `CardKeyword.Exhaust`, `CardKeyword.Innate` |

# 添加新事件（Event）流程（zzz-mod）

> 基于 STS2-RitsuLib 框架，参考 `TestEvent` 的实现。

---

## 1. 创建事件代码

在 `Code/Events/` 目录下创建新的 `.cs` 文件。

**路径示例：** `Code/Events/YourEvent.cs`

**模板：**

```csharp
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

/// <summary>
///     事件名称 —— 简短描述事件内容。
/// </summary>
[RegisterActEvent(typeof(Overgrowth))]  // 指定在哪个章节生成
// [RegisterSharedEvent]  // 如果需要自定义生成条件，可以注册成通用再重载 IsAllowed
public sealed class YourEvent : ModEventTemplate
{
    // ── 背景图 ──
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://ZZZMod/images/events/your_event.png"
    );

    // ── 动态变量（用于本地化中的数值引用）──
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Unblockable | ValueProp.Unpowered),
        new GoldVar(60)
    ];

    // ── 生成条件（RegisterSharedEvent 时需要重写）──
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.All(p => p.Gold >= DynamicVars.Gold.BaseValue);

    // ── 事件开始前的逻辑（可选）──
    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        // 例如：禁止玩家移除药水
        return Task.CompletedTask;
    }

    // ── 事件结束后的逻辑（可选）──
    protected override void OnEventFinished()
    {
        // 例如：恢复玩家操作
    }

    // ── 生成初始选项 ──
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, TakeDamage, InitialOptionKey("TAKE_DAMAGE")),
        new EventOption(this, LoseGold, InitialOptionKey("LOSE_GOLD")),
    ];

    // ── 选项效果：失去生命 ──
    private async Task TakeDamage()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.Damage, null, null);
        ChooseRewardPage();
    }

    // ── 选项效果：失去金币 ──
    private async Task LoseGold()
    {
        await PlayerCmd.LoseGold(DynamicVars.Gold.BaseValue, Owner!, GoldLossType.Stolen);
        ChooseRewardPage();
    }

    // ── 进入第二阶段：选择奖励 ──
    private void ChooseRewardPage()
    {
        SetEventState(L10NLookup($"{Id.Entry}.pages.CHOOSE_REWARD.description"), [
            new EventOption(this, ChoosePotions, ModOptionKey("CHOOSE_REWARD", "CHOOSE_POTIONS")),
            new EventOption(this, ChooseCards, ModOptionKey("CHOOSE_REWARD", "CHOOSE_CARDS")),
        ]);
    }

    // ── 选择药水奖励 ──
    private async Task ChoosePotions()
    {
        await RewardsCmd.OfferCustom(Owner!, [new PotionReward(Owner!)]);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.POTIONS_CHOSEN.description"));
    }

    // ── 选择卡牌奖励 ──
    private async Task ChooseCards()
    {
        await RewardsCmd.OfferCustom(Owner!, [new CardReward(CardCreationOptions.ForNonCombatWithDefaultOdds([Owner!.Character.CardPool]), 3, Owner)]);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.CARDS_CHOSEN.description"));
    }
}
```

**关键点：**

- 必须加 `[RegisterActEvent(typeof(XXX))]` 指定章节，或 `[RegisterSharedEvent]` 注册为通用事件。
- 继承 `ModEventTemplate`（不是 `EventModel`）。
- 选项方法名会自动转换为本地化键名（如 `TakeDamage` → `TAKE_DAMAGE`）。

---

## 2. 事件注册方式

### 2.1 章节限定事件

```csharp
[RegisterActEvent(typeof(Overgrowth))]  // 只在密林章节生成
public sealed class YourEvent : ModEventTemplate { ... }
```

可用的章节类型：

| 章节 | 类型 |
|------|------|
| 密林 | `typeof(Overgrowth)` |
| 荣耀 | `typeof(Glory)` |
| 其他 | 查看游戏 API 中的 `ActModel` 子类 |

### 2.2 通用事件（自定义条件）

```csharp
[RegisterSharedEvent]
public sealed class YourEvent : ModEventTemplate
{
    // 需要重写 IsAllowed 来定义生成条件
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.All(p => p.Gold >= 60);
}
```

---

## 3. 事件流程控制

### 3.1 生成选项

```csharp
protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
[
    new EventOption(this, MethodName, InitialOptionKey("OPTION_KEY")),
];
```

- `MethodName`：选项执行的方法名
- `OPTION_KEY`：本地化键名后缀（会自动转换为大写蛇形）

### 3.2 切换页面

```csharp
// 切换到新页面
SetEventState(L10NLookup($"{Id.Entry}.pages.PAGE_KEY.description"), [
    new EventOption(this, MethodName, ModOptionKey("PAGE_KEY", "OPTION_KEY")),
]);

// 结束事件
SetEventFinished(L10NLookup($"{Id.Entry}.pages.END_PAGE.description"));
```

### 3.3 常用命令

| 命令 | 说明 |
|------|------|
| `CreatureCmd.Damage(...)` | 造成伤害 |
| `PlayerCmd.LoseGold(...)` | 失去金币 |
| `RewardsCmd.OfferCustom(...)` | 提供奖励 |
| `CardPileCmd.Draw(...)` | 抽牌 |
| `PowerCmd.Apply<T>(...)` | 施加能力 |

---

## 4. 添加本地化文本

编辑 `ZZZMod/localization/zhs/events.json`。

**Key 命名规则：** `ZZZ_MOD_EVENT_类名大写蛇形.key`

### 基本结构

```json
{
    "ZZZ_MOD_EVENT_YOUR_EVENT.title": "事件标题",

    "ZZZ_MOD_EVENT_YOUR_EVENT.pages.INITIAL.description": "初始页面描述文本。",

    "ZZZ_MOD_EVENT_YOUR_EVENT.pages.INITIAL.options.TAKE_DAMAGE.title": "选项标题",
    "ZZZ_MOD_EVENT_YOUR_EVENT.pages.INITIAL.options.TAKE_DAMAGE.description": "选项描述",

    "ZZZ_MOD_EVENT_YOUR_EVENT.pages.CHOOSE_REWARD.description": "第二页面描述文本。",

    "ZZZ_MOD_EVENT_YOUR_EVENT.pages.CHOOSE_REWARD.options.CHOOSE_POTIONS.title": "选择药水",
    "ZZZ_MOD_EVENT_YOUR_EVENT.pages.CHOOSE_REWARD.options.CHOOSE_POTIONS.description": "领取药水奖励。",

    "ZZZ_MOD_EVENT_YOUR_EVENT.pages.CHOOSE_REWARD.options.CHOOSE_CARDS.title": "选择卡牌",
    "ZZZ_MOD_EVENT_YOUR_EVENT.pages.CHOOSE_REWARD.options.CHOOSE_CARDS.description": "领取卡牌奖励。",

    "ZZZ_MOD_EVENT_YOUR_EVENT.pages.POTIONS_CHOSEN.description": "事件结束描述（药水线）。",
    "ZZZ_MOD_EVENT_YOUR_EVENT.pages.CARDS_CHOSEN.description": "事件结束描述（卡牌线）。"
}
```

### DynamicVar 引用格式

| 语法 | 说明 |
|------|------|
| `{Damage}` | 伤害变量值 |
| `{Gold}` | 金币变量值 |
| `[red]文本[/red]` | 红色高亮 |
| `[gold]文本[/gold]` | 金色高亮 |
| `[sine]文本[/sine]` | 波浪动画 |

---

## 5. 补充图片资源

将背景图放置到 `ZZZMod/images/events/` 目录：

| 文件 | 用途 |
|------|------|
| `{event_name}.png` | 事件背景图 |

---

## 6. 构建与测试

```bash
# 编译验证（开发迭代，只看编译是否通过）
dotnet build

# 部署到游戏（DLL + 导出 .pck + 复制到游戏 mods 目录）
./autobuild.bat          # 即 dotnet build -t:ExportPck

# 进入对应章节后，有概率遇到事件
```

---

## 附录：事件属性速查

| 成员 | 类型 | 说明 |
|------|------|------|
| `AssetProfile` | `EventAssetProfile` | 事件背景图（必须重写） |
| `CanonicalVars` | `IEnumerable<DynamicVar>` | 动态变量（可选） |
| `IsAllowed` | `bool` | 生成条件（通用事件时必须重写） |
| `LayoutType` | `EventLayoutType` | 布局类型（战斗事件时使用 `Combat`） |

## 附录：事件状态键名规则

| 键名部分 | 说明 | 示例 |
|---------|------|------|
| `INITIAL` | 初始页面（固定） | `pages.INITIAL.description` |
| `{METHOD_NAME}` | 选项名（自动大写蛇形） | `pages.INITIAL.options.TAKE_DAMAGE.title` |
| `{CUSTOM_KEY}` | 自定义页面名 | `pages.CHOOSE_REWARD.description` |

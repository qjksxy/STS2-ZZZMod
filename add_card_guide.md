# 添加卡牌流程（zzz-mod）

> 基于 STS2-RitsuLib 框架，参考 `InvestCard` 的创建过程。

---

## 1. 创建卡牌代码

在 `Code/Cards/` 下按稀有度创建子目录，放置卡牌的 `.cs` 文件。

**路径示例：** `Code/Cards/Rare/InvestCard.cs`

**模板：**

```csharp
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ZZZMod.Code.Cards.Rare;

[RegisterCard(typeof(ColorlessCardPool))]
public class YourCard() : ZZZBaseCard(
    2,                    // 费用
    CardType.Skill,       // 类型：Attack / Skill / Power
    CardRarity.Rare,      // 稀有度：Common / Uncommon / Rare
    TargetType.Self,      // 目标：Self / AnyEnemy / AnyAlly
    true                  // 是否显示在图鉴中
)
{
    // ── 动态变量（所有数值都应通过 DynamicVar 定义，避免硬编码魔法值）──
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("InvestAmount", 1m)
    ];

    // ── 悬浮提示（引用关联的能力、卡牌或关键词，鼠标悬停时显示）──
    protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
        HoverTipFactory.FromPower<InvestPower>()
    ];

    // ── 打出效果 ──
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = DynamicVars["InvestAmount"].IntValue;
        await ApplyPowerSelf<InvestPower>(amount);
    }

    // ── 升级效果 ──
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
```

**关键点：**

- 必须加 `[RegisterCard(typeof(XXXPool))]`，通过 `ModTypeDiscoveryHub` 自动注册。
- 卡池选择：`ColorlessCardPool`（无色通用），角色专属池需自定义。
- 基类 `ZZZBaseCard` 自动处理卡图路径：`res://ZZZMod/images/cards/{ClassName}.png`。

---

### 1.1 动态变量（CanonicalVars）

**所有数值都应定义为动态变量，禁止在逻辑中直接写魔法值。** 动态变量会被本地化系统自动引用，并在升级、显示等场景中统一管理。

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars => [
    new DamageVar(12, ValueProp.Move),       // 伤害变量（含伤害属性）
    new DynamicVar("BonusDamage", 4m),       // 普通数值变量
    new DynamicVar("InvestAmount", 1m)       // 自定义命名变量
];
```

在 `OnPlay` 中通过 `DynamicVars["变量名"]` 取值：

```csharp
var dmg = DynamicVars.Damage;               // 内置 Damage 变量
var amount = DynamicVars["InvestAmount"].IntValue;  // 自定义变量
```

在 `OnUpgrade` 中修改变量值：

```csharp
DynamicVars.Damage.UpgradeValueBy(4);              // 伤害 +4
DynamicVars["InvestAmount"].UpgradeValueBy(1m);    // 自定义变量 +1
EnergyCost.UpgradeBy(-1);                          // 降 1 费
AddKeyword(CardKeyword.Innate);                    // 添加固有关键词
```

---

### 1.2 悬浮提示（OwnAdditionalHoverTips）

卡牌涉及指定能力、Token 卡牌或关键词时，应添加悬浮提示让玩家可以悬停查看详情。

```csharp
protected override IEnumerable<IHoverTip> OwnAdditionalHoverTips => [
    HoverTipFactory.FromCard<Shiv>(),               // 引用指定卡牌
    HoverTipFactory.FromPower<InvestPower>(),       // 引用指定能力
    HoverTipFactory.FromKeyword(CardKeyword.Exhaust) // 引用游戏关键词
];
```

> **注意：** ZZZBaseCard 已将 `AdditionalHoverTips` 密封，子类应使用 `OwnAdditionalHoverTips`。

---

### 1.3 自定义标签（OwnKeywordIds）

如果卡牌使用了自定义标签（如「满盈」），需要在 `OwnKeywordIds` 中声明，以便关键词系统识别并正确渲染。

```csharp
protected override IEnumerable<string> OwnKeywordIds => [
    ZZZModKeywords.Overflow
];
```

标签定义在 `Code/Cards/ZZZKeywords.cs` 中，使用 `[RegisterOwnedCardKeyword]` 特性注册：

```csharp
[RegisterOwnedCardKeyword(nameof(Overflow), IconPath = "res://icon.svg")]
public class ZZZModKeywords
{
    public static readonly string Overflow =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Overflow));
}
```

> 如果卡牌不涉及自定义标签，此项可省略。

---

### 1.4 ZZZBaseCard 常用辅助方法

| 方法 | 用途 |
|------|------|
| `ApplyPowerSelf<T>(amount)` | 为自己施加能力 |
| `ApplyPowerTo<T>(target, amount)` | 为目标施加能力 |
| `DealDamage(context, damageVar, target)` | 对单体造成伤害 |
| `GainBlockRaw(amount)` | 为自己获得格挡 |
| `GainEnergy(amount)` | 获得能量 |
| `ShouldTriggerOverflow()` | 满盈关键词条件判定 |

---

## 2. 添加本地化文本

编辑 `ZZZMod/localization/zhs/cards.json`，按 `{ModId}_{类别}_{类名大写蛇形}` 格式添加条目。

**Key 命名规则：** `ZZZ_MOD_CARD_类名大写蛇形.key`

| Key 后缀 | 说明 | 示例 |
|-----------|------|------|
| `.title` | 卡牌名称 | `"投资"` |
| `.description` | 卡牌描述 | `"获得{InvestAmount:diff()}层[gold]投资[/gold]。"` |

**DynamicVar 引用格式：**

| 语法 | 说明 | 示例 |
|------|------|------|
| `{Damage:diff()}` | 伤害变量 | `造成{Damage:diff()}点伤害。` |
| `{Block:diff()}` | 格挡变量 | `获得{Block:diff()}点格挡。` |
| `{变量名:diff()}` | 自定义变量 | `获得{InvestAmount:diff()}层[gold]投资[/gold]。` |
| `[gold]文本[/gold]` | 金色高亮 | `[gold]投资[/gold]` |
| `[blue]文本[/blue]` | 蓝色高亮（数值区） | — |

---

## 3. 补充卡图（可选）

将卡图放置到 `ZZZMod/images/cards/`，命名为 `{ClassName}.png`。

`ZZZBaseCard` 已默认配置此路径，无需额外代码。

---

## 4. 构建与测试

```bash
# 编译并复制到游戏 mods 目录
dotnet build

# 编译并导出 .pck（需要 Godot 编辑器）
dotnet build -t:ExportPck
```

---

## 附录：可重写的关键成员速查

| 成员 | 用途 | 示例 |
|------|------|------|
| `CanonicalVars` | 定义所有数值变量 | `new DynamicVar("Amount", 1m)` |
| `OwnAdditionalHoverTips` | 关联能力/卡牌/关键词提示 | `HoverTipFactory.FromPower<T>()` |
| `OwnKeywordIds` | 声明自定义标签 | `ZZZModKeywords.Overflow` |
| `OnPlay` | 卡牌打出逻辑 | `choiceContext`, `cardPlay` |
| `OnUpgrade` | 升级效果 | `EnergyCost.UpgradeBy(-1)` |
| `CanonicalKeywords` | 游戏内建关键词 | `CardKeyword.Exhaust`, `CardKeyword.Innate` |
| `ShouldGlowGold` | 手牌金色高亮条件 | `bool` |

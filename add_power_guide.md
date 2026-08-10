# 添加新能力（Power）流程（zzz-mod）

> 基于 STS2-RitsuLib 框架，参考 `InvestPower` 的实现。

---

## 1. 创建能力代码

在 `Code/Powers/` 目录下创建新的 `.cs` 文件。

**路径示例：** `Code/Powers/InvestPower.cs`

**模板：**

```csharp
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ZZZMod.Code.Powers;

/// <summary>
///     能力名称 —— 简短描述效果。
///     可堆叠说明（如适用）。
/// </summary>
[RegisterPower]
public sealed class YourPower : ModPowerTemplate
{
    // ── 能力类型 ──
    public override PowerType Type => PowerType.Buff;  // Buff 或 Debuff

    // ── 叠加类型 ──
    public override PowerStackType StackType => PowerStackType.Counter;  // Counter 可叠加，Single 不可叠加

    // ── 是否允许负数层数（可选）──
    public override bool AllowNegative => false;

    // ── 图标资源 ──
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/your_power.png",       // 小图 64x64
        BigIconPath: "res://ZZZMod/images/powers/your_power_big.png" // 大图 256x256
    );

    // ── 效果钩子（按需重写）──
    // 以下为常用钩子，选择需要的时机重写即可。

    /// <summary>回合结束时触发。</summary>
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;  // 只在拥有者回合结束时生效
        if (Amount <= 0) return;

        // 执行效果，例如获得力量
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
    }

    /// <summary>抽牌后触发。</summary>
    // public override async Task AfterCardDrawn(
    //     PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    // {
    //     // 执行效果
    // }
}
```

**关键点：**

- 必须加 `[RegisterPower]`，通过 `ModTypeDiscoveryHub` 自动注册。
- 继承 `ModPowerTemplate`（不是 `PowerModel`）。
- `AssetProfile` 的 `IconPath`（小图）和 `BigIconPath`（大图）分别对应不同尺寸。

---

## 2. 能力属性配置

### 2.1 PowerType（能力类型）

| 值 | 说明 | 示例 |
|----|------|------|
| `PowerType.Buff` | 正面效果（绿色） | 力量、敏捷 |
| `PowerType.Debuff` | 负面效果（红色） | 虚弱、易伤 |

### 2.2 PowerStackType（叠加类型）

| 值 | 说明 | 示例 |
|----|------|------|
| `PowerStackType.Counter` | 可叠加，显示层数 | 力量、敏捷 |
| `PowerStackType.Single` | 不可叠加，只显示有/无 | 某些特殊状态 |

### 2.3 AllowNegative（允许负数）

- `true`：层数可以为负数（用于某些特殊机制）
- `false`：层数最小为 0（默认推荐）

---

## 3. 效果钩子（Hooks）

在 `ModPowerTemplate` 中重写以下方法来实现效果：

| 钩子方法 | 触发时机 | 参数说明 |
|---------|---------|---------|
| `AfterSideTurnEnd` | 某方回合结束时 | `side`: 当前回合方，`participants`: 参与者列表 |
| `AfterCardDrawn` | 抽牌后 | `card`: 抽到的牌，`fromHandDraw`: 是否从手牌堆抽取 |
| `AfterTurnStart` | 回合开始时 | `choiceContext` |
| `BeforeTurnStart` | 回合开始前 | `choiceContext` |
| `AfterAttackHit` | 攻击命中后 | `attacker`, `target`, `damage`, `isCrit` |

**常用模式：**

```csharp
// 只在拥有者回合结束时生效
public override async Task AfterSideTurnEnd(
    PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
{
    if (side != Owner.Side) return;
    // 执行效果
}

// 每次抽牌时生效
public override async Task AfterCardDrawn(
    PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
{
    // 执行效果
}
```

---

## 4. 施加能力的 API

使用 `PowerCmd.Apply<T>()` 给目标施加能力：

```csharp
// 基本签名
await PowerCmd.Apply<TPower>(
    PlayerChoiceContext choiceContext,  // 上下文
    Creature target,                    // 目标
    int amount,                         // 层数
    Creature? applier,                  // 施加者（通常是 Owner）
    CardModel? cardSource               // 来源卡牌（可为 null）
);

// 示例：给自身施加力量
await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);

// 示例：给目标施加虚弱
await PowerCmd.Apply<WeakPower>(choiceContext, target, Amount, Owner, null);
```

---

## 5. 添加本地化文本

编辑 `ZZZMod/localization/zhs/powers.json`。

**Key 命名规则：** `ZZZ_MOD_POWER_类名大写蛇形.key`

| Key 后缀 | 说明 | 示例 |
|-----------|------|------|
| `.title` | 能力名称 | `"投资"` |
| `.description` | 基础描述 | `"每回合结束时，获得1层力量。"` |
| `.smartDescription` | 动态描述（可选） | `"每回合结束时，获得[blue]{Amount}[/blue]层力量。"` |

**DynamicVar 引用格式：**

| 语法 | 说明 |
|------|------|
| `{Amount}` | 当前层数（仅在 `smartDescription` 中可用） |
| `[blue]文本[/blue]` | 蓝色高亮 |
| `[gold]文本[/gold]` | 金色高亮 |

**示例：**

```json
{
    "ZZZ_MOD_POWER_INVEST_POWER.title": "投资",
    "ZZZ_MOD_POWER_INVEST_POWER.description": "每回合结束时，获得1层力量和1层虚弱。",
    "ZZZ_MOD_POWER_INVEST_POWER.smartDescription": "每回合结束时，获得[blue]{Amount}[/blue]层力量和[blue]{Amount}[/blue]层虚弱。"
}
```

---

## 6. 补充图标资源

将图标放置到 `ZZZMod/images/powers/` 目录：

| 文件 | 尺寸 | 用途 |
|------|------|------|
| `{power_name}.png` | 64x64 | 小图标（战斗界面显示） |
| `{power_name}_big.png` | 256x256 | 大图标（详情界面显示） |

---

## 7. 构建与测试

```bash
# 编译并复制到游戏 mods 目录
dotnet build

# 使用控制台指令测试（战斗中按 ~ 打开控制台）
# power ZZZ_MOD_POWER_YOUR_POWER 1 0
# 格式：power {能力ID} {层数} {目标(0=自身,1=敌人)}
```

---

## 8. 临时能力（TempPower）

如果能力是临时的（回合结束自动消失），使用 `ModTemporaryAppliedPowerTemplate`：

```csharp
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Cards;

namespace ZZZMod.Code.Powers;

/// <summary>
///     临时能力 —— 由某张卡牌施加，回合结束自动消失。
/// </summary>
[RegisterPower]
public class TempFromYourCardPower : ModTemporaryAppliedPowerTemplate<YourCard, StrengthPower>
{
    // 两个泛型参数：
    // - 第一个：来源卡牌类型（谁施加的）
    // - 第二个：代表哪个原版能力的临时效果

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://ZZZMod/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://ZZZMod/images/powers/{GetType().Name}_big.png"
    );

    // 可选配置：
    // protected override bool IsPositive => true;              // 正面还是负面效果
    // protected override bool UntilEndOfOtherSideTurn => false; // 在哪方回合结束时过期
    // protected override int LastForXExtraTurns => 0;          // 额外持续回合数
}
```

---

## 附录：能力属性速查

| 成员 | 类型 | 说明 | 默认值 |
|------|------|------|--------|
| `Type` | `PowerType` | Buff 或 Debuff | 必须重写 |
| `StackType` | `PowerStackType` | Counter（可叠加）或 Single（不可叠加） | 必须重写 |
| `AllowNegative` | `bool` | 是否允许负数层数 | `false` |
| `AssetProfile` | `PowerAssetProfile` | 图标资源路径 | 必须重写 |
| `Amount` | `int` | 当前层数（只读） | — |
| `Owner` | `Creature` | 能力拥有者（只读） | — |

## 附录：常用原版能力类型

| 能力 | 说明 |
|------|------|
| `StrengthPower` | 力量（增加攻击伤害） |
| `WeakPower` | 虚弱（减少攻击伤害） |
| `VulnerablePower` | 易伤（受到更多伤害） |
| `DexterityPower` | 敏捷（增加格挡值） |
| `ThornsPower` | 荆棘（受击时反弹伤害） |
| `MetallicizePower` | 金属化（回合结束获得格挡） |
| `RegeneratePower` | 再生（回合开始恢复生命） |
| `PoisonPower` | 中毒（回合开始受到毒素伤害） |

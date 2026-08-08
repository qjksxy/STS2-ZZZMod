# 添加新遗物（Relic）流程（zzz-mod）

> 基于 STS2-RitsuLib 框架，参考 `ZZZStarterRelic` 的实现。

---

## 1. 创建遗物代码

在 `Code/Relics/` 目录下创建新的 `.cs` 文件。

**路径示例：** `Code/Relics/ZZZStarterRelic.cs`

**模板：**

```csharp
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Character;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Relics;

/// <summary>
///     遗物名称 —— 简短描述效果。
/// </summary>
[RegisterRelic(typeof(ZZZRelicPool))]
// [RegisterCharacterStarterRelic(typeof(ZZZCharacter))]  // 注册为初始遗物（可选）
public sealed class YourRelic : ModRelicTemplate
{
    // ── 稀有度 ──
    public override RelicRarity Rarity => RelicRarity.Common;

    // ── 图标资源 ──
    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/relics/your_relic.png",           // 小图标 85x85
        IconOutlinePath: "res://ZZZMod/images/relics/your_relic.png",    // 轮廓图标 85x85
        BigIconPath: "res://ZZZMod/images/relics/your_relic_big.png"     // 大图标 256x256
    );

    // ── 效果钩子（按需重写）──

    /// <summary>每回合开始时触发。</summary>
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 例如：抽 1 张牌
        await CardPileCmd.Draw(choiceContext, 1, player);
    }
}
```

**关键点：**

- 必须加 `[RegisterRelic(typeof(XXXPool))]`，通过 `ModTypeDiscoveryHub` 自动注册。
- 继承 `ModRelicTemplate`（不是 `RelicModel`）。
- 如果是初始遗物，添加 `[RegisterCharacterStarterRelic(typeof(ZZZCharacter))]`。

---

## 2. 遗物属性配置

### 2.1 RelicRarity（稀有度）

| 值 | 说明 |
|----|------|
| `RelicRarity.Starter` | 初始遗物（角色自带） |
| `RelicRarity.Common` | 普通遗物 |
| `RelicRarity.Uncommon` | 稀有遗物 |
| `RelicRarity.Rare` | 史诗遗物 |
| `RelicRarity.Shop` | 商店遗物 |
| `RelicRarity.Boss` | Boss 遗物 |
| `RelicRarity.Special` | 特殊遗物 |

---

## 3. 效果钩子（Hooks）

在 `ModRelicTemplate` 中重写以下方法来实现效果：

| 钩子方法 | 触发时机 | 参数说明 |
|---------|---------|---------|
| `AfterPlayerTurnStart` | 玩家回合开始时 | `player`: 当前玩家 |
| `AfterPlayerTurnEnd` | 玩家回合结束时 | `player`: 当前玩家 |
| `OnCombatStarted` | 战斗开始时 | `choiceContext` |
| `OnCombatEnded` | 战斗结束时 | `choiceContext` |
| `OnCardPlayed` | 打出卡牌后 | `choiceContext`, `cardPlay` |
| `OnAttacked` | 受到攻击后 | `choiceContext`, `attacker`, `damage` |

**常用模式：**

```csharp
// 每回合开始时抽牌
public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
{
    await CardPileCmd.Draw(choiceContext, 1, player);
}

// 战斗开始时获得力量
public override async Task OnCombatStarted(PlayerChoiceContext choiceContext)
{
    await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
}
```

---

## 4. 添加本地化文本

编辑 `ZZZMod/localization/zhs/relics.json`（如不存在需创建）。

**Key 命名规则：** `ZZZ_MOD_RELIC_类名大写蛇形.key`

| Key 后缀 | 说明 | 示例 |
|-----------|------|------|
| `.title` | 遗物名称 | `"耐心之证"` |
| `.description` | 遗物效果描述 | `"每回合开始时，抽[blue]{Cards}[/blue]张牌。"` |
| `.flavor` | 风味文本（可选） | `"等待本身就是一种力量。"` |

**示例：**

```json
{
    "ZZZ_MOD_RELIC_YOUR_RELIC.title": "耐心之证",
    "ZZZ_MOD_RELIC_YOUR_RELIC.description": "每回合开始时，抽1张牌。",
    "ZZZ_MOD_RELIC_YOUR_RELIC.flavor": "等待本身就是一种力量。"
}
```

---

## 5. 补充图标资源

将图标放置到 `ZZZMod/images/relics/` 目录：

| 文件 | 尺寸 | 用途 |
|------|------|------|
| `{relic_name}.png` | 85x85 | 小图标（战斗界面显示） |
| `{relic_name}_big.png` | 256x256 | 大图标（详情界面显示） |

---

## 6. 构建与测试

```bash
# 编译并复制到游戏 mods 目录
dotnet build

# 使用控制台指令测试（战斗中按 ~ 打开控制台）
# relics ZZZ_MOD_RELIC_YOUR_RELIC
```

---

## 附录：遗物属性速查

| 成员 | 类型 | 说明 |
|------|------|------|
| `Rarity` | `RelicRarity` | 稀有度（必须重写） |
| `AssetProfile` | `RelicAssetProfile` | 图标资源路径（必须重写） |
| `CanonicalVars` | `IEnumerable<DynamicVar>` | 动态变量（可选，用于本地化中的数值引用） |

## 附录：常用原版遗物类型

| 遗物 | 效果 |
|------|------|
| `Akabeko` | 第一次攻击造成额外伤害 |
| `BurningBlood` | 每场战斗后恢复生命 |
| `Vajra` | 战斗开始时获得力量 |
| `OddMushroom` | 战斗开始时获得敏捷 |
| `Orichalcum` | 回合结束时如果没有格挡，获得格挡 |

# 添加新药水（Potion）流程（zzz-mod）

> 基于 STS2-RitsuLib 框架。

---

## 1. 创建药水代码

在 `Code/Potions/` 目录下创建新的 `.cs` 文件（如目录不存在需创建）。

**路径示例：** `Code/Potions/YourPotion.cs`

**模板：**

```csharp
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Potions;

/// <summary>
///     药水名称 —— 简短描述效果。
/// </summary>
[RegisterPotion(typeof(ZZZPotionPool))]
public sealed class YourPotion : ModPotionTemplate
{
    // ── 稀有度 ──
    public override PotionRarity Rarity => PotionRarity.Common;

    // ── 使用方式 ──
    public override PotionUsage Usage => PotionUsage.CombatOnly;  // CombatOnly / OutOfCombatOnly / Anytime

    // ── 目标类型 ──
    public override TargetType TargetType => TargetType.Self;  // Self / AnyEnemy / AnyAlly

    // ── 动态变量 ──
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3)  // 生成 3 张卡牌
    ];

    // ── 悬浮提示（可选）──
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<Soul>()  // 显示灵魂卡牌预览
    ];

    // ── 图标资源 ──
    public override PotionAssetProfile AssetProfile => new(
        ImagePath: "res://ZZZMod/images/potions/your_potion.png",       // 药水本体图
        OutlinePath: "res://ZZZMod/images/potions/your_potion.png"      // 轮廓图
    );

    // ── 使用效果 ──
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        // 例如：生成 3 张灵魂到手牌
        await Soul.CreateInHand(Owner, DynamicVars.Cards.IntValue, Owner.Creature.CombatState!);
    }
}
```

**关键点：**

- 必须加 `[RegisterPotion(typeof(XXXPool))]`，通过 `ModTypeDiscoveryHub` 自动注册。
- 继承 `ModPotionTemplate`（不是 `PotionModel`）。

---

## 2. 药水属性配置

### 2.1 PotionRarity（稀有度）

| 值 | 说明 |
|----|------|
| `PotionRarity.Common` | 普通药水 |
| `PotionRarity.Uncommon` | 稀有药水 |
| `PotionRarity.Rare` | 史诗药水 |
| `PotionRarity.Colorless` | 无色药水 |
| `PotionRarity.Healing` | 治疗药水 |

### 2.2 PotionUsage（使用方式）

| 值 | 说明 |
|----|------|
| `PotionUsage.CombatOnly` | 只能在战斗中使用 |
| `PotionUsage.OutOfCombatOnly` | 只能在战斗外使用 |
| `PotionUsage.Anytime` | 随时可用 |

### 2.3 TargetType（目标类型）

| 值 | 说明 |
|----|------|
| `TargetType.Self` | 对自身使用 |
| `TargetType.AnyEnemy` | 对任意敌人使用 |
| `TargetType.AnyAlly` | 对任意友方使用 |

---

## 3. 使用效果 API

在 `OnUse` 方法中实现药水效果：

```csharp
protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
{
    // 施加能力
    await PowerCmd.Apply<StrengthPower>(choiceContext, target, 2, Owner.Creature, null);

    // 造成伤害
    await DamageCmd.Attack(10).FromPotion(this).Targeting(target).Execute(choiceContext);

    // 获得格挡
    await CreatureCmd.GainBlock(target, new BlockVar(5, ValueProp.Move), null);

    // 抽牌
    await CardPileCmd.Draw(choiceContext, 2, Owner);

    // 获得能量
    await PlayerCmd.GainEnergy(1, Owner);
}
```

---

## 4. 添加本地化文本

编辑 `ZZZMod/localization/zhs/potions.json`（如不存在需创建）。

**Key 命名规则：** `ZZZ_MOD_POTION_类名大写蛇形.key`

| Key 后缀 | 说明 | 示例 |
|-----------|------|------|
| `.title` | 药水名称 | `"时间之水"` |
| `.description` | 药水效果描述 | `"获得[blue]{Cards}[/blue]张[gold]灵魂[/gold]。"` |

**示例：**

```json
{
    "ZZZ_MOD_POTION_YOUR_POTION.title": "时间之水",
    "ZZZ_MOD_POTION_YOUR_POTION.description": "将[blue]{Cards}[/blue]张[gold]灵魂[/gold]加入你的[gold]手牌[/gold]。"
}
```

---

## 5. 补充图片资源

将药水图放置到 `ZZZMod/images/potions/` 目录：

| 文件 | 用途 |
|------|------|
| `{potion_name}.png` | 药水本体图 |
| `{potion_name}_outline.png` | 轮廓图（可选，可复用本体图） |

---

## 6. 构建与测试

```bash
# 编译验证（开发迭代，只看编译是否通过）
dotnet build

# 部署到游戏（DLL + 导出 .pck + 复制到游戏 mods 目录）
./autobuild.bat          # 即 dotnet build -t:ExportPck

# 使用控制台指令测试（战斗中按 ~ 打开控制台）
# potion ZZZ_MOD_POTION_YOUR_POTION
```

---

## 附录：药水属性速查

| 成员 | 类型 | 说明 |
|------|------|------|
| `Rarity` | `PotionRarity` | 稀有度（必须重写） |
| `Usage` | `PotionUsage` | 使用方式（必须重写） |
| `TargetType` | `TargetType` | 目标类型（必须重写） |
| `CanonicalVars` | `IEnumerable<DynamicVar>` | 动态变量（可选） |
| `AssetProfile` | `PotionAssetProfile` | 图标资源路径（必须重写） |
| `AdditionalHoverTips` | `IEnumerable<IHoverTip>` | 悬浮提示（可选） |

## 附录：常用原版药水类型

| 药水 | 效果 |
|------|------|
| `FirePotion` | 造成伤害 |
| `Block Potion` | 获得格挡 |
| `StrengthPotion` | 获得力量 |
| `DexterityPotion` | 获得敏捷 |
| `EnergyPotion` | 获得能量 |
| `FairyPotion` | 死亡时复活 |

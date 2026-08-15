# ZZZ Mod — 杀戮尖塔2 模组

> 一个以「等待」与「时间」为主题的 Slay the Spire 2 角色模组。

[![Godot](https://img.shields.io/badge/Godot-4.5-blue)](https://godotengine.org/)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![RitsuLib](https://img.shields.io/badge/RitsuLib-0.5.12-green)](https://github.com/BAKAOLC/STS2-RitsuLib)
[![License](https://img.shields.io/badge/License-MIT-yellow)](LICENSE)

---

## 📖 项目简介

ZZZ Mod 为《杀戮尖塔 2》添加了一名新角色——**法厄同（Phaethon）**。

> *一个在无尽等待中的存在。*
> *时间对法厄同而言，不过是另一种形式的永恒。*

### 角色特性

| 属性 | 值 |
|------|-----|
| **角色名** | 法厄同（Phaethon） |
| **初始生命** | 80 |
| **初始金币** | 99 |
| **能量颜色** | 蓝紫色 |
| **战斗机制** | 失衡（Daze）+ 喧响（Decibel） |
| **专属关键词** | 满盈（Overflow）、喧响（Decibel） |

---

## ⚔️ 战斗机制

### 失衡（Daze）

> 玩家的核心资源之一：打出攻击牌会**累积敌人目标的失衡值**，累积满后敌人进入失衡状态。

- 失衡条上限 = `max(4, 怪物最大生命值 / 12)`，仅对怪物生效
- 每次攻击命中使目标失衡值 +N（默认 +1，卡牌可通过 `IDazeCardSource.DazeAmount` 自定义）
- 失衡值累积满 → 下一回合怪物进入**失衡状态**（受到伤害 +50%，失衡条变紫）
- 失衡状态持续一回合后恢复，失衡条归零重新累积
- 有「失衡易伤」时失衡期间受伤提升至 +100%
- 失衡伤害修正不经过 Power 系统，**不受人工制品**影响；失衡易伤本身是标准 Power，可被人工作品抵消
- 失衡条打满时获得喧响（失衡上限的一半）

### 喧响（Decibel）

> 渐进式资源：通过出牌自然积累，蓄满后由卡牌消耗换取强效果。

| 参数 | 值 |
|------|-----|
| 喧响上限 | 50 |
| 默认消耗（`IDecibelCardSource`） | 20 |
| 打出 Basic / Common / Uncommon / Rare 卡 | +1 / +2 / +3 / +4 |
| 击杀敌人 | +5 |
| 失衡条打满 | +失衡上限的一半 |
| 跨战斗持久化 | 是（RunSavedData） |

- 消耗喧响的卡牌实现 `IDecibelCardSource` 接口，通过 `DecibelSystem.TrySpendDecibel(cost)` 消耗
- 战斗中通过玩家血条旁的圆形表盘 UI 显示

### 满盈（Overflow）

> 生命值满时生效。带有「满盈」标签的卡牌在满血时触发额外效果，手牌中会发黄光提示。

---

## 🎴 已实现内容

### 卡牌（11 张）

| 卡牌 | 稀有度 | 费用 | 类型 | 效果 |
|------|--------|------|------|------|
| 打击 | Basic | 1 | 攻击 | 造成 6 点伤害（升级 +3） |
| 防御 | Basic | 1 | 技能 | 获得 5 点格挡（升级 +3） |
| 测试卡牌 | Common | 1 | 攻击 | 造成 12 点伤害，满盈时额外造成 4 点 |
| 时之锋 | Common | 1 | 攻击 | 造成 8 点伤害，每次攻击失衡 +2 |
| 时间障壁 | Common | 1 | 技能 | 获得 7 点格挡，本回合受击时攻击者失衡 +1 |
| 时序连击 | Uncommon | 1 | 攻击 | 造成 5 点伤害，每次攻击失衡 +3 |
| 悖论冲击 | Uncommon | 2 | 攻击 | 造成 12 点伤害，若目标正在失衡则伤害翻倍 |
| 醉花月云转 | Uncommon | 1 | 攻击 | 造成 6 点伤害并施加 3 点失衡，归零时施加失衡易伤 |
| 八声甘州 | Uncommon | 2 | 攻击 | 对全体敌人造成 4 点伤害并施加 3 点失衡 |
| 投资 | Rare | 2 | 技能 | 获得 1 层「投资」（升级后 1 费） |
| 时光碎裂 | Rare | 2 | 技能 | 对目标造成 999 点失衡（升级后 1 费） |

### 能力（Power）

| 能力 | 类型 | 效果 |
|------|------|------|
| 投资 | Buff | 每回合结束时，获得等同于层数的力量和虚弱 |
| 失衡 | Debuff | 失衡状态的纯视觉标记（图标显示） |
| 失衡易伤 | Debuff | 失衡期间受到的额外伤害提升 50% |
| 时间障壁 | Buff | 本回合受到攻击时，攻击者失衡 +1（回合结束移除） |

### 事件

| 事件 | 章节 | 描述 |
|------|------|------|
| 与戈多相遇 | 密林（Overgrowth） | 在岔路口遇到等待中的戈多，选择失去生命或金币，换取药水或卡牌奖励 |

### 其他

- **初始遗物**：法厄同专属起始遗物
- **关键词**：满盈（Overflow）、喧响（Decibel）

---

## 📋 卡牌设计流水线

所有准备实现的卡牌以 **`docs/Cards.csv` 为唯一实现依据**：

1. 在 `docs/Cards.csv` 填写卡牌行（`ID` 列使用英文 Title Case，如 `Charged Beat`）
2. 运行 `python docs/csv_to_cards.py`，自动生成 `ZZZMod/localization/zhs/cards_generated.json` 本地化
3. 在 `Code/Cards/{Rarity}/` 下编写卡牌类，并将本地化并入 `cards.json`

> 约定：`docs/` 下的设计稿（如 `decibel_cards_design.md`、`daze_system.md`）只是前期设计参考，不作为实现方案；
> `Cards.csv` 的「对应角色」列仅供卡图绘制参考，开发时忽略。

---

## 🛠️ 开发环境

### 前置要求

- **Godot 4.5**（Mono 版本，支持 C#）
- **.NET 9.0 SDK**
- **Slay the Spire 2**（Steam）
- **STS2-RitsuLib 0.5.12** 框架

### 安装步骤

1. **克隆仓库**

```bash
git clone https://github.com/你的用户名/zzz-mod.git
cd zzz-mod
```

2. **安装依赖**

确保已安装 .NET 9.0 SDK，NuGet 包会自动还原。

3. **配置游戏路径**

编辑 `ZZZMod.csproj`，修改以下路径指向你的游戏安装目录：

```xml
<PropertyGroup>
    <Sts2Dir>C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2</Sts2Dir>
    <Sts2DataDir>$(Sts2Dir)\data_sts2_windows_x86_64</Sts2DataDir>
    <GodotExe>你的Godot路径\Godot_v4.5.1-stable_mono_win64.exe</GodotExe>
</PropertyGroup>
```

4. **安装 RitsuLib**

从 [GitHub Releases](https://github.com/BAKAOLC/STS2-RitsuLib/releases) 下载 RitsuLib 0.5.12，解压到游戏的 `mods/` 目录。

5. **编译与部署**

```bash
# 编译验证（开发迭代）：只看编译是否通过
dotnet build

# 部署到游戏：编译 DLL + 导出 .pck + 复制到游戏 mods 目录
./autobuild.bat          # 即 dotnet build -t:ExportPck
```

> `dotnet build` 用于编译验证；其 PostBuild 复制步骤在游戏运行时可能报 Access denied，不影响编译结果，验证时忽略即可。

---

## 📁 项目结构

```
zzz-mod/
├── Code/                           # C# 源代码
│   ├── Entry.cs                    # 模组入口点（注册装配 + Harmony 补丁 + 系统初始化）
│   ├── Cards/                      # 卡牌定义
│   │   ├── ZZZBaseCard.cs          # 卡牌基类（实现 IDazeCardSource）
│   │   ├── ZZZKeywords.cs          # 关键词定义（满盈、喧响）
│   │   ├── Basic/                  # 基础卡牌（打击、防御）
│   │   ├── Common/                 # 普通卡牌（测试卡牌、时之锋、时间障壁）
│   │   ├── Uncommon/               # 罕见卡牌（时序连击、悖论冲击、醉花月云转、八声甘州）
│   │   └── Rare/                   # 稀有卡牌（投资、时光碎裂）
│   ├── Character/
│   │   └── character.cs            # 角色定义（法厄同）
│   ├── Daze/                       # 失衡系统
│   │   ├── DazeState.cs            # 状态模型 + DazeStore 全局存储
│   │   ├── DazeSystem.cs           # 系统入口：生命周期 + 状态推进
│   │   ├── DazeHitListener.cs      # 攻击命中钩子：累积失衡值
│   │   ├── DazeDamageModifier.cs   # 伤害修正：失衡目标受伤 +50%
│   │   ├── DazeBarPatch.cs         # 失衡条 UI（血条下方）
│   │   ├── DazePower.cs            # 失衡视觉标记
│   │   └── DazeVulnerablePower.cs  # 失衡易伤
│   ├── Decibel/                    # 喧响系统
│   │   ├── DecibelData.cs          # 数据模型（上限/消耗）
│   │   ├── DecibelSystem.cs        # 系统入口：获得/消耗/持久化
│   │   ├── DecibelBarPatch.cs      # 喧响圆形表盘 UI
│   │   └── IDecibelCardSource.cs   # 喧响卡牌接口
│   ├── Events/
│   │   └── TestEvent.cs            # 事件定义（与戈多相遇）
│   ├── Pools/                      # 卡池定义
│   │   ├── ZZZCardPool.cs          # 卡牌池
│   │   ├── ZZZRelicPool.cs         # 遗物池
│   │   └── ZZZPotionPool.cs        # 药水池
│   ├── Powers/
│   │   ├── InvestPower.cs          # 投资能力
│   │   └── TimeBarrierPower.cs     # 时间障壁能力
│   └── Relics/
│       └── ZZZStarterRelic.cs      # 初始遗物
├── docs/                           # 开发文档与卡牌流水线（详见 docs/README.md）
│   ├── README.md                   # 文档索引
│   ├── Cards.csv                   # ★ 卡牌设计表（唯一实现依据）
│   ├── csv_to_cards.py             # CSV → 本地化 JSON 生成脚本
│   ├── add_card_guide.md           # 添加卡牌指南
│   ├── add_power_guide.md          # 添加能力指南
│   ├── add_relic_guide.md          # 添加遗物指南
│   ├── add_event_guide.md          # 添加事件指南
│   ├── add_potion_guide.md         # 添加药水指南
│   ├── daze_system.md              # 失衡系统设计（参考）
│   └── decibel_cards_design.md     # 喧响卡牌设计稿（参考）
├── ZZZMod/                         # 游戏资源
│   ├── images/                     # 图片资源（cards / character / events / powers）
│   ├── localization/zhs/           # 简体中文本地化（cards / powers / events / keywords）
│   └── scenes/                     # Godot 场景文件（角色模型、能量表盘、背景）
├── ZZZMod.csproj                   # .NET 项目文件
├── ZZZMod.json                     # 模组清单
├── ZZZMod.sln                      # Visual Studio 解决方案
├── project.godot                   # Godot 项目配置
├── export_presets.cfg              # 导出配置
├── autobuild.bat                   # 部署构建脚本（dotnet build -t:ExportPck）
└── README.md                       # 本文件
```

---

## 🚀 添加新内容

### 添加卡牌

参考 [add_card_guide.md](docs/add_card_guide.md) 获取详细流程，并遵循 [卡牌设计流水线](#-卡牌设计流水线)。

**快速开始：**

```csharp
// Code/Cards/Common/YourCard.cs
[RegisterCard(typeof(ZZZCardPool))]
public class YourCard() : ZZZBaseCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
{
    public override int DazeAmount => 2;  // 自定义失衡值

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;
        await DealDamage(choiceContext, DynamicVars.Damage, target);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}
```

然后在 `ZZZMod/localization/zhs/cards.json` 添加本地化：

```json
{
    "ZZZ_MOD_CARD_YOUR_CARD.title": "你的卡牌",
    "ZZZ_MOD_CARD_YOUR_CARD.description": "造成{Damage:diff()}点伤害。"
}
```

### 添加喧响卡牌

卡牌类实现 `IDecibelCardSource` 接口，在 `OnPlay` 中用 `DecibelSystem.TrySpendDecibel(cost)` 判断并消耗喧响：

```csharp
public sealed class YourDecibelCard() : ZZZBaseCard(...), IDecibelCardSource
{
    public int DecibelCost => 20;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!EnsureTarget(cardPlay, out var target)) return;
        if (DecibelSystem.TrySpendDecibel(DecibelCost))
        {
            // 喧响增强效果
        }
        else
        {
            // 基础效果
        }
    }
}
```

### 添加能力

参考 [add_power_guide.md](docs/add_power_guide.md) 获取详细流程。

**快速开始：**

```csharp
// Code/Powers/YourPower.cs
[RegisterPower]
public sealed class YourPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://ZZZMod/images/powers/your_power.png",
        BigIconPath: "res://ZZZMod/images/powers/your_power_big.png"
    );

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        // 执行效果
    }
}
```

---

## 🎮 调试命令

在游戏中按 `~` 打开控制台，使用以下命令：

| 命令 | 说明 |
|------|------|
| `card ZZZ_MOD_CARD_YOUR_CARD` | 获得指定卡牌 |
| `power ZZZ_MOD_POWER_YOUR_POWER 1 0` | 施加指定能力 |
| `gold 999` | 增加金币 |
| `hp 999` | 增加生命值 |

---

## 📝 开发文档

- [开发文档索引](docs/README.md)
- [添加卡牌指南](docs/add_card_guide.md)
- [添加能力指南](docs/add_power_guide.md)
- [失衡系统设计](docs/daze_system.md)
- [喧响卡牌设计稿](docs/decibel_cards_design.md)
- [RitsuLib 官方文档](https://sts2-ritsulib.ritsukage.com/)
- [杀戮尖塔2 Mod 制作教程](https://tutorials.sts2modding.com/)

---

## 🤝 致谢

- [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) — Mod 开发框架
- [杀戮尖塔2 Mod 制作教程](https://tutorials.sts2modding.com/) — 官方教程
- [Slay the Spire 2](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/) — 游戏本体

---

## 📄 许可证

本项目采用 [MIT 许可证](LICENSE)。

---

## 📮 联系方式

- **作者**: Piner
- **Issues**: [GitHub Issues](https://github.com/你的用户名/zzz-mod/issues)

# ZZZ Mod — 杀戮尖塔2 模组

> 一个以「等待」与「时间」为主题的 Slay the Spire 2 角色模组。

[![Godot](https://img.shields.io/badge/Godot-4.5-blue)](https://godotengine.org/)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![RitsuLib](https://img.shields.io/badge/RitsuLib-0.5.10-green)](https://github.com/BAKAOLC/STS2-RitsuLib)
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
| **专属关键词** | 满盈（Overflow） |

### 专属关键词：满盈（Overflow）

> 生命值满时生效。

当角色处于满血状态时，带有「满盈」标签的卡牌会触发额外效果，在手牌中会发黄光提示。

---

## 🎴 已实现内容

### 卡牌

| 卡牌 | 稀有度 | 费用 | 类型 | 效果 |
|------|--------|------|------|------|
| 打击 | Basic | 1 | 攻击 | 造成 6 点伤害 |
| 防御 | Basic | 1 | 技能 | 获得 5 点格挡 |
| 测试卡牌 | Common | 1 | 攻击 | 造成 12 点伤害，满盈时额外造成 4 点 |
| 投资 | Rare | 2 | 技能 | 获得 1 层「投资」能力 |

### 能力（Power）

| 能力 | 类型 | 效果 |
|------|------|------|
| 投资 | Buff | 每回合结束时，获得等同于层数的力量和虚弱 |
| 代偿 | Debuff | 造成伤害时减少相应层数，回合结束时受到剩余层数的伤害 |

### 事件

| 事件 | 章节 | 描述 |
|------|------|------|
| 与戈多相遇 | 密林（Overgrowth） | 在岔路口遇到等待中的戈多，选择失去生命或金币，换取药水或卡牌奖励 |

---

## 🛠️ 开发环境

### 前置要求

- **Godot 4.5**（Mono 版本，支持 C#）
- **.NET 9.0 SDK**
- **Slay the Spire 2**（Steam）
- **STS2-RitsuLib** 框架

### 安装步骤

1. **克隆仓库**

```bash
git clone https://github.com/你的用户名/zzz-mod.git
cd zz-mod
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

从 [GitHub Releases](https://github.com/BAKAOLC/STS2-RitsuLib/releases) 下载 RitsuLib，解压到游戏的 `mods/` 目录。

5. **编译并部署**

```bash
# 编译 DLL 并复制到游戏 mods 目录
dotnet build

# 编译并导出 .pck（需要 Godot 编辑器）
dotnet build -t:ExportPck
```

---

## 📁 项目结构

```
zzz-mod/
├── Code/                           # C# 源代码
│   ├── Entry.cs                    # 模组入口点
│   ├── Cards/                      # 卡牌定义
│   │   ├── ZZZBaseCard.cs          # 卡牌基类
│   │   ├── ZZZKeywords.cs          # 关键词定义
│   │   ├── Basic/                  # 基础卡牌（打击、防御）
│   │   ├── Common/                 # 普通卡牌
│   │   └── Rare/                   # 稀有卡牌
│   ├── Character/
│   │   └── character.cs            # 角色定义
│   ├── Events/
│   │   └── TestEvent.cs            # 事件定义
│   ├── Pools/                      # 卡池定义
│   │   ├── ZZZCardPool.cs          # 卡牌池
│   │   ├── ZZZRelicPool.cs         # 遗物池
│   │   └── ZZZPotionPool.cs        # 药水池
│   ├── Powers/
│   │   └── InvestPower.cs          # 能力定义
│   └── Relics/
│       └── ZZZStarterRelic.cs      # 初始遗物
├── ZZZMod/                         # 游戏资源
│   ├── images/                     # 图片资源
│   │   ├── cards/                  # 卡图
│   │   ├── character/              # 角色立绘、头像
│   │   ├── events/                 # 事件背景
│   │   └── powers/                 # 能力图标
│   ├── localization/
│   │   └── zhs/                    # 简体中文本地化
│   │       ├── cards.json          # 卡牌文本
│   │       ├── character.json      # 角色文本
│   │       ├── events.json         # 事件文本
│   │       ├── powers.json         # 能力文本
│   │       └── card_keywords.json  # 关键词文本
│   └── scenes/                     # Godot 场景文件
│       ├── test_character.tscn     # 角色模型
│       ├── test_energy_counter.tscn # 能量表盘
│       └── test_bg.tscn            # 角色选择背景
├── ZZZMod.csproj                   # .NET 项目文件
├── ZZZMod.json                     # 模组清单
├── ZZZMod.sln                      # Visual Studio 解决方案
├── project.godot                   # Godot 项目配置
├── export_presets.cfg              # 导出配置
├── add_card_guide.md               # 添加卡牌指南
├── add_power_guide.md              # 添加能力指南
└── README.md                       # 本文件
```

---

## 🚀 添加新内容

### 添加卡牌

参考 [add_card_guide.md](add_card_guide.md) 获取详细流程。

**快速开始：**

```csharp
// Code/Cards/Common/YourCard.cs
[RegisterCard(typeof(ZZZCardPool))]
public class YourCard() : ZZZBaseCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
{
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

### 添加能力

参考 [add_power_guide.md](add_power_guide.md) 获取详细流程。

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

- [添加卡牌指南](add_card_guide.md)
- [添加能力指南](add_power_guide.md)
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

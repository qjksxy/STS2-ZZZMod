# 失衡系统文档

> **设计参考文档**：实现以 `docs/Cards.csv` 与代码为准，本文仅描述当前失衡系统的机制与结构。

## 概述

失衡（Daze）是法厄同角色的专属战斗机制。**正向计数**：玩家通过攻击牌累积敌人的失衡值，累积满后怪物进入失衡状态（受伤 +50%），持续一回合后恢复。

## 机制详解

### 失衡条

- 失衡条上限 = `max(4, 怪物最大生命值 / 12)`
- **初始为 0（空）**，随攻击累积上涨，仅对怪物生效，玩家不显示失衡条
- 失衡条打满时奖励喧响（失衡上限的一半，见 `DazeHitListener`）
- 战斗结束自动清空

### 流程

```
回合 T   玩家攻击 → 失衡值 +N（默认 +1，卡牌可自定义）
          失衡值累积满 → PendingDaze = true

回合 T+1 怪物回合开始 → 进入失衡状态
          IsDazed = true，受伤 +50%，失衡条变紫色

回合 T+2 怪物回合开始 → 恢复
          IsDazed = false，失衡条归零重新累积
```

### 伤害修正

失衡状态的 +50% 伤害通过 `DazeDamageModifier`（SingletonModel）实现，直接参与 `Hook.ModifyDamage` 计算。不经过 Power 系统，因此不受人工制品（抵消负面效果）影响。

- 基础倍率：失衡状态受伤 **×1.5**
- 有「失衡易伤」（`DazeVulnerablePower`，由醉花月云转施加）时提升为 **×2.0**；该 Power 是标准 Power，可被人工作品抵消

### 自定义失衡值

卡牌可通过实现 `IDazeCardSource` 接口自定义每次命中的失衡值：

```csharp
// ZZZBaseCard 已实现此接口，默认 DazeAmount = 1
public virtual int DazeAmount => 1;

// 子类重写
public override int DazeAmount => 2;  // 每次攻击失衡 +2
```

> 设 `DazeAmount => 0` 可关闭自动累积，改为在 `OnPlay` 中手动 `DazeStore.Get(target).AddDaze(数值)`（如醉花月云转、八声甘州）。

## 文件结构

```
Code/Daze/
├── DazeState.cs              失衡状态数据模型 + DazeStore 全局存储
├── DazeSystem.cs             核心入口：生命周期 + DazedCreatures HashSet
├── DazeHitListener.cs        攻击命中钩子：累积失衡值（含打满奖励喧响）
├── DazeDamageModifier.cs     伤害修正：失衡目标受伤 +50%（有失衡易伤 +100%）
├── DazeBarPatch.cs           失衡条 UI：血条下方绘制进度条
├── DazePower.cs              纯视觉标记（图标显示）
└── DazeVulnerablePower.cs    失衡易伤（可被人工作品抵消的额外易伤）
```

## 核心 API

| 类/方法 | 用途 |
|---------|------|
| `DazeStore.Get(creature)` | 获取/初始化怪物的失衡状态 |
| `DazeStore.TryGet(creature, out state)` | 尝试获取（不自动初始化） |
| `DazeStore.ClearAll()` | 清空所有失衡数据 |
| `DazeSystem.IsDazed(creature)` | 检查怪物是否处于失衡状态 |
| `DazeSystem.CalcMaxDaze(creature)` | 计算失衡条上限 |
| `DazeState.AddDaze(amount)` | 累积失衡值，返回是否刚好达到上限 |
| `DazeState.TickTurnStart()` | 推进状态机，返回 `DazeTurnAction` |

## 失衡条 UI

参考 firefly_mod 的 `NToughness_Patch` 实现：

- 补丁 `NHealthBar.RefreshValues`，在 `_hpForegroundContainer` 下添加子节点
- 使用 `Panel` + `StyleBoxFlat`（圆角矩形）
- 颜色状态：黄色（累积中）→ 红色（即将满）→ 紫色（失衡中）
- 显示数值文字（如 `3/6`）

## 卡牌设计（参考）

> 以下仅为设计参考，实现以 `docs/Cards.csv` 为准。

### 已实现

| 名称 | 稀有度 | 费用 | 效果 | DazeAmount |
|------|--------|------|------|------------|
| 时之锋 | Common | 1 | 8伤，失衡+2 | 2 |
| 时间障壁 | Common | 1 | 7格挡，受击反制失衡+1 | — |
| 时序连击 | Uncommon | 1 | 5伤，失衡+3 | 3 |
| 悖论冲击 | Uncommon | 2 | 12伤，失衡目标翻倍 | 1 |
| 醉花月云转 | Uncommon | 1 | 6伤，失衡+3，归零时施加失衡易伤 | 0（手动） |
| 八声甘州 | Uncommon | 2 | 全体4伤，失衡+3 | 0（手动） |
| 时光碎裂 | Rare | 2 | 直接施加999失衡 | — |

### 待实现设计

| 名称 | 稀有度 | 费用 | 效果 |
|------|--------|------|------|
| 裂隙穿刺 | Common | 1 | 6伤，失衡+1 |
| 时间回溯 | Common | 1 | 抽1牌，本回合攻击额外失衡+1 |
| 时滞力场 | Uncommon | 2 | Power，每回合自动全体失衡+1 |
| 碎时重击 | Rare | 2 | 16伤，失衡目标24伤 |
| 时之共鸣 | Rare | 1 | Power，攻击失衡时额外+1 |

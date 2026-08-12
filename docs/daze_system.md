# 失衡系统文档

## 概述

失衡是法厄同角色的专属战斗机制。玩家通过攻击怪物减少其失衡值，归零后怪物进入失衡状态（受伤 +50%），持续一回合后恢复。

## 机制详解

### 失衡条

- 失衡条上限 = `max(4, 怪物最大生命值 / 12)`
- 初始为满值，仅对怪物生效，玩家不显示失衡条
- 战斗结束自动清空

### 倒计时流程

```
回合 T   玩家攻击 → 失衡值 -N（默认 -1）
          失衡值归 0 → PendingDaze = true

回合 T+1 怪物回合开始 → 进入失衡状态
          IsDazed = true，受伤 +50%
          失衡条变紫色

回合 T+2 怪物回合开始 → 恢复
          IsDazed = false，失衡条重置为满值
```

### 伤害修正

失衡状态的 +50% 伤害通过 `DazeDamageModifier`（SingletonModel）实现，直接参与 `Hook.ModifyDamage` 计算。不经过 Power 系统，因此不受人工制品（抵消负面效果）影响。

### 自定义失衡值

卡牌可通过实现 `IDazeCardSource` 接口自定义每次命中的失衡值：

```csharp
// ZZZBaseCard 已实现此接口，默认 DazeAmount = 1
public virtual int DazeAmount => 1;

// 子类重写
public override int DazeAmount => 2;  // 每次攻击失衡 -2
```

## 文件结构

```
Code/Daze/
├── DazeState.cs              失衡状态数据模型 + DazeStore 全局存储
├── DazeSystem.cs             核心入口：生命周期 + DazedCreatures HashSet
├── DazeHitListener.cs        攻击命中钩子：减少失衡值
├── DazeDamageModifier.cs     伤害修正：失衡目标受伤 +50%
├── DazeBarPatch.cs           失衡条 UI：血条下方绘制进度条
├── DazePower.cs              纯视觉标记（图标显示）
```

## 核心 API

| 类/方法 | 用途 |
|---------|------|
| `DazeStore.Get(creature)` | 获取/初始化怪物的失衡状态 |
| `DazeStore.TryGet(creature, out state)` | 尝试获取（不自动初始化） |
| `DazeStore.ClearAll()` | 清空所有失衡数据 |
| `DazeSystem.IsDazed(creature)` | 检查怪物是否处于失衡状态 |
| `DazeSystem.CalcMaxDaze(creature)` | 计算失衡条上限 |
| `DazeState.ReduceDaze(amount)` | 减少失衡值 |
| `DazeState.TickTurnStart()` | 推进状态机，返回 `DazeTurnAction` |

## 失衡条 UI

参考 firefly_mod 的 `NToughness_Patch` 实现：

- 补丁 `NHealthBar.RefreshValues`，在 `_hpForegroundContainer` 下添加子节点
- 使用 `Panel` + `StyleBoxFlat`（圆角矩形）
- 颜色状态：黄色（倒计时）→ 红色（归零）→ 紫色（失衡中）
- 显示数值文字（如 `3/6`）

## 卡牌设计

### 已实现

| 名称 | 稀有度 | 费用 | 效果 | DazeAmount |
|------|--------|------|------|------------|
| 时之锋 | Common | 1 | 8伤，失衡-2 | 2 |
| 时间障壁 | Common | 1 | 7格挡，受击反制失衡-1 | — |
| 时序连击 | Uncommon | 1 | 5伤，失衡-3 | 3 |
| 悖论冲击 | Uncommon | 2 | 12伤，失衡目标翻倍 | 1 |
| 时光碎裂 | Rare | 2 | 直接归零触发失衡 | — |

### 设计稿

| 名称 | 稀有度 | 费用 | 效果 |
|------|--------|------|------|
| 裂隙穿刺 | Common | 1 | 6伤，失衡-1 |
| 时间回溯 | Common | 1 | 抽1牌，本回合攻击额外失衡-1 |
| 时滞力场 | Uncommon | 2 | Power，每回合自动全体失衡-1 |
| 碎时重击 | Rare | 2 | 16伤，失衡目标24伤 |
| 时之共鸣 | Rare | 1 | Power，攻击失衡时额外-1 |

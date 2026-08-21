# zzz-mod 开发文档

> `.gdignore`：docs/ 已被 Godot 排除在资源扫描外（否则 `Cards.csv` 会被当作翻译资源自动导入，生成 `Cards.*.translation` 噪音文件）。

## 📋 卡牌流水线

| 文件 | 说明 |
|------|------|
| `Cards.csv` | **★ 卡牌设计表（唯一实现依据）**，新卡先填这里 |
| `csv_to_cards.py` | 从 CSV 生成 `cards_generated.json` 本地化 |
| `implemented_cards.md` | **已实现卡牌清单**，更新 Cards.csv 后对比此文件快速定位变更 |

## 📖 添加内容指南

| 指南 | 说明 |
|------|------|
| [add_card_guide.md](add_card_guide.md) | 添加卡牌（含 Cards.csv 流水线、失衡/喧响机制、关键词、手牌高亮） |
| [add_power_guide.md](add_power_guide.md) | 添加能力（含钩子、自定义数据存储模式） |
| [add_relic_guide.md](add_relic_guide.md) | 添加遗物 |
| [add_event_guide.md](add_event_guide.md) | 添加事件 |
| [add_potion_guide.md](add_potion_guide.md) | 添加药水 |

## ⚙️ 系统设计（参考）

| 文档 | 说明 |
|------|------|
| [daze_system.md](daze_system.md) | 失衡系统机制与结构 |
| [decibel_cards_design.md](decibel_cards_design.md) | 喧响卡牌设计稿 |
| [chain_system.md](chain_system.md) | 连携系统机制与结构 |

> **约定**：设计稿仅为前期参考，不作为实现方案；实现一律以 `Cards.csv` 为准。
> `Cards.csv` 的「对应角色」列仅供卡图绘制参考，开发时忽略；玩家角色始终是法厄同（Phaethon）。

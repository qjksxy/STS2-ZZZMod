# 快速参考（selfhelp）

## 构建

```bash
dotnet build          # 编译验证（开发迭代，只看编译是否通过）
./autobuild.bat       # 部署到游戏（dotnet build -t:ExportPck）
```

> `dotnet build` 的 PostBuild 复制步骤在游戏运行时可能报 `Access denied`，不影响编译结果，验证时忽略。

## 卡牌设计流水线

1. 在 `docs/Cards.csv` 填写卡牌行（**唯一实现依据**）
2. `python docs/csv_to_cards.py` 生成 `ZZZMod/localization/zhs/cards_generated.json`
3. 实现卡牌类（`Code/Cards/{稀有度}/`）+ 将本地化并入 `cards.json`

## 游戏内调试（按 `~` 打开控制台）

| 命令 | 说明 |
|------|------|
| `card ZZZ_MOD_CARD_XXX` | 获得指定卡牌 |
| `power ZZZ_MOD_POWER_XXX 1 0` | 施加指定能力 |
| `gold 999` / `hp 999` | 增加金币 / 生命值 |

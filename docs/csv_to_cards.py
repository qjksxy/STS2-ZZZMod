"""
CSV 转 cards.json 本地化文件生成器

用法: python csv_to_cards.py [csv路径] [json输出路径]

默认:
  csv路径:  docs/Cards.csv
  json输出: ZZZMod/localization/zhs/cards_generated.json
"""

import csv
import json
import re
import sys
from pathlib import Path


def id_to_snake(name: str) -> str:
    """将 Title Case 英文 ID 转为 UPPER_SNAKE_CASE。

    示例:
        "Enchanted Moonlit Blossoms" -> "ENCHANTED_MOONLIT_BLOSSOMS"
        "Eight Sounds of Ganzhou"    -> "EIGHT_SOUNDS_OF_GANZHOU"
    """
    # 去除首尾空格，按空格拆分，转大写，用下划线连接
    words = name.strip().split()
    return "_".join(w.upper() for w in words if w)


def main():
    # 路径参数
    script_dir = Path(__file__).parent
    csv_path = Path(sys.argv[1]) if len(sys.argv) > 1 else script_dir / "Cards.csv"
    json_path = Path(sys.argv[2]) if len(sys.argv) > 2 else script_dir.parent / "ZZZMod" / "localization" / "zhs" / "cards_generated.json"

    if not csv_path.exists():
        print(f"错误: CSV 文件不存在: {csv_path}")
        sys.exit(1)

    # 读取 CSV
    entries: dict[str, str] = {}
    with open(csv_path, "r", encoding="utf-8-sig") as f:
        reader = csv.DictReader(f)
        for row in reader:
            card_id = row.get("ID", "").strip()
            if not card_id:
                continue

            snake = id_to_snake(card_id)
            title = row.get("名称", "").strip()
            description = row.get("卡牌效果", "").strip()

            if not title and not description:
                continue

            key_prefix = f"ZZZ_MOD_CARD_{snake}"
            if title:
                entries[f"{key_prefix}.title"] = title
            if description:
                entries[f"{key_prefix}.description"] = description

    if not entries:
        print("警告: 未找到有效卡牌数据")
        sys.exit(0)

    # 写入 JSON
    json_path.parent.mkdir(parents=True, exist_ok=True)
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(entries, f, ensure_ascii=False, indent=4)

    print(f"已生成 {len(entries)} 条本地化条目 -> {json_path}")
    for key, value in entries.items():
        print(f"  {key}: {value}")


if __name__ == "__main__":
    main()

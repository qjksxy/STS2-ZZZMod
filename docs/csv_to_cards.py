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
    """将 ID 转为本地化 key 的 UPPER_SNAKE_CASE。

    与实现约定和游戏实际查找规则完全一致：
    1) 按空格拆词 → 去掉每个词内的非字母数字符号（如 "Bird's" -> "Birds"、
       "V-Form" -> "VForm"）→ 词首字母大写 → 拼接 → 得到类名形式
       （如 "Eight Sounds of Ganzhou" -> "EightSoundsOfGanzhou"）；
    2) 再按 RitsuLib `ModContentRegistry.NormalizePublicStem` 规范化：
       拆分缩写词/驼峰边界、合并连续下划线、转大写。
       生成的 key 即游戏 `cards` 表实际查找的 `Id.Entry`。

    示例:
        "Enchanted Moonlit Blossoms"      -> "ENCHANTED_MOONLIT_BLOSSOMS"
        "Eight Sounds of Ganzhou"         -> "EIGHT_SOUNDS_OF_GANZHOU"
        "Soul Hunter - Punishment"        -> "SOUL_HUNTER_PUNISHMENT"
        "L-Form Thundering Fist"          -> "L_FORM_THUNDERING_FIST"
        "Soaring Bird's Song"             -> "SOARING_BIRDS_SONG"
    """
    # 1) 按空白拆词 → 词内去符号 → 词首字母大写 → 拼接（即类名形式）
    words = []
    for token in name.split():
        cleaned = re.sub(r"[^A-Za-z0-9]", "", token)
        if cleaned:
            words.append(cleaned[:1].upper() + cleaned[1:])
    class_name = "".join(words)
    # 2) RitsuLib NormalizePublicStem 规范化
    normalized = re.sub(r"([A-Z]+)([A-Z][a-z])", r"\1_\2", class_name)   # 缩写词边界 (ZZZStrike → ZZZ_Strike)
    normalized = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", normalized)       # 驼峰边界 (SoaringBirdsSong → Soaring_Birds_Song)
    normalized = re.sub(r"_+", "_", normalized)                           # 合并连续下划线
    return normalized.strip("_").upper()


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

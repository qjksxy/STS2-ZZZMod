using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ZZZMod.Code.Cards;

[RegisterOwnedCardKeyword(nameof(Overflow), IconPath = "res://icon.svg")]
// [RegisterOwnedCardKeyword(nameof(Unique2), IconPath = "res://icon.svg")] // 如果要加更多关键词，添加特性
public class ZZZModKeywords
{
    /// <summary>
    ///     满盈：角色满血时触发额外效果。
    ///     在手牌中发黄光提示，打出时额外造成 4 点伤害。
    /// </summary>
    public static readonly string Overflow =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Overflow));
}
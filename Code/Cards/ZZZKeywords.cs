using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ZZZMod.Code.Cards;

[RegisterOwnedCardKeyword(nameof(Overflow), IconPath = "res://icon.svg")]
[RegisterOwnedCardKeyword(nameof(Decibel), IconPath = "res://icon.svg")]
public class ZZZModKeywords
{
    /// <summary>
    ///     满盈：角色满血时触发额外效果。
    /// </summary>
    public static readonly string Overflow =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Overflow));

    /// <summary>
    ///     Decibel：Decibel 值足够时，消耗指定点数激活额外效果。
    /// </summary>
    public static readonly string Decibel =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Decibel));
}

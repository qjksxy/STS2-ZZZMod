using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ZZZMod.Code.Character;
using ZZZMod.Code.Pools;

namespace ZZZMod.Code.Relics;

/// <summary>
///     初始遗物 —— 角色专属初始遗物。
/// </summary>
[RegisterRelic(typeof(ZZZRelicPool))]
[RegisterCharacterStarterRelic(typeof(ZZZCharacter))]
public sealed class ZZZStarterRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://icon.svg"
    );
}

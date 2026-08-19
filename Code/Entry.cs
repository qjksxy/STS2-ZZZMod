using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using ZZZMod.Code.Chain;
using ZZZMod.Code.Daze;
using ZZZMod.Code.Decibel;

namespace ZZZMod.Code;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "ZZZMod";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        var patcher = RitsuLibFramework.CreatePatcher(ModId, "combat");
        patcher.RegisterPatch<DazeBarPatch>();
        patcher.RegisterPatch<DecibelBarPatch>();
        patcher.PatchAll();

        DazeSystem.Init();
        DecibelSystem.Init();
        ChainSystem.Init();
    }
}

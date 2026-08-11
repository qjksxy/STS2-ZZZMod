using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using ZZZMod.Code.Daze;

namespace ZZZMod.Code;

[ModInitializer(nameof(Init))]
public class Entry
{
    // 你的modid
    public const string ModId = "ZZZMod";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        // 自动注册内容
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        // 注册失衡系统 Harmony 补丁
        var patcher = RitsuLibFramework.CreatePatcher(ModId, "daze");
        patcher.RegisterPatch<DazeBarPatch>();
        patcher.PatchAll();

        // 注册失衡系统生命周期
        DazeSystem.Init();
    }
}
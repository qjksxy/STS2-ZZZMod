using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡条 UI：参考 firefly_mod 的 NToughness_Patch 实现。
///     补丁 NHealthBar.RefreshValues，在 _hpForegroundContainer 下添加失衡条子节点。
///     填充通过 scale:x 缩放实现，位置使用本地坐标。
///     使用 Panel + StyleBoxFlat 绘制（无纹理依赖）。
/// </summary>
[HarmonyPriority(Priority.Last)]
internal sealed class DazeBarPatch : IPatchMethod
{
    public static string PatchId => "zzz_daze_bar";
    public static string Description => "Daze bar on monster health bars";
    public static bool IsCritical => false;

    private const float BarHeight = 8f;
    private const float BarVerticalOffset = 32f;
    private const float TextVerticalOffset = 28f;

    private static readonly Color DazeFillColor = new("FFD700");
    private static readonly Color DazeFullColor = new("FF4444");
    private static readonly Color DazeBgColor = new("3A3A18");

    private static readonly FieldInfo CreatureField =
        AccessTools.Field(typeof(NHealthBar), "_creature");

    private static readonly FieldInfo FgContainerField =
        AccessTools.Field(typeof(NHealthBar), "_hpForegroundContainer");

    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(NHealthBar), "RefreshValues")];

    public static void Postfix(NHealthBar __instance)
    {
        var creature = (Creature?)CreatureField.GetValue(__instance);
        var container = (Control?)FgContainerField.GetValue(__instance);

        if (creature == null || container == null)
        {
            HideAll(__instance);
            return;
        }

        if (!creature.IsAlive || creature.Side != CombatSide.Enemy)
        {
            HideAll(__instance);
            return;
        }

        // 获取失衡数据（Get 会自动初始化为满值）
        var daze = DazeStore.Get(creature);
        var current = daze.CurrentValue;
        var max = daze.MaxValue;

        var rect = container.GetRect();
        var barWidth = rect.Size.X;
        var barY = rect.Size.Y - BarVerticalOffset;
        var ratio = Mathf.Clamp((float)current / max, 0f, 1f);

        var bg = GetOrCreateNode<Panel>(container, "DazeBg", CreateBg);
        var fill = GetOrCreateNode<Panel>(container, "DazeFill", CreateFill);
        var label = GetOrCreateNode<Label>(container, "DazeLabel", CreateLabel);

        // 背景条
        bg.SetPosition(new Vector2(0f, barY), false);
        bg.SetSize(new Vector2(barWidth, BarHeight), false);
        bg.Visible = true;

        // 颜色：每次创建新 StyleBox 并覆盖（GetThemeStylebox 返回副本，不能直接改）
        Color fillColor;
        float fillWidth;
        if (daze.IsDazed)
        {
            fillColor = new Color("9933FF"); // 紫色：正在失衡
            fillWidth = barWidth;            // 满条紫色，明确标识失衡状态
        }
        else if (daze.IsEmpty)
        {
            fillColor = DazeFullColor;       // 红色：即将失衡
            fillWidth = 0;                   // 空条，等待触发
        }
        else
        {
            fillColor = DazeFillColor;       // 黄色：正常倒计时
            fillWidth = barWidth * ratio;
        }

        // 填充条：直接调整宽度（保持圆角不变形）
        fill.SetPosition(new Vector2(0f, barY), false);
        fill.SetSize(new Vector2(fillWidth, BarHeight), false);
        fill.Visible = true;

        fill.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = fillColor,
            CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
        });

        // 文字标签
        label.SetPosition(new Vector2(0f, barY - TextVerticalOffset), false);
        label.SetSize(new Vector2(barWidth, 40f), false);
        label.Visible = true;
        label.Text = $"{current}/{max}";
    }

    private static Panel CreateBg()
    {
        var panel = new Panel { Name = "DazeBg" };
        var style = new StyleBoxFlat
        {
            BgColor = DazeBgColor,
            CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
        };
        panel.AddThemeStyleboxOverride("panel", style);
        return panel;
    }

    private static Panel CreateFill()
    {
        var panel = new Panel
        {
            Name = "DazeFill",
            ClipContents = true,
        };
        var style = new StyleBoxFlat
        {
            BgColor = DazeFillColor,
            CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
        };
        panel.AddThemeStyleboxOverride("panel", style);
        return panel;
    }

    private static Label CreateLabel()
    {
        var label = new Label
        {
            Name = "DazeLabel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        label.AddThemeColorOverride("font_color", new Color("FFFFAA"));
        label.AddThemeColorOverride("font_outline_color", new Color("1A1A00"));
        label.AddThemeConstantOverride("outline_size", 4);
        label.AddThemeFontSizeOverride("font_size", 20);
        return label;
    }

    private static T GetOrCreateNode<T>(Control parent, string name, Func<T> factory) where T : Node
    {
        var node = parent.GetNodeOrNull<T>(name);
        if (node == null)
        {
            node = factory();
            parent.AddChild(node);
        }
        return node;
    }

    private static void HideAll(NHealthBar bar)
    {
        var container = (Control?)FgContainerField.GetValue(bar);
        if (container == null) return;

        var bg = container.GetNodeOrNull<Panel>("DazeBg");
        var fill = container.GetNodeOrNull<Panel>("DazeFill");
        var label = container.GetNodeOrNull<Label>("DazeLabel");

        if (bg != null) bg.Visible = false;
        if (fill != null) fill.Visible = false;
        if (label != null) label.Visible = false;
    }
}

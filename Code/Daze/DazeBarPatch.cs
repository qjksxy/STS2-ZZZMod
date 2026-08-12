using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡条 UI：分段式正向计数。
///     补丁 NHealthBar.RefreshValues，仅在失衡值 > 0 时显示。
/// </summary>
[HarmonyPriority(Priority.Last)]
internal sealed class DazeBarPatch : IPatchMethod
{
    public static string PatchId => "zzz_daze_bar";
    public static string Description => "Daze bar on monster health bars";
    public static bool IsCritical => false;

    private const float BarHeight = 10f;
    private const float BarVerticalOffset = 34f;
    private const float SegmentGap = 2f;
    private const float CornerRadius = 3f;

    private static readonly Color BgColor = new(0, 0, 0, 0.5f);
    private static readonly Color SegActiveColor = new("FFD700");
    private static readonly Color SegFullColor = new("FF4444");
    private static readonly Color SegDazedColor = new("9933FF");
    private static readonly Color BorderColor = new("2A2A10");

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

        if (creature == null || container == null || !creature.IsAlive || creature.Side != CombatSide.Enemy)
        {
            HideAll(container);
            return;
        }

        var daze = DazeStore.Get(creature);
        var current = daze.CurrentValue;
        var max = daze.MaxValue;
        if (max <= 0) { HideAll(container); return; }

        var rect = container.GetRect();
        var totalWidth = rect.Size.X;
        var barY = rect.Size.Y - BarVerticalOffset;
        var segWidth = (totalWidth - SegmentGap * (max - 1)) / max;

        // 分段容器
        var segContainer = GetOrCreateNode<Control>(container, "DazeSegments", () => new Control
        {
            Name = "DazeSegments",
            MouseFilter = Control.MouseFilterEnum.Ignore,
        });
        segContainer.SetPosition(new Vector2(0f, barY), false);
        segContainer.Size = new Vector2(totalWidth, BarHeight);
        segContainer.Visible = true;

        // 更新分段
        for (var i = 0; i < max; i++)
        {
            var seg = GetOrCreateNode<Panel>(segContainer, $"Seg{i}", () => CreateSegment($"Seg{i}"));
            var x = i * (segWidth + SegmentGap);
            seg.SetPosition(new Vector2(x, 0), false);
            seg.SetSize(new Vector2(segWidth, BarHeight), false);
            seg.Visible = true;

            Color color;
            if (daze.IsDazed)
                color = SegDazedColor;
            else if (daze.IsFull)
                color = SegFullColor;
            else
                color = i < current ? SegActiveColor : BgColor;

            seg.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = color,
                CornerRadiusTopLeft = (int)CornerRadius,
                CornerRadiusTopRight = (int)CornerRadius,
                CornerRadiusBottomLeft = (int)CornerRadius,
                CornerRadiusBottomRight = (int)CornerRadius,
            });
        }

        // 隐藏多余分段
        for (var i = max; i < 20; i++)
        {
            var old = segContainer.GetNodeOrNull<Panel>($"Seg{i}");
            if (old != null) old.Visible = false;
        }

        // 标签
        var label = GetOrCreateNode<Label>(container, "DazeLabel", CreateLabel);
        label.SetPosition(new Vector2(0f, barY - BarHeight - 4f), false);
        label.SetSize(new Vector2(totalWidth, BarHeight + 2f), false);
        label.Visible = true;

        if (daze.IsDazed)
        {
            label.Text = "失衡中";
            label.AddThemeColorOverride("font_color", new Color("CC88FF"));
        }
        else if (current <= 0)
        {
            label.Text = $"{current}/{max}";
            label.AddThemeColorOverride("font_color", new Color("888866"));
        }
        else
        {
            label.Text = $"{current}/{max}";
            label.AddThemeColorOverride("font_color", daze.IsFull ? SegFullColor : SegActiveColor);
        }
    }

    private static Panel CreateSegment(string name)
    {
        var panel = new Panel { Name = name, MouseFilter = Control.MouseFilterEnum.Ignore };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = SegActiveColor,
            CornerRadiusTopLeft = (int)CornerRadius,
            CornerRadiusTopRight = (int)CornerRadius,
            CornerRadiusBottomLeft = (int)CornerRadius,
            CornerRadiusBottomRight = (int)CornerRadius,
        });
        return panel;
    }

    private static Label CreateLabel()
    {
        var label = new Label
        {
            Name = "DazeLabel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        label.AddThemeColorOverride("font_color", SegActiveColor);
        label.AddThemeColorOverride("font_outline_color", new Color("1A1A00"));
        label.AddThemeConstantOverride("outline_size", 3);
        label.AddThemeFontSizeOverride("font_size", 14);
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

    private static void HideAll(Control? container)
    {
        if (container == null) return;
        var segs = container.GetNodeOrNull<Control>("DazeSegments");
        var label = container.GetNodeOrNull<Label>("DazeLabel");
        if (segs != null) segs.Visible = false;
        if (label != null) label.Visible = false;
    }
}

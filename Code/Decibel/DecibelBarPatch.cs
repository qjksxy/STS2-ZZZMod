using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;

namespace ZZZMod.Code.Decibel;

/// <summary>
///     Decibel UI：在玩家血条旁显示当前值（浮动方框 + 数字）。
/// </summary>
[HarmonyPriority(Priority.Last)]
internal sealed class DecibelBarPatch : IPatchMethod
{
    public static string PatchId => "zzz_decibel_bar";
    public static string Description => "Decibel display near player health bar";
    public static bool IsCritical => false;

    private const float BoxSize = 36f;
    private const float OffsetX = 10f;
    private const float OffsetY = -22f;

    private static readonly Color BoxBgColor = new(0, 0, 0, 0.7f);
    private static readonly Color TextNormalColor = new("FFDDAA");
    private static readonly Color TextFullColor = new("FFAA00");

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

        if (creature == null || container == null || !creature.IsAlive || creature.Side != CombatSide.Player)
        {
            HideAll(container);
            return;
        }

        var value = DecibelSystem.GetValue();

        var rect = container.GetRect();
        var boxX = rect.Size.X + OffsetX;
        var boxY = rect.Size.Y + OffsetY;

        var box = GetOrCreateNode<Panel>(container, "DecibelBox", CreateBox);
        box.SetPosition(new Vector2(boxX, boxY), false);
        box.SetSize(new Vector2(BoxSize, BoxSize), false);
        box.Visible = true;

        var label = GetOrCreateNode<Label>(container, "DecibelLabel", CreateLabel);
        label.SetPosition(new Vector2(boxX, boxY), false);
        label.SetSize(new Vector2(BoxSize, BoxSize), false);
        label.Text = $"{value}";
        label.AddThemeColorOverride("font_color", value >= DecibelData.MaxValue ? TextFullColor : TextNormalColor);
        label.Visible = true;
    }

    private static Panel CreateBox()
    {
        var panel = new Panel { Name = "DecibelBox", MouseFilter = Control.MouseFilterEnum.Ignore };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = BoxBgColor,
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
            BorderColor = new Color("555530"),
            BorderWidthTop = 1, BorderWidthBottom = 1,
            BorderWidthLeft = 1, BorderWidthRight = 1,
        });
        return panel;
    }

    private static Label CreateLabel()
    {
        var label = new Label
        {
            Name = "DecibelLabel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        label.AddThemeColorOverride("font_color", TextNormalColor);
        label.AddThemeColorOverride("font_outline_color", new Color("1A0800"));
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

    private static void HideAll(Control? container)
    {
        if (container == null) return;
        var box = container.GetNodeOrNull<Panel>("DecibelBox");
        var label = container.GetNodeOrNull<Label>("DecibelLabel");
        if (box != null) box.Visible = false;
        if (label != null) label.Visible = false;
    }
}

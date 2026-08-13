using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Utils;

namespace ZZZMod.Code.Daze;

/// <summary>
///     失衡条 UI：分段式正向计数 + 动画效果。
///     - 分段点亮时从白色展开
///     - 进入失衡状态时分段逐渐变为紫色
///     - 始终显示边框和文字，即使失衡值为 0
/// </summary>
[HarmonyPriority(Priority.Last)]
internal sealed class DazeBarPatch : IPatchMethod
{
    public static string PatchId => "zzz_daze_bar";
    public static string Description => "Daze bar on monster health bars";
    public static bool IsCritical => false;

    private const float BarHeight = 10f;
    private const float BarVerticalOffset = 36f;
    private const float SegmentGap = 2f;
    private const float CornerRadius = 3f;
    private const float BorderPad = 2f;
    private const float FlashDuration = 0.35f;
    private const float GrowDuration = 0.3f;
    private const float DazeTransitionDuration = 0.6f;
    private const float FlashScaleUp = 1.3f;

    private static readonly Color BgColor = new(0, 0, 0, 0.5f);
    private static readonly Color BorderColor = new("555530");
    private static readonly Color SegActiveColor = new("FFD700");
    private static readonly Color SegFullColor = new("FF4444");
    private static readonly Color SegDazedColor = new("9933FF");

    private static readonly FieldInfo CreatureField =
        AccessTools.Field(typeof(NHealthBar), "_creature");
    private static readonly FieldInfo FgContainerField =
        AccessTools.Field(typeof(NHealthBar), "_hpForegroundContainer");

    private static readonly AttachedState<NHealthBar, int> PrevDazeValues = new(() => -1);
    private static readonly AttachedState<NHealthBar, bool> PrevIsDazed = new(() => false);

    private static readonly StringName MetaFlashTween = "DazeFlashTween";
    private static readonly StringName MetaGrowTween = "DazeGrowTween";
    private static readonly StringName MetaDazeTween = "DazeTransitionTween";

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

        var rect = container.GetRect();
        var totalWidth = rect.Size.X;
        var barY = rect.Size.Y - BarVerticalOffset;
        var segWidth = max > 0 ? (totalWidth - SegmentGap * (max - 1)) / max : 0;

        // 检测变化
        var prev = PrevDazeValues[__instance];
        var changed = prev != current;
        PrevDazeValues[__instance] = current;

        var wasDazed = PrevIsDazed[__instance];
        var justEnteredDazed = daze.IsDazed && !wasDazed;
        PrevIsDazed[__instance] = daze.IsDazed;

        // ── 边框（始终显示）──
        var border = GetOrCreateNode<Panel>(container, "DazeBorder", CreateBorder);
        border.SetPosition(new Vector2(-BorderPad, barY - BorderPad), false);
        border.SetSize(new Vector2(totalWidth + BorderPad * 2, BarHeight + BorderPad * 2), false);
        border.Visible = true;

        // ── 分段容器 ──
        if (max <= 0) { HideSegments(container); ShowLabel(container, totalWidth, barY, daze); return; }

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
            seg.Visible = true;

            Color color;
            if (daze.IsDazed)
                color = SegDazedColor;
            else if (daze.IsFull)
                color = SegFullColor;
            else
                color = i < current ? SegActiveColor : BgColor;

            // 新点亮的分段：从宽度 0 逐渐展开
            var isNewlyLit = changed && i == current - 1 && current > 0 && !daze.IsDazed;
            if (isNewlyLit)
            {
                seg.SetSize(new Vector2(0, BarHeight), false);
                StartGrow(seg, segWidth, BarHeight, color);
            }
            else if (!seg.HasMeta(MetaGrowTween) || !seg.GetMeta(MetaGrowTween).As<Tween>().IsValid())
            {
                seg.SetSize(new Vector2(segWidth, BarHeight), false);
            }

            // 刚进入失衡状态：所有分段逐渐变为紫色
            if (justEnteredDazed && i < max)
            {
                StartDazeTransition(seg, color);
            }
            // 非动画中：更新颜色
            else if (!seg.HasMeta(MetaDazeTween) || !seg.GetMeta(MetaDazeTween).As<Tween>().IsValid())
            {
                var currentStyle = seg.GetThemeStylebox("panel") as StyleBoxFlat;
                if (currentStyle == null || currentStyle.BgColor != color)
                {
                    seg.AddThemeStyleboxOverride("panel", new StyleBoxFlat
                    {
                        BgColor = color,
                        CornerRadiusTopLeft = (int)CornerRadius,
                        CornerRadiusTopRight = (int)CornerRadius,
                        CornerRadiusBottomLeft = (int)CornerRadius,
                        CornerRadiusBottomRight = (int)CornerRadius,
                    });
                }
            }
        }

        // 隐藏多余分段
        for (var i = max; i < 20; i++)
        {
            var old = segContainer.GetNodeOrNull<Panel>($"Seg{i}");
            if (old != null) old.Visible = false;
        }

        // ── 标签 ──
        ShowLabel(container, totalWidth, barY, daze);
    }

    private static void ShowLabel(Control container, float totalWidth, float barY, DazeState daze)
    {
        var label = GetOrCreateNode<Label>(container, "DazeLabel", CreateLabel);
        label.SetPosition(new Vector2(0f, barY - BarHeight - 6f), false);
        label.SetSize(new Vector2(totalWidth, BarHeight + 4f), false);
        label.Visible = true;

        if (daze.IsDazed)
        {
            label.Text = "失衡中";
            label.AddThemeColorOverride("font_color", new Color("CC88FF"));
        }
        else if (daze.CurrentValue <= 0)
        {
            label.Text = $"{daze.CurrentValue}/{daze.MaxValue}";
            label.AddThemeColorOverride("font_color", new Color("888866"));
        }
        else
        {
            label.Text = $"{daze.CurrentValue}/{daze.MaxValue}";
            label.AddThemeColorOverride("font_color", daze.IsFull ? SegFullColor : SegActiveColor);
        }
    }

    private static void HideSegments(Control container)
    {
        var segs = container.GetNodeOrNull<Control>("DazeSegments");
        if (segs != null) segs.Visible = false;
    }

    // ── 动画 ──

    private static void StartGrow(Panel seg, float targetWidth, float targetHeight, Color targetColor)
    {
        if (seg.HasMeta(MetaGrowTween))
        {
            var existing = seg.GetMeta(MetaGrowTween).As<Tween>();
            if (existing != null && existing.IsValid()) return;
        }

        seg.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("FFFFFF"),
            CornerRadiusTopLeft = (int)CornerRadius,
            CornerRadiusTopRight = (int)CornerRadius,
            CornerRadiusBottomLeft = (int)CornerRadius,
            CornerRadiusBottomRight = (int)CornerRadius,
        });
        seg.PivotOffset = new Vector2(0, targetHeight * 0.5f);
        seg.Scale = new Vector2(1f, FlashScaleUp);

        var tween = seg.CreateTween().SetParallel(true);

        tween.TweenProperty(seg, "size", new Vector2(targetWidth, targetHeight), GrowDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);

        tween.TweenMethod(Callable.From<Color>(c =>
        {
            seg.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = c,
                CornerRadiusTopLeft = (int)CornerRadius,
                CornerRadiusTopRight = (int)CornerRadius,
                CornerRadiusBottomLeft = (int)CornerRadius,
                CornerRadiusBottomRight = (int)CornerRadius,
            });
        }), new Color("FFFFFF"), targetColor, FlashDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);

        tween.TweenProperty(seg, "scale:y", 1f, FlashDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Elastic);

        tween.Chain().Finished += () =>
        {
            seg.Scale = Vector2.One;
            seg.PivotOffset = Vector2.Zero;
            if (seg.HasMeta(MetaGrowTween)) seg.RemoveMeta(MetaGrowTween);
        };
        seg.SetMeta(MetaGrowTween, tween);
    }

    private static void StartDazeTransition(Panel seg, Color targetColor)
    {
        if (seg.HasMeta(MetaDazeTween))
        {
            var existing = seg.GetMeta(MetaDazeTween).As<Tween>();
            if (existing != null && existing.IsValid()) return;
        }

        var currentStyle = seg.GetThemeStylebox("panel") as StyleBoxFlat;
        var startColor = currentStyle?.BgColor ?? SegActiveColor;

        var tween = seg.CreateTween();
        tween.TweenMethod(Callable.From<Color>(c =>
        {
            seg.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = c,
                CornerRadiusTopLeft = (int)CornerRadius,
                CornerRadiusTopRight = (int)CornerRadius,
                CornerRadiusBottomLeft = (int)CornerRadius,
                CornerRadiusBottomRight = (int)CornerRadius,
            });
        }), startColor, targetColor, DazeTransitionDuration)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine);

        tween.Finished += () =>
        {
            if (seg.HasMeta(MetaDazeTween)) seg.RemoveMeta(MetaDazeTween);
        };
        seg.SetMeta(MetaDazeTween, tween);
    }

    // ── 节点工厂 ──

    private static Panel CreateSegment(string name)
    {
        var panel = new Panel { Name = name, MouseFilter = Control.MouseFilterEnum.Ignore };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = BgColor,
            CornerRadiusTopLeft = (int)CornerRadius,
            CornerRadiusTopRight = (int)CornerRadius,
            CornerRadiusBottomLeft = (int)CornerRadius,
            CornerRadiusBottomRight = (int)CornerRadius,
        });
        return panel;
    }

    private static Panel CreateBorder()
    {
        var panel = new Panel { Name = "DazeBorder", MouseFilter = Control.MouseFilterEnum.Ignore };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = BorderColor,
            CornerRadiusTopLeft = (int)(CornerRadius + BorderPad),
            CornerRadiusTopRight = (int)(CornerRadius + BorderPad),
            CornerRadiusBottomLeft = (int)(CornerRadius + BorderPad),
            CornerRadiusBottomRight = (int)(CornerRadius + BorderPad),
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
        label.AddThemeConstantOverride("outline_size", 4);
        label.AddThemeFontSizeOverride("font_size", 18);
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
        var border = container.GetNodeOrNull<Panel>("DazeBorder");
        var segs = container.GetNodeOrNull<Control>("DazeSegments");
        var label = container.GetNodeOrNull<Label>("DazeLabel");
        if (border != null) border.Visible = false;
        if (segs != null) segs.Visible = false;
        if (label != null) label.Visible = false;
    }
}

using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Utils;

namespace ZZZMod.Code.Decibel;

/// <summary>
///     Decibel UI：圆形外框 + 多层发光 + 粒子特效 + 呼吸动效。
/// </summary>
[HarmonyPriority(Priority.Last)]
internal sealed class DecibelBarPatch : IPatchMethod
{
    public static string PatchId => "zzz_decibel_bar";
    public static string Description => "Decibel display near player health bar";
    public static bool IsCritical => false;

    private const float BoxSize = 44f;
    private const float Glow1 = 50f;
    private const float Glow2 = 60f;
    private const float Glow3 = 72f;
    private const float OffsetX = -100f;
    private const float OffsetY = -110f;
    private const int R = 999;

    private static readonly Color FrameColor = new("8B6914");
    private static readonly Color FrameFullColor = new("FFAA00");
    private static readonly Color InnerBgColor = new(0.05f, 0.03f, 0f, 0.85f);
    private static readonly Color TextColor = new("FFDDAA");
    private static readonly Color TextFullColor = new("FFCC00");

    private static readonly FieldInfo CreatureField =
        AccessTools.Field(typeof(NHealthBar), "_creature");
    private static readonly FieldInfo FgContainerField =
        AccessTools.Field(typeof(NHealthBar), "_hpForegroundContainer");

    private static readonly AttachedState<NHealthBar, int> PrevValues = new(() => -1);
    private static readonly StringName MetaPopTween = "DBPop";
    private static readonly StringName MetaBreathTween = "DBBreath";

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
        var prev = PrevValues[__instance];
        var changed = prev != value;
        PrevValues[__instance] = value;
        var isFull = value >= DecibelData.MaxValue;

        var rect = container.GetRect();
        var cx = OffsetX + BoxSize * 0.5f;
        var cy = rect.Size.Y + OffsetY + BoxSize * 0.5f;
        var x = OffsetX;
        var y = rect.Size.Y + OffsetY;

        // ── 多层发光（外侧渐变模糊）──
        var gc3 = isFull ? new Color("FF6600", 0.08f) : new Color("FF4400", 0.04f);
        var gc2 = isFull ? new Color("FF8800", 0.18f) : new Color("FF6600", 0.1f);
        var gc1 = isFull ? new Color("FFAA00", 0.45f) : new Color("FF8800", 0.3f);

        var glow3 = GetOrCreateNode<Panel>(container, "DBGlow3", () => MakeCircle("DBGlow3", Glow3, gc3));
        glow3.SetPosition(new Vector2(cx - Glow3 * 0.5f, cy - Glow3 * 0.5f), false);
        glow3.SetSize(new Vector2(Glow3, Glow3), false);
        glow3.Visible = true;
        SetCircleColor(glow3, gc3);

        var glow2 = GetOrCreateNode<Panel>(container, "DBGlow2", () => MakeCircle("DBGlow2", Glow2, gc2));
        glow2.SetPosition(new Vector2(cx - Glow2 * 0.5f, cy - Glow2 * 0.5f), false);
        glow2.SetSize(new Vector2(Glow2, Glow2), false);
        glow2.Visible = true;
        SetCircleColor(glow2, gc2);

        var glow1 = GetOrCreateNode<Panel>(container, "DBGlow1", () => MakeCircle("DBGlow1", Glow1, gc1));
        glow1.SetPosition(new Vector2(cx - Glow1 * 0.5f, cy - Glow1 * 0.5f), false);
        glow1.SetSize(new Vector2(Glow1, Glow1), false);
        glow1.Visible = true;
        SetCircleColor(glow1, gc1);

        // ── 主框 ──
        var frame = GetOrCreateNode<Panel>(container, "DBFrame", () => MakeCircleBorder("DBFrame", BoxSize, FrameColor));
        frame.SetPosition(new Vector2(x, y), false);
        frame.SetSize(new Vector2(BoxSize, BoxSize), false);
        frame.Visible = true;
        SetCircleBorderStyle(frame, isFull ? FrameFullColor : FrameColor, isFull ? 3 : 2);

        // ── 内底 ──
        var inner = GetOrCreateNode<Panel>(container, "DBInner", () => MakeCircle("DBInner", BoxSize - 6, InnerBgColor));
        inner.SetPosition(new Vector2(x + 3, y + 3), false);
        inner.SetSize(new Vector2(BoxSize - 6, BoxSize - 6), false);
        inner.Visible = true;

        // ── 呼吸动效 ──
        EnsureBreathing(container, glow1, glow2, glow3, frame, isFull);

        // ── 边框粒子（常态也显示）──
        UpdateBorderSparks(container, cx, cy, true);

        // ── 数字 ──
        var label = GetOrCreateNode<Label>(container, "DBLabel", MakeLabel);
        if (!label.HasMeta(MetaPopTween) || !label.GetMeta(MetaPopTween).As<Tween>().IsValid())
        {
            label.SetPosition(new Vector2(x - 2, y + 2), false);
            label.SetSize(new Vector2(BoxSize + 4, BoxSize - 4), false);
            label.Scale = Vector2.One;
            label.AddThemeColorOverride("font_color", isFull ? TextFullColor : TextColor);
        }
        label.Text = $"{value}";
        label.Visible = true;

        // ── 环绕粒子（常态也显示）──
        UpdateOrbitParticles(container, cx, cy, true);

        // ── 值变化反馈 ──
        if (changed && value > 0)
        {
            StartPop(label, isFull);
        }
    }

    // ── 边框火花粒子 ──

    private static void UpdateBorderSparks(Control container, float cx, float cy, bool active)
    {
        var particles = GetOrCreateNode<GpuParticles2D>(container, "DBSparks", CreateBorderSparks);
        particles.Position = new Vector2(cx, cy);
        particles.Emitting = active;
        particles.Visible = active;
    }

    private static GpuParticles2D CreateBorderSparks()
    {
        var p = new GpuParticles2D
        {
            Name = "DBSparks",
            Amount = 8,
            Lifetime = 0.8f,
            Explosiveness = 0f,
            Randomness = 1f,
            VisibilityRect = new Rect2(-50, -50, 100, 100),
        };
        var mat = new ParticleProcessMaterial
        {
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Ring,
            EmissionRingRadius = 23f,
            EmissionRingInnerRadius = 22f,
            EmissionRingHeight = 0.1f,
            Direction = new Vector3(0, -1, 0),
            Spread = 360f,
            InitialVelocityMin = 2f,
            InitialVelocityMax = 6f,
            Gravity = Vector3.Zero,
            ScaleMin = 0.5f,
            ScaleMax = 1.2f,
            Color = new Color("FFCC44"),
        };
        p.ProcessMaterial = mat;
        return p;
    }

    // ── 呼吸动效 ──

    private static void EnsureBreathing(Control container, Panel g1, Panel g2, Panel g3, Panel frame, bool isFull)
    {
        if (container.HasMeta(MetaBreathTween))
        {
            var existing = container.GetMeta(MetaBreathTween).As<Tween>();
            if (existing != null && existing.IsValid()) return;
        }

        g1.PivotOffset = g1.Size * 0.5f;
        g2.PivotOffset = g2.Size * 0.5f;
        g3.PivotOffset = g3.Size * 0.5f;

        var tween = container.CreateTween();
        tween.SetLoops();

        // 外层发光：大幅透明度呼吸 + 缩放
        tween.TweenProperty(g3, "self_modulate:a", 0.2f, 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.Parallel().TweenProperty(g3, "scale", new Vector2(1.1f, 1.1f), 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);

        // 中层
        tween.TweenProperty(g2, "self_modulate:a", 0.4f, 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.Parallel().TweenProperty(g2, "scale", new Vector2(1.08f, 1.08f), 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);

        // 内层
        tween.TweenProperty(g1, "self_modulate:a", 0.5f, 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.Parallel().TweenProperty(g1, "scale", new Vector2(1.05f, 1.05f), 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);

        // 收回
        tween.TweenProperty(g3, "self_modulate:a", 1.0f, 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.Parallel().TweenProperty(g3, "scale", Vector2.One, 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(g2, "self_modulate:a", 1.0f, 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.Parallel().TweenProperty(g2, "scale", Vector2.One, 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(g1, "self_modulate:a", 1.0f, 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.Parallel().TweenProperty(g1, "scale", Vector2.One, 1.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);

        // 边框颜色渐变流转
        var cA = isFull ? new Color("FFDD00") : new Color("6B4904");
        var cB = isFull ? new Color("FF5500") : new Color("CC9922");
        var borderWidth = isFull ? 3 : 2;

        tween.TweenMethod(Callable.From<Color>(c =>
        {
            SetCircleBorderStyle(frame, c, borderWidth);
        }), cA, cB, 2.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
        tween.TweenMethod(Callable.From<Color>(c =>
        {
            SetCircleBorderStyle(frame, c, borderWidth);
        }), cB, cA, 2.0f)
            .SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);

        container.SetMeta(MetaBreathTween, tween);
    }

    // ── 弹跳 ──

    private static void StartPop(Label label, bool isFull)
    {
        if (label.HasMeta(MetaPopTween))
        {
            var existing = label.GetMeta(MetaPopTween).As<Tween>();
            if (existing != null && existing.IsValid()) existing.Kill();
        }

        label.PivotOffset = label.Size * 0.5f;
        label.Scale = new Vector2(1.2f, 1.2f);

        var tween = label.CreateTween();
        tween.TweenProperty(label, "scale", Vector2.One, 0.3f)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        tween.Chain().Finished += () =>
        {
            label.Scale = Vector2.One;
            label.PivotOffset = Vector2.Zero;
            label.AddThemeColorOverride("font_color", isFull ? TextFullColor : TextColor);
            label.RemoveMeta(MetaPopTween);
        };
        label.SetMeta(MetaPopTween, tween);
    }

    // ── 粒子 ──

    private static void UpdateOrbitParticles(Control container, float cx, float cy, bool active)
    {
        var p = GetOrCreateNode<GpuParticles2D>(container, "DBOrbit", CreateOrbitParticles);
        p.Position = new Vector2(cx, cy);
        p.Emitting = active;
        p.Visible = active;
    }

    private static GpuParticles2D CreateOrbitParticles()
    {
        var p = new GpuParticles2D
        {
            Name = "DBOrbit",
            Amount = 12,
            Lifetime = 1.5f,
            Explosiveness = 0f,
            Randomness = 0.5f,
            VisibilityRect = new Rect2(-40, -40, 80, 80),
        };
        var mat = new ParticleProcessMaterial
        {
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Ring,
            EmissionRingRadius = 24f,
            EmissionRingInnerRadius = 22f,
            EmissionRingHeight = 0.1f,
            Direction = new Vector3(0, -1, 0),
            Spread = 360f,
            InitialVelocityMin = 8f,
            InitialVelocityMax = 15f,
            Gravity = Vector3.Zero,
            ScaleMin = 0.8f,
            ScaleMax = 1.5f,
            Color = new Color("FFAA00"),
        };
        p.ProcessMaterial = mat;
        return p;
    }

    // ── 节点工厂 ──

    private static Panel MakeCircle(string name, float size, Color color)
    {
        var r = (int)(size * 0.5f);
        var p = new Panel { Name = name, MouseFilter = Control.MouseFilterEnum.Ignore };
        p.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = color,
            CornerRadiusTopLeft = r, CornerRadiusTopRight = r,
            CornerRadiusBottomLeft = r, CornerRadiusBottomRight = r,
        });
        return p;
    }

    private static Panel MakeCircleBorder(string name, float size, Color borderColor)
    {
        var r = (int)(size * 0.5f);
        var p = new Panel { Name = name, MouseFilter = Control.MouseFilterEnum.Ignore };
        p.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0, 0, 0, 0),
            BorderColor = borderColor,
            BorderWidthTop = 2, BorderWidthBottom = 2,
            BorderWidthLeft = 2, BorderWidthRight = 2,
            CornerRadiusTopLeft = r, CornerRadiusTopRight = r,
            CornerRadiusBottomLeft = r, CornerRadiusBottomRight = r,
        });
        return p;
    }

    private static void SetCircleColor(Panel p, Color color)
    {
        if (p.GetThemeStylebox("panel") is StyleBoxFlat s) s.BgColor = color;
    }

    private static void SetCircleBorderStyle(Panel p, Color borderColor, int width)
    {
        p.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0, 0, 0, 0),
            BorderColor = borderColor,
            BorderWidthTop = width, BorderWidthBottom = width,
            BorderWidthLeft = width, BorderWidthRight = width,
            CornerRadiusTopLeft = R, CornerRadiusTopRight = R,
            CornerRadiusBottomLeft = R, CornerRadiusBottomRight = R,
        });
    }

    private static Label MakeLabel()
    {
        var l = new Label
        {
            Name = "DBLabel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        l.AddThemeColorOverride("font_color", TextColor);
        l.AddThemeColorOverride("font_outline_color", new Color("0A0500"));
        l.AddThemeConstantOverride("outline_size", 5);
        l.AddThemeFontSizeOverride("font_size", 24);
        return l;
    }

    private static T GetOrCreateNode<T>(Control parent, string name, Func<T> factory) where T : Node
    {
        var node = parent.GetNodeOrNull<T>(name);
        if (node == null) { node = factory(); parent.AddChild(node); }
        return node;
    }

    private static void HideAll(Control? container)
    {
        if (container == null) return;
        foreach (var name in new[] { "DBGlow3", "DBGlow2", "DBGlow1", "DBFrame", "DBInner", "DBLabel", "DBOrbit", "DBSparks" })
        {
            var node = container.GetNodeOrNull<Node>(name);
            if (node == null) continue;
            if (node is Control c) c.Visible = false;
            else if (node is GpuParticles2D gp) { gp.Emitting = false; gp.Visible = false; }
        }
    }
}

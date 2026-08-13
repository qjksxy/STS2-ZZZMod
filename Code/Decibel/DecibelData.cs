namespace ZZZMod.Code.Decibel;

/// <summary>
///     Decibel 运行时数据（战斗内使用）。
///     跨战斗持久化通过 RunSavedData 实现。
/// </summary>
public sealed class DecibelData
{
    public const int MaxValue = 50;
    public const int DefaultCost = 20;

    public int Value { get; set; }

    public bool CanSpend(int cost) => Value >= cost;

    public bool TrySpend(int cost)
    {
        if (!CanSpend(cost)) return false;
        Value -= cost;
        return true;
    }

    public void Gain(int amount)
    {
        if (amount <= 0) return;
        Value = Math.Min(Value + amount, MaxValue);
    }
}

/// <summary>
///     跨战斗持久化的 Decibel 数据。
/// </summary>
public sealed class DecibelSaveData
{
    public int Value { get; set; }
}

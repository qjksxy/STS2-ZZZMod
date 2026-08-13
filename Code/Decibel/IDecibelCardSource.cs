namespace ZZZMod.Code.Decibel;

/// <summary>
///     实现此接口的卡牌拥有 Decibel 效果。
///     当 Decibel 值足够时，自动消耗并激活额外效果。
/// </summary>
public interface IDecibelCardSource
{
    int DecibelCost => DecibelData.DefaultCost;
}

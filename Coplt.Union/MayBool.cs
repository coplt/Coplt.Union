namespace Coplt.Union.Misc;

/// <summary>
/// Three-valued bool
/// </summary>
public enum MayBool : byte
{
    /// <summary>
    /// False
    /// </summary>
    False = 0,
    /// <summary>
    /// True
    /// </summary>
    True = 1,
    /// <summary>
    /// None
    /// </summary>
    None = 255,
}

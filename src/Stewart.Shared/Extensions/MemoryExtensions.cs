namespace Stewart.Shared.Extensions;

public static class MemoryExtensions
{
    public static double GetValue(this ulong value)
    {
        return Math.Round(value / 1024d / 1024d / 1024d, 2);
    }
}
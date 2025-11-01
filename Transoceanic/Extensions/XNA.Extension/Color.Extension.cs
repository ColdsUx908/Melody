namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    private static class Color_Extension
    {
        public static readonly Color _realGreen = new(0, 255, 0);
    }

    extension(Color color)
    {
        public string ToHexCode() => $"{color.R:X2}{color.G:X2}{color.B:X2}";

        public string FormatString(string input) => $"[c/{color.ToHexCode()}:{input}]";
    }

    extension(Color)
    {
        public static Color RealGreen => Color_Extension._realGreen;

        /// <summary>
        /// 在多个颜色间提供插值。
        /// </summary>
        /// <param name="amount">插值比率。范围为 [0, 1]。</param>
        /// <returns></returns>
        public static Color LerpMany(IList<Color> colors, float amount)
        {
            ArgumentException.ThrowIfNullOrEmpty(colors);

            switch (colors.Count)
            {
                case 1:
                    return colors[0];
                case 2:
                    return Color.Lerp(colors[0], colors[1], amount);
                default:
                    if (amount <= 0f)
                        return colors[0];
                    if (amount >= 1f)
                        return colors[^1];
                    (int index, float localRatio) = TOMathHelper.SplitFloat(amount * (colors.Count - 1));
                    return Color.Lerp(colors[index], colors[index + 1], localRatio);
            }
        }
    }
}
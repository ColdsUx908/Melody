namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension([NotNull] Color[] colors)
    {
        public Color LerpMany(float ratio)
        {
            ArgumentNullException.ThrowIfNull(colors, nameof(colors));

            ratio = Math.Clamp(ratio, 0f, colors.Length - 1);

            switch (colors.Length)
            {
                case 0:
                    return Color.White;
                case 1:
                    return colors[0];
                case 2:
                    return Color.Lerp(colors[0], colors[1], ratio);
                default:
                    if (ratio <= 0f)
                        return colors[0];
                    if (ratio >= 1)
                        return colors[^1];
                    (int index, float localRatio) = TOMathHelper.SplitFloat(Math.Clamp(ratio * (colors.Length - 1), 0f, colors.Length - 1));
                    return Color.Lerp(colors[index], colors[index + 1], localRatio);
            }
        }
    }

    extension([NotNull] List<Color> colors)
    {
        public Color LerpMany(float ratio)
        {
            ArgumentNullException.ThrowIfNull(colors, nameof(colors));

            ratio = Math.Clamp(ratio, 0f, colors.Count - 1);

            switch (colors.Count)
            {
                case 0:
                    return Color.White;
                case 1:
                    return colors[0];
                case 2:
                    return Color.Lerp(colors[0], colors[1], ratio);
                default:
                    if (ratio <= 0f)
                        return colors[0];
                    if (ratio >= 1)
                        return colors[^1];
                    (int index, float localRatio) = TOMathHelper.SplitFloat(Math.Clamp(ratio * (colors.Count - 1), 0f, colors.Count - 1));
                    return Color.Lerp(colors[index], colors[index + 1], localRatio);
            }
        }
    }
}

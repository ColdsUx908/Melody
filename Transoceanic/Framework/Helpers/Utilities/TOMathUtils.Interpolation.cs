namespace Transoceanic.Framework.Helpers;

public static partial class TOMathUtils
{
    public static class Interpolation
    {
        /// <summary>
        /// 二次方缓入。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadraticEaseIn(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio * ratio;
        }

        /// <inheritdoc cref="QuadraticEaseIn(float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadraticEaseIn(float from, float to, float ratio) => from + (to - from) * QuadraticEaseIn(ratio);

        /// <summary>
        /// 二次方缓出。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadraticEaseOut(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio * (2f - ratio);
        }

        /// <inheritdoc cref="QuadraticEaseOut(float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float QuadraticEaseOut(float from, float to, float ratio) => from + (to - from) * QuadraticEaseOut(ratio);

        /// <summary>
        /// 二次方缓入缓出。
        /// </summary>
        public static float QuadraticEaseInOut(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio < 0.5f ? 2f * ratio * ratio : -2f * ratio * ratio + 4f * ratio - 1f;
        }

        /// <inheritdoc cref="QuadraticEaseInOut(float)"/>
        public static float QuadraticEaseInOut(float from, float to, float ratio) => from + (to - from) * QuadraticEaseInOut(ratio);

        /// <summary>
        /// 三次方缓入。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicEaseIn(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio * ratio * ratio;
        }

        /// <inheritdoc cref="CubicEaseIn(float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicEaseIn(float from, float to, float ratio) => from + (to - from) * CubicEaseIn(ratio);

        /// <summary>
        /// 三次方缓出。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicEaseOut(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            float inv = 1f - ratio;
            return 1f - inv * inv * inv;
        }

        /// <inheritdoc cref="CubicEaseOut(float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicEaseOut(float from, float to, float ratio) => from + (to - from) * CubicEaseOut(ratio);

        /// <summary>
        /// 三次方缓入缓出。
        /// </summary>
        public static float CubicEaseInOut(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio < 0.5f ? 4f * ratio * ratio * ratio : 4f * ratio * ratio * ratio - 12f * ratio * ratio + 12f * ratio - 3f;
        }

        /// <inheritdoc cref="CubicEaseInOut(float)"/>
        public static float CubicEaseInOut(float from, float to, float ratio) => from + (to - from) * CubicEaseInOut(ratio);

        /// <summary>
        /// 指数缓入。
        /// </summary>
        public static float ExponentialEaseIn(float ratio, float exponent)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return MathF.Pow(ratio, exponent);
        }

        /// <inheritdoc cref="ExponentialEaseIn(float, float)"/>
        public static float ExponentialEaseIn(float from, float to, float ratio, float exponent) => from + (to - from) * ExponentialEaseIn(ratio, exponent);

        /// <summary>
        /// 指数缓出。
        /// </summary>
        public static float ExponentialEaseOut(float ratio, float exponent)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return 1f - MathF.Pow(1f - ratio, exponent);
        }

        /// <inheritdoc cref="ExponentialEaseOut(float, float)"/>
        public static float ExponentialEaseOut(float from, float to, float ratio, float exponent) => from + (to - from) * ExponentialEaseOut(ratio, exponent);

        /// <summary>
        /// 指数缓入缓出。
        /// </summary>
        public static float ExponentialEaseInOut(float ratio, float exponent)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio < 0.5f ? 0.5f * MathF.Pow(2f * ratio, exponent) : 1f - 0.5f * MathF.Pow(2f * (1f - ratio), exponent);
        }

        /// <inheritdoc cref="ExponentialEaseInOut(float, float)"/>
        public static float ExponentialEaseInOut(float from, float to, float ratio, float exponent) => from + (to - from) * ExponentialEaseInOut(ratio, exponent);

        /// <summary>
        /// 正弦缓入。
        /// <br/>计算公式为：<c>y = 1 - cos(π/2 * x)</c>
        /// </summary>
        public static float SineEaseIn(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return 1f - MathF.Cos(ratio * MathHelper.PiOver2);
        }

        /// <inheritdoc cref="SineEaseIn(float)"/>
        public static float SineEaseIn(float from, float to, float ratio) => from + (to - from) * SineEaseIn(ratio);

        /// <summary>
        /// 正弦缓出。
        /// <br/>计算公式为：<c>y = sin(π/2 * x)</c>
        /// </summary>
        public static float SineEaseOut(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return MathF.Sin(ratio * MathHelper.PiOver2);
        }

        /// <inheritdoc cref="SineEaseOut(float)"/>
        public static float SineEaseOut(float from, float to, float ratio) => from + (to - from) * SineEaseOut(ratio);

        /// <summary>
        /// 正弦缓入缓出。
        /// <br/>计算公式为：<c>y = 0.5 - 0.5 * cos(π * x)</c>
        /// </summary>
        public static float SineEaseInOut(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return 0.5f - 0.5f * MathF.Cos(ratio * MathHelper.Pi);
        }

        /// <inheritdoc cref="SineEaseInOut(float)"/>
        public static float SineEaseInOut(float from, float to, float ratio) => from + (to - from) * SineEaseInOut(ratio);

        /// <summary>
        /// 对数缓入。
        /// <br/>计算公式为：<c>y = 1 - ln((e - 1) * (1 - x) + 1)</c>
        /// </summary>
        public static float LogarithmicEaseIn(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return 1f - MathF.Log((MathF.E - 1f) * (1f - ratio) + 1f);
        }

        /// <inheritdoc cref="LogarithmicEaseIn(float)"/>
        public static float LogarithmicEaseIn(float from, float to, float ratio) => from + (to - from) * LogarithmicEaseIn(ratio);

        /// <summary>
        /// 对数缓出。
        /// <br/>计算公式为：<c>y = ln((e - 1) * x + 1)</c>
        /// </summary>
        public static float LogarithmicEaseOut(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return MathF.Log((MathF.E - 1f) * ratio + 1f);
        }

        /// <inheritdoc cref="LogarithmicEaseOut(float)"/>
        public static float LogarithmicEaseOut(float from, float to, float ratio) => from + (to - from) * LogarithmicEaseOut(ratio);

        /// <summary>
        /// 对数缓入缓出。
        /// <br/>前半段使用对数缓入，后半段使用对数缓出。
        /// </summary>
        public static float LogarithmicEaseInOut(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio < 0.5f ? 0.5f * (1f - MathF.Log((MathF.E - 1f) * (1f - 2f * ratio) + 1f)) : 0.5f * MathF.Log((MathF.E - 1f) * (2f * ratio - 1f) + 1f) + 0.5f;
        }

        /// <inheritdoc cref="LogarithmicEaseInOut(float)"/>
        public static float LogarithmicEaseInOut(float from, float to, float ratio) => from + (to - from) * LogarithmicEaseInOut(ratio);

        /// <summary>
        /// 更平滑插值，使用五次方。
        /// </summary>
        public static float SmootherStep(float ratio)
        {
            ratio = Math.Clamp(ratio, 0f, 1f);
            return ratio * ratio * ratio * (ratio * (ratio * 6f - 15f) + 10f);
        }

        /// <inheritdoc cref="SmootherStep(float)"/>
        public static float SmootherStep(float from, float to, float ratio) => from + (to - from) * SmootherStep(ratio);
    }
}
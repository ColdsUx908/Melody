namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(Vector2 vector)
    {
        public void Deconstruct(out float x, out float y)
        {
            x = vector.X;
            y = vector.Y;
        }

        /// <summary>
        /// 获取向量的顺时针旋转角。
        /// </summary>
        /// <returns>零向量返回0，否则返回 [0, 2π) 范围内的浮点值。</returns>
        public float Angle => TOMathHelper.NormalizeAngle(MathF.Atan2(vector.Y, vector.X));

        /// <summary>
        /// 安全地将向量化为单位向量。
        /// </summary>
        /// <returns>零向量返回零向量，否则返回单位向量。</returns>
        public Vector2 SafeNormalize() => vector == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector);

        /// <summary>
        /// 获取模为特定值的原向量同向向量。不改变原向量值。
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public Vector2 ToCustomLength(float length) => vector.SafeNormalize() * length;

        public Vector2 RotatedByRandom() => vector.RotatedByRandom(MathHelper.Pi);
    }

    extension(ref Vector2 vector)
    {
        public void CopyFrom(Vector2 other)
        {
            vector.X = other.X;
            vector.Y = other.Y;
        }

        public float Modulus
        {
            get => vector.Length();
            set => vector.CopyFrom(vector.ToCustomLength(value));
        }

        public float Rotation
        {
            get => vector.ToRotation();
            set => vector = new PolarVector2(vector.Length(), value);
        }
    }

    extension(Vector2)
    {
        public static float GetRotation(Vector2 from, Vector2 to) => (to - from).ToRotation();

        /// <summary>
        /// 计算两个向量的夹角。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float IncludedAngle(Vector2 a, Vector2 b) => (float)Math.Acos(Vector2.Dot(a, b) / (a.Modulus * b.Modulus));

        /// <summary>
        /// 获取两个向量角平分线的单位方向向量。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector2 UnitAngleBisector(Vector2 a, Vector2 b) => new PolarVector2((a.Angle + b.Angle) / 2);

        public static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

        /// <summary>
        /// 位似变换。
        /// </summary>
        /// <param name="value">待变换点。</param>
        /// <param name="center">位似中心。</param>
        /// <param name="ratio">位似比。</param>
        /// <returns></returns>
        public static Vector2 Homothetic(Vector2 value, Vector2 center, float ratio) => center + ratio * (value - center);
    }
}
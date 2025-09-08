namespace Transoceanic.Core.Extensions;

public static partial class TOExtensions
{
    extension(Rectangle rect)
    {
        public LineSegment TopSide => new(rect.TopLeft(), rect.TopRight());

        public LineSegment BottomSide => new(rect.BottomLeft(), rect.BottomRight());

        public LineSegment LeftSide => new(rect.TopLeft(), rect.BottomLeft());

        public LineSegment RightSide => new(rect.BottomLeft(), rect.BottomRight());

        public bool Contains(Vector2 point) =>
            point.X >= rect.Left && point.X <= rect.Right && point.Y >= rect.Top && point.Y <= rect.Bottom;
    }

    extension(Rectangle)
    {
        public static Rectangle FromCenter(Vector2 center, float width, float height)
        {
            Vector2 topLeft = center - new Vector2(width / 2f, height / 2f);
            return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)width, (int)height);
        }
    }

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
        public float Angle => vector.Y switch
        {
            > 0f => MathF.Atan2(vector.Y, vector.X),
            0f => vector.X switch
            {
                >= 0f => 0f, //零向量返回0，方向为x轴正方向返回0
                _ => MathHelper.Pi //方向为x轴负方向返回Pi
            },
            _ => MathHelper.TwoPi + MathF.Atan2(vector.Y, vector.X), //将Atan2方法返回的负值转换为正值
        };

        /// <summary>
        /// 安全地将向量化为单位向量。
        /// </summary>
        /// <returns>零向量返回零向量，否则返回单位向量。</returns>
        public Vector2 SafelyNormalized => vector == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector);

        /// <summary>
        /// 获取模为特定值的原向量同向向量。不改变原向量值。
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public Vector2 ToCustomLength(float length) => vector.SafelyNormalized * length;
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
    }

    extension(Vector2)
    {
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
    }
}

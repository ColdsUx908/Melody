using Newtonsoft.Json.Linq;

namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(Vector2 vector)
    {
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
        public float Modulus
        {
            get => vector.Length();
            set => vector = vector.ToCustomLength(value);
        }
    }
}

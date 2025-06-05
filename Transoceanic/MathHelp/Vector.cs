namespace Transoceanic.MathHelp;

public static partial class TOMathHelper
{
    /// <summary>
    /// 计算两个向量的夹角。
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static float IncludedAngle(Vector2 value1, Vector2 value2) => (float)Math.Acos(Vector2.Dot(value1, value2) / (value1.Modulus * value2.Modulus));

    /// <summary>
    /// 获取两个向量角平分线的单位方向向量。
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    public static Vector2 UnitAngleBisector(Vector2 value1, Vector2 value2) => new PolarVector2((value1.Angle + value2.Angle) / 2);
}

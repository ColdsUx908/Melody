namespace Transoceanic.Data.Geometry;

public interface ICollidableWithRectangle
{
    public abstract bool Collides(Rectangle other);
}

public interface ICollidable<TSelf, TOther>
    where TSelf : ICollidable<TSelf, TOther>
    where TOther : ICollidable<TOther, TSelf>
{
    public abstract bool Collides(TOther other);
}

public static class CollisionHelper
{
    public static class SingleCollision
    {
        public static bool FloatRectanglevCircleCollision(FloatRectangle a, Circle b)
        {
            float deltaX = Math.Clamp(b.Center.X, a.Left, a.Right) - b.Center.X;
            float deltaY = Math.Clamp(b.Center.Y, a.Top, a.Bottom) - b.Center.Y;
            return deltaX * deltaX + deltaY * deltaY <= b.Radius * b.Radius;
        }

        public static bool RotatedRectanglevCircleCollision(RotatedRectangle a, Circle b)
        {
            Vector2 newCenter = b.Center.RotatedBy(-a.Rotation, a.Center);
            float deltaX = Math.Clamp(newCenter.X, a.Source.Left, a.Source.Right) - newCenter.X;
            float deltaY = Math.Clamp(newCenter.Y, a.Source.Top, a.Source.Bottom) - newCenter.Y;
            return deltaX * deltaX + deltaY * deltaY <= b.Radius * b.Radius;
        }

        public static bool RotatedRectanglevFloatRectangleCollision(RotatedRectangle a, FloatRectangle b)
        {
            (Vector2 aPoint1, Vector2 aPoint2, Vector2 aPoint3, Vector2 aPoint4) = a.Vertices;
            ReadOnlySpan<Vector2> aPoints = [aPoint1, aPoint2, aPoint3, aPoint4];
            ReadOnlySpan<Vector2> bPoints = [b.TopLeft, b.TopRight, b.BottomLeft, b.BottomRight];

            (float sinA, float cosA) = MathF.SinCos(a.Rotation);
            if (!OverlapOnAxis(new Vector2(cosA, sinA), aPoints, bPoints))
                return false;
            if (!OverlapOnAxis(new Vector2(-sinA, cosA), aPoints, bPoints))
                return false;

            float aMin = float.MaxValue, aMax = float.MinValue;
            foreach (Vector2 point in aPoints)
            {
                float projection = point.X;
                aMin = Math.Min(aMin, projection);
                aMax = Math.Max(aMax, projection);
            }
            if (aMax < b.Left || b.Right < aMin)
                return false;

            aMin = float.MaxValue;
            aMax = float.MinValue;
            foreach (Vector2 point in aPoints)
            {
                float projection = point.Y;
                aMin = Math.Min(aMin, projection);
                aMax = Math.Max(aMax, projection);
            }
            if (aMax < b.Top || b.Bottom < aMin)
                return false;

            return true;
        }
    }

    public static bool Collides<T>(T a, Rectangle targetHitbox) where T : ICollidableWithRectangle => a.Collides(targetHitbox);

    public static bool Collides<T1, T2>(T1 a, T2 b)
        where T1 : ICollidable<T1, T2>
        where T2 : ICollidable<T2, T1>
        => a.Collides(b);

    public static bool OverlapOnAxis(Vector2 axis, ReadOnlySpan<Vector2> aPoints, ReadOnlySpan<Vector2> bPoints)
    {
        float aMin = float.MaxValue, aMax = float.MinValue;
        foreach (Vector2 point in aPoints)
        {
            float projection = Vector2.Dot(point, axis);
            aMin = Math.Min(aMin, projection);
            aMax = Math.Max(aMax, projection);
        }
        float bMin = float.MaxValue, bMax = float.MinValue;
        foreach (Vector2 point in bPoints)
        {
            float projection = Vector2.Dot(point, axis);
            bMin = Math.Min(bMin, projection);
            bMax = Math.Max(bMax, projection);
        }
        return aMax >= bMin && bMax >= aMin;
    }
}

namespace Transoceanic.Core.Utilities;

public static class TOCollisionUtils
{
    public static bool CheckAABBvLineCollision(Vector2 objectPosition, Vector2 objectDimensions, Vector2 lineStart, Vector2 lineEnd, float lineWidth, ref float collisionPoint)
    {
        if (Collision.CheckAABBvLineCollision(objectPosition, objectDimensions, lineStart, lineEnd, lineWidth, ref collisionPoint))
            return true;

        Vector2 line = lineEnd - lineStart;
        Vector2 lineDir = line.SafelyNormalized;
        float lineLength = line.Length();
        (float newY, float newXnegative) = lineDir * (lineWidth / 2f);
        Vector2 halfWidth = new(-newXnegative, newY);
        Vector2 lineMid = (lineStart + lineEnd) / 2f;
        float x1 = objectPosition.X;
        float y1 = objectPosition.Y;
        float x2 = x1 + objectDimensions.X;
        float y2 = x2 + objectDimensions.Y;
        foreach (Vector2 point in (ReadOnlySpan<Vector2>)[lineStart + halfWidth, lineStart - halfWidth, lineEnd + halfWidth, lineEnd - halfWidth, lineMid + halfWidth, lineMid - halfWidth])
        {
            if (point.X >= x1 && point.X <= x2 && point.Y >= y1 && point.Y <= y2)
            {
                collisionPoint = Math.Min(collisionPoint, MathHelper.Clamp(Vector2.Dot(point - lineStart, lineDir), 0, lineLength));
                return true;
            }
        }
        return false;
    }
}

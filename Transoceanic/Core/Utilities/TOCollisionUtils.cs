namespace Transoceanic.Core.Utilities;

public static class TOCollisionUtils
{
    public static bool AABBvWideLineCollision(Rectangle targetHitbox, Vector2 lineStart, Vector2 lineEnd, float lineWidth)
    {
        float collisionPoint = 0f;
        if (Collision.CheckAABBvLineCollision(targetHitbox.BottomLeft(), targetHitbox.Size(), lineStart, lineEnd, lineWidth, ref collisionPoint))
            return true;
        (float newY, float newXnegative) = (lineEnd - lineStart).ToCustomLength(lineWidth / 2f);
        Vector2 halfWidth = new(-newXnegative, newY);
        Vector2 lineMid = (lineStart + lineEnd) / 2f;
        foreach (Vector2 point in (ReadOnlySpan<Vector2>)[lineStart + halfWidth, lineStart - halfWidth, lineEnd + halfWidth, lineEnd - halfWidth, lineMid + halfWidth, lineMid - halfWidth, lineStart, lineEnd])
        {
            if (targetHitbox.Contains(point))
                return true;
        }
        return false;
    }

    public static bool AABBvCircularCollision(Rectangle targetHitbox, Vector2 center, float radius)
    {
        float deltaX = Math.Clamp(center.X, targetHitbox.X, targetHitbox.X + targetHitbox.Width) - center.X;
        float deltaY = Math.Clamp(center.Y, targetHitbox.Y, targetHitbox.Y + targetHitbox.Height) - center.Y;
        return deltaX * deltaX + deltaY * deltaY <= radius * radius;
    }
}

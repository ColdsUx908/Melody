using Transoceanic.Maths.Geometry.Collision;

namespace Transoceanic.Maths.Geometry;

public struct RotatedRectangle : IEquatable<RotatedRectangle>,
    ICollidableWithRectangle,
    ICollidable<RotatedRectangle, RotatedRectangle>,
    ICollidable<RotatedRectangle, FloatRectangle>,
    ICollidable<RotatedRectangle, Circle>
{
    public FloatRectangle Source;
    public float Rotation;

    public RotatedRectangle(FloatRectangle source, float rotation)
    {
        Source = source;
        Rotation = rotation;
    }

    public RotatedRectangle(Vector2 center, float width, float height, float rotation) : this(Rectangle.FromCenter(center, width, height), rotation) { }

    public readonly Vector2 Center => Source.Center;

    public readonly Vector2 TopLeft => Source.TopLeft.RotatedBy(Rotation, Center);
    public readonly Vector2 TopRight => Source.TopRight.RotatedBy(Rotation, Center);
    public readonly Vector2 BottomLeft => Source.BottomLeft.RotatedBy(Rotation, Center);
    public readonly Vector2 BottomRight => Source.BottomRight.RotatedBy(Rotation, Center);

    public readonly LineSegment TopSide => new(TopLeft, TopRight);

    public readonly LineSegment BottomSide => new(BottomLeft, BottomRight);

    public readonly LineSegment LeftSide => new(TopLeft, BottomLeft);

    public readonly LineSegment RightSide => new(BottomLeft, BottomRight);

    public readonly (Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight) Vertices
    {
        get
        {
            Vector2 center = Center;
            Vector2 widthRotated = new Vector2(Source.Width / 2, 0f).RotatedBy(Rotation);
            Vector2 heightRotated = new Vector2(0f, Source.Height / 2).RotatedBy(Rotation);
            return (center - widthRotated - heightRotated, center + widthRotated - heightRotated, center - widthRotated + heightRotated, center + widthRotated + heightRotated);
        }
    }

    public readonly (LineSegment top, LineSegment bottom, LineSegment left, LineSegment right) Sides
    {
        get
        {
            (Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight) = Vertices;
            return (new LineSegment(topLeft, topRight), new LineSegment(bottomLeft, bottomRight), new LineSegment(topLeft, bottomLeft), new LineSegment(topRight, bottomRight));
        }
    }

    public readonly bool Contains(Vector2 point) => Source.Contains(Center + (point - Center).RotatedBy(-Rotation));

    public override readonly bool Equals(object obj) => obj is RotatedRectangle other && Equals(other);
    public readonly bool Equals(RotatedRectangle other) => Source.Equals(other.Source) && Rotation == other.Rotation;
    public static bool operator ==(RotatedRectangle left, RotatedRectangle right) => left.Equals(right);
    public static bool operator !=(RotatedRectangle left, RotatedRectangle right) => !(left == right);
    public override readonly int GetHashCode() => HashCode.Combine(Source, Rotation);

    public override readonly string ToString() => $"RotatedRectangle {{Source: {Source}, Rotation: {Rotation}}}";

    public static implicit operator RotatedRectangle(FloatRectangle rect) => new(rect, 0f);

    public readonly bool Collides(RotatedRectangle other)
    {
        //SAT检测
        (Vector2 aPoint1, Vector2 aPoint2, Vector2 aPoint3, Vector2 aPoint4) = Vertices;
        (Vector2 bPoint1, Vector2 bPoint2, Vector2 bPoint3, Vector2 bPoint4) = other.Vertices;
        ReadOnlySpan<Vector2> aPoints = [aPoint1, aPoint2, aPoint3, aPoint4];
        ReadOnlySpan<Vector2> bPoints = [bPoint1, bPoint2, bPoint3, bPoint4];

        (float sinA, float cosA) = MathF.SinCos(Rotation);
        if (!CollisionHelper.OverlapOnAxis(new Vector2(cosA, sinA), aPoints, bPoints))
            return false;
        if (!CollisionHelper.OverlapOnAxis(new Vector2(-sinA, cosA), aPoints, bPoints))
            return false;

        (float sinB, float cosB) = MathF.SinCos(other.Rotation);
        if (!CollisionHelper.OverlapOnAxis(new Vector2(cosB, sinB), aPoints, bPoints))
            return false;
        if (!CollisionHelper.OverlapOnAxis(new Vector2(-sinB, cosB), aPoints, bPoints))
            return false;

        return true;
    }

    public readonly bool Collides(Rectangle other) => Collides((FloatRectangle)other);
    public readonly bool Collides(FloatRectangle other) => CollisionHelper.SingleCollision.RotatedRectanglevFloatRectangleCollision(this, other);
    public readonly bool Collides(Circle other) => CollisionHelper.SingleCollision.RotatedRectanglevCircleCollision(this, other);
}

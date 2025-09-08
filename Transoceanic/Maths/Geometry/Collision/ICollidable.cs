namespace Transoceanic.Maths.Geometry.Collision;

public interface ICollidableWithRectangle
{
    public bool Collides(Rectangle other);
}

public interface ICollidable<TSelf, TOther>
    where TSelf : ICollidable<TSelf, TOther>
    where TOther : ICollidable<TOther, TSelf>
{
    public bool Collides(TOther other);
}

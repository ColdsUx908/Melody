namespace Transoceanic.GlobalInstances.Behaviors.Projectiles;

public sealed class ProjectileMiscAI : TOGlobalProjectileBehavior
{
    public override decimal Priority => 500m;

    public override bool PreAI(Projectile projectile)
    {
        TOGlobalProjectile oceanProjectile = projectile.Ocean();
        if (oceanProjectile.AlwaysRotating)
            projectile.VelocityToRotation(oceanProjectile.RotationOffset);
        return true;
    }
}

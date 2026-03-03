namespace Transoceanic.GlobalInstances.Single;

public sealed class ProjectileMiscAI : TOGlobalProjectileBehavior
{
    public override decimal Priority => 500m;

    public override bool PreAI(Projectile projectile)
    {
        return true;
    }

    public override void PostAI(Projectile projectile)
    {
        if (projectile.AlwaysRotating)
            projectile.VelocityToRotation(projectile.RotationOffset);
    }
}

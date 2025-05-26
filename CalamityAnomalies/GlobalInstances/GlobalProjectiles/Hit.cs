using CalamityAnomalies.Override;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalProjectiles;

public partial class CAGlobalProjectile : GlobalProjectile
{
    public override bool? CanCutTiles(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            bool? result = projectileOverride.CanCutTiles();
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void CutTiles(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.CutTiles();
    }

    public override bool? CanDamage(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            bool? result = projectileOverride.CanDamage();
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool MinionContactDamage(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.MinionContactDamage())
                return false;
        }

        return false;
    }

    public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.ModifyDamageHitbox(ref hitbox);
    }

    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            bool? result = projectileOverride.CanHitNPC(target);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.OnHitNPC(target, hit, damageDone);
    }

    public override bool CanHitPvp(Projectile projectile, Player target)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.CanHitPvp(target))
                return false;
        }

        return true;
    }

    public override bool CanHitPlayer(Projectile projectile, Player target)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.CanHitPlayer(target))
                return false;
        }

        return true;
    }

    public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.OnHitPlayer(target, info);
    }

    public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            bool? result = projectileOverride.Colliding(projHitbox, targetHitbox);
            if (result is not null)
                return result;
        }

        return null;
    }
}

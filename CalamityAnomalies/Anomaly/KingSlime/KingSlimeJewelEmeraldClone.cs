namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelEmeraldClone : ModProjectile
{
    public override string Texture => JewelHandler.JewelTexturePath;

    public override void SetDefaults()
    {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.timeLeft = 60;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 0f, Projectile.Opacity, 0f);
        Projectile.Opacity = MathHelper.Lerp(0f, 0.8f, Math.Clamp(Projectile.timeLeft, 0f, 12f) / 12f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.DrawFromCenter(Projectile.Texture, Projectile.Center - Main.screenPosition, Color.Lerp(Main.zenithWorld ? Color.Purple : Color.RealGreen, Main.zenithWorld ? new Color(255, 175, 255) : new Color(175, 255, 175), Math.Clamp(Projectile.timeLeft, 0f, 12f) / 12f) * Projectile.Opacity, null, Projectile.rotation, Projectile.scale);
        return false;
    }
}

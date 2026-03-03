using CalamityMod.Buffs.DamageOverTime;
using Transoceanic.Framework.Helpers.AbstractionHandlers;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public class BloodFlame : CAModProjectile
{
    public override string LocalizationCategory => "Anomaly.EyeofCthulhu";
    public override string Texture => "CalamityMod/Projectiles/FireProj";

    public const int Lifetime = 35;
    public const int Fadetime = Lifetime - 10;
    public int MistType = -1;

    public float LifeCompletion => Timer1 / (float)Lifetime;
    public float FireSize => Utils.Remap(LifeCompletion, 0.2f, 0.5f, 0.25f, 1f);

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 10;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.MaxUpdates = 3;
        Projectile.timeLeft = Lifetime;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        Timer1++;

        if (MistType == -1)
            MistType = Main.rand.Next(3);

        Lighting.AddLight(Projectile.Center, 0.7f, 0.05f, 0.15f);

        if (Timer1 < Fadetime && Main.rand.NextBool(6))
        {
            int dustType = Main.rand.Next(3) switch
            {
                2 => DustID.CrimsonSpray,
                _ => DustID.Blood,
            };
            Vector2 cinderPos = Projectile.Center + Main.rand.NextVector2Circular(60f, 60f) * Utils.Remap(Timer1, 0f, Lifetime, 0.5f, 1f);
            float cinderSize = Utils.GetLerpValue(6f, 12f, Timer1, true);
            Dust.NewDustAction(cinderPos, 4, 4, dustType, Projectile.velocity * 0.25f, cinder =>
            {
                if (Main.rand.NextBool(3))
                {
                    cinder.scale *= 2f;
                    cinder.velocity *= 2f;
                }
                cinder.noGravity = true;
                cinder.scale *= cinderSize * 1.2f;
                cinder.velocity += Projectile.velocity * Utils.Remap(Timer1, 0f, Fadetime * 0.75f, 1f, 0.1f) * Utils.Remap(Timer1, 0f, Fadetime * 0.1f, 0.1f, 1f);
            });
        }
    }

    // Keeping the flames in place when hitting a block
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity = oldVelocity * 0.95f;
        Projectile.position -= Projectile.velocity;
        Timer1++;
        Projectile.timeLeft--;
        return false;
    }

    public override bool CanHitPlayer(Player target) => Timer1 <= MathHelper.Lerp(Fadetime, Lifetime, 0.5f);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => new Circle(Projectile.Center, 40f * FireSize).Collides(targetHitbox);

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (info.Damage <= 0)
            return;

        target.AddBuff<BrimstoneFlames>(120);
        target.AddBuff<BurningBlood>(120);
        target.AddBuff(BuffID.OnFire3, 120);

        // Cook you up (still scales with player size in case it's manipulated)
        int smokeCount = 4 + (int)MathHelper.Clamp(target.width * 0.1f, 0f, 20f);
        for (int i = 0; i < smokeCount; i++)
        {
            Vector2 smokePos = target.Center + Main.rand.NextVector2Circular(target.width * 0.5f, target.height * 0.5f);
            Vector2 smokeVel = Vector2.UnitY * Main.rand.NextFloat(-2.4f, -0.8f) * MathHelper.Clamp(target.height * 0.1f, 1f, 10f);
            ParticleHandler.SpawnParticle(new MediumMistParticle(smokePos, smokeVel, new Color(100, 255, 0), Color.DimGray, Main.rand.NextFloat(1f, 2f), 245 - Main.rand.Next(50), 0.1f));
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (LifeCompletion >= 1f)
            return false;

        Texture2D fire = TextureAssets.Projectile[Type].Value;
        Texture2D mist = ParticleHandler.GetTexture<MediumMistParticle>();

        // The conga line of colors to sift through
        Color color1 = new(200, 10, 10, 200);
        Color color2 = new(200, 50, 50, 70);
        Color color3 = new(200, 30, 30, 100);
        Color color4 = new(150, 0, 50, 100);
        float length = (Timer1 > Fadetime - 10f) ? 0.1f : 0.15f;
        float vOffset = Math.Min(Timer1, 20f);

        for (float j = 1f; j >= 0f; j -= length)
        {
            // Color
            Color fireColor = LifeCompletion < 0.1f ? Color.Lerp(Color.Transparent, color1, Utils.GetLerpValue(0f, 0.1f, LifeCompletion))
                : LifeCompletion < 0.2f ? Color.Lerp(color1, color2, Utils.GetLerpValue(0.1f, 0.2f, LifeCompletion))
                : LifeCompletion < 0.35f ? color2
                : LifeCompletion < 0.7f ? Color.Lerp(color2, color3, Utils.GetLerpValue(0.35f, 0.7f, LifeCompletion))
                : LifeCompletion < 0.85f ? Color.Lerp(color3, color4, Utils.GetLerpValue(0.7f, 0.85f, LifeCompletion))
                : Color.Lerp(color4, Color.Transparent, Utils.GetLerpValue(0.85f, 1f, LifeCompletion));
            fireColor *= (1f - j) * Utils.GetLerpValue(0f, 0.2f, LifeCompletion, true);
            Color innerColor = Color.Lerp(fireColor, Color.Black, 0.3f);

            // Positions and rotations
            Vector2 firePos = Projectile.Center - Main.screenPosition - Projectile.velocity * vOffset * j;
            float mainRot = -j * MathHelper.PiOver2 - Main.GlobalTimeWrappedHourly * (j + 1f) * 2f / length;
            float trailRot = MathHelper.PiOver4 - mainRot;

            // Draw one backtrail
            Vector2 trailOffset = Projectile.velocity * vOffset * length * 0.5f;
            Main.spriteBatch.DrawFromCenter(fire, firePos - trailOffset, null, innerColor * 0.25f, trailRot, FireSize, SpriteEffects.None);

            // Draw the main fire
            Main.spriteBatch.DrawFromCenter(fire, firePos, null, innerColor, mainRot, FireSize, SpriteEffects.None);

            // Draw the masking smoke
            Main.spriteBatch.ChangeBlendState(BlendState.Additive);
            Rectangle frame = mist.Frame(1, 3, 0, MistType);
            Main.spriteBatch.DrawFromCenter(mist, firePos, frame, Color.Lerp(fireColor, Color.White, 0.3f), mainRot, FireSize, SpriteEffects.None);
            Main.spriteBatch.DrawFromCenter(mist, firePos, frame, fireColor, mainRot, FireSize * 3f, SpriteEffects.None);
            Main.spriteBatch.ChangeBlendState(BlendState.AlphaBlend);
        }

        return false;
    }
}

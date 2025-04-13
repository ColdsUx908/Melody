using System.IO;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core;
using Transoceanic.Data;
using Transoceanic.Systems;

namespace CalamityAnomalies.Projectiles.LegendaryItems;

public class ImmortalVoidRain : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.LegendaryItems";

    private const float homingDistance = 10000f;
    private NPC npcTarget = null;

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.coldDamage = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.scale = 1f;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 1230; //追踪10秒
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Projectile.localAI[0]);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Projectile.localAI[0] = reader.ReadSingle();
    }

    public override void AI()
    {
        Lighting.AddLight((int)((Projectile.position.X + Projectile.width / 2) / 16f), (int)((Projectile.position.Y + Projectile.height / 2) / 16f), 175f / 255f, 1f, 1f);

        switch (Projectile.ai[0])
        {
            case 0: //初始化
                TOProjectileUtils.VelocityToRotation(Projectile);
                Projectile.ai[0] = 1;
                Projectile.ai[1] = 0;
                break;
            case 1: //逐渐减速
                Projectile.ai[1]++;
                TOMathHelper.ToCustomLength(Projectile.velocity, Projectile.ai[2] * (20 - Projectile.ai[1]) / 20);
                if (Projectile.ai[1] == 20)
                {
                    Projectile.ai[0] = 2;
                    Projectile.ai[1] = 0;
                }
                break;
            case 2: //停顿9帧后向上飞出
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 11)
                {
                    //SoundEngine.PlaySound(SoundID.NPCHit5, Projectile.Center);
                    for (int i = 0; i < 10; i++)
                    {
                        int dusttype = Main.rand.NextBool() ? 68 : 67;
                        if (Main.rand.NextBool(4))
                            dusttype = 80;
                        Vector2 dspeed = new(Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-7f, 7f));
                        int dust = Dust.NewDust(Projectile.Center, 1, 1, dusttype, dspeed.X, dspeed.Y, 50, default, 1.1f);
                        Main.dust[dust].noGravity = true;
                    }

                    float velocity2 = Main.rand.NextFloat(10f, 12.5f);
                    TOProjectileUtils.SetVelocityandRotation(Projectile, new Vector2(0, -velocity2));
                    Projectile.extraUpdates = 1;
                    Projectile.ai[0] = 3;
                    Projectile.ai[1] = 0;
                }
                break;
            case 3: //寻找目标并追踪；在追踪超过5秒后加速
                Projectile.ai[1]++;
                if (!TOKinematicUtils.Homing(Projectile, npcTarget, rotating: true))
                    npcTarget = TOKinematicUtils.GetNPCTarget(Projectile.Center, homingDistance, true, true, PriorityType.LifeMax);
                if (Projectile.ai[1] > 600)
                    Projectile.velocity *= 1.0015f;
                break;
        }
    }

    public override Color? GetAlpha(Color lightColor) => new Color(1f, 1f, 1f, 1f) * Projectile.Opacity;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Rectangle? frame = texture.Frame();

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = texture.Size() * 0.5f;

        Main.spriteBatch.Draw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, 0, 0f);
        //Projectile.DrawProjectileWithBackglow(Cryogen.BackglowColor, lightColor, 4f);
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        //SoundEngine.PlaySound(SoundID.Item27 with { Volume = SoundID.Item27.Volume * 0.25f }, Projectile.Center);
        for (int j = 0; j < 3; j++)
        {
            int snowDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Snow, 0f, 0f, 0, default, 1f);
            Main.dust[snowDust].noGravity = true;
            Main.dust[snowDust].noLight = true;
            Main.dust[snowDust].scale = 0.7f;
        }
    }

    public override bool? CanHitNPC(NPC target)
    {
        if (TOMain.BossActive && !target.IsBossTO())
            return false;
        return true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        Player player = Main.player[Projectile.owner];
        if (!player.Ocean().Celesgod)
            return;

        if (false) //待补充
        {
            //modifiers.SetInstantKill();
        }
        else
            target.life -= (int)(target.lifeMax * 0.00025);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Player player = Main.player[Projectile.owner];
        if (player is null)
            return;

        if (player.Ocean().Celesgod)
        {
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 4f, targetHitbox);
}

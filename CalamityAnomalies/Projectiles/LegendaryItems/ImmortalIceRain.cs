﻿using System.IO;
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

namespace CalamityAnomalies.Projectiles.LegendaryItems;

public class ImmortalIceRain : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.LegendaryItems";

    private const float homingDistance = 4000f;

    private int AI_Phase
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    private int AI_Timer
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    private float AI_InitialVelocity => Projectile.ai[2];

    private NPC AI_NPCTarget
    {
        get
        {
            int temp = (int)Projectile.ai[3];
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }
        set => Projectile.ai[3] = value?.whoAmI ?? -1;
    }

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

    public override void AI()
    {
        AI_Timer++;

        Lighting.AddLight((int)((Projectile.position.X + Projectile.width / 2) / 16f), (int)((Projectile.position.Y + Projectile.height / 2) / 16f), 175f / 255f, 1f, 1f);

        switch (AI_Phase)
        {
            case 0: //初始化
                TOProjectileUtils.VelocityToRotation(Projectile);
                AI_Phase = 1;
                AI_Timer = 0;
                break;
            case 1: //逐渐减速
                TOMathHelper.ToCustomLength(Projectile.velocity, AI_InitialVelocity * (20 - AI_Timer) / 20);
                if (AI_Timer == 20)
                {
                    AI_Phase = 2;
                    AI_Timer = 0;
                }
                break;
            case 2: //停顿10帧后向上飞出
                if (AI_Timer > 10)
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
                    AI_Phase = 3;
                    AI_Timer = 0;
                }
                break;
            case 3: //寻找目标并追踪；在追踪超过3秒后加速
                if (!TOKinematicUtils.Homing(Projectile, AI_NPCTarget, rotating: true))
                    AI_NPCTarget = TOKinematicUtils.GetNPCTarget(Projectile.Center, homingDistance, true, true, PriorityType.LifeMax);
                if (AI_Timer > 360)
                    Projectile.velocity *= 1.0013f;
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
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 4f, targetHitbox);
}

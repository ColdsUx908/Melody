using System.Collections.Generic;
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

namespace CalamityAnomalies.Projectiles.LegendaryItems;

public class ImmortalBloodRain : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.LegendaryItems";

    private const float homingDistance = 10000f;
    private Player playerTarget = null;
    private NPC npcTarget = null;
    private static readonly List<int> InstantKill =
    [
        //1,
        //2
    ];

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
        Projectile.timeLeft = 930; //追踪5秒
        Projectile.extraUpdates = 2; //速度UpUp
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
            case 1: //寻找目标并追踪，优先追踪玩家；在追踪超过3秒后加速，速度显著更高
                Projectile.ai[1]++;
                if (!TOKinematicUtils.Homing(Projectile, playerTarget, rotating: true))
                {
                    if ((playerTarget = TOKinematicUtils.GetPvPPlayerTarget(Main.player[Projectile.owner], Projectile.Center, homingDistance, true, PriorityType.LifeMax)) != null)
                        break;
                    if (!TOKinematicUtils.Homing(Projectile, npcTarget, rotating: true))
                        npcTarget = TOKinematicUtils.GetNPCTarget(Projectile.Center, homingDistance, true, true, PriorityType.LifeMax);
                }
                if (Projectile.ai[1] > 360)
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
        Player player = Main.player[Projectile.owner];
        if (!player.Ocean().Celesgod)
            return;

        if (InstantKill.Contains(target.type)) //待补充
            modifiers.FlatBonusDamage += target.lifeMax;
        else if (!target.IsABoss())
            modifiers.FlatBonusDamage += (int)(target.lifeMax * 0.015);
        else
            modifiers.FlatBonusDamage += (int)(target.lifeMax * 0.005);
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

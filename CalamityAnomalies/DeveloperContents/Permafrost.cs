using System;
using System.Collections.Generic;
using CalamityAnomalies.UI;
using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Skies;
using CalamityMod.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic.GameData;
using Transoceanic.GameData.Utilities;
using Transoceanic.GlobalInstances;
using Transoceanic.IL;
using Transoceanic.MathHelp;
using Transoceanic.Visual;
using static CalamityMod.NPCs.SupremeCalamitas.SupremeCalamitas;
using static CalamityMod.Projectiles.Boss.SCalRitualDrama;

namespace CalamityAnomalies.DeveloperContents;

public class Permafrost : CANPCOverride<SupremeCalamitas>
{
    #region 枚举、数值、属性、AI状态
    public enum AttackType
    {
        Welcome = 0,
        NonSpell1 = 1,
    }

    private static class Data
    {
        public static Color BlueColor => Color.Lerp(Color.LightCyan, Color.Cyan, TOMathHelper.GetTimeSin(0.2f, 1f, 0f, true));

        public static List<Color> NameColors { get; } =
        [
            Color.Cyan,
            Color.LightYellow,
            Color.LightPink,
            Color.SkyBlue,
            Color.LightSkyBlue
        ];

        public const float DespawnDistance = 15000f;

        public const int ArenaSize = 125;
    }

    public int Arena_TopLeftX
    {
        get => (int)AnomalyNPC.AnomalyAI[0];
        set => AnomalyNPC.SetAnomalyAI(value, 0);
    }

    public int Arena_TopLeftY
    {
        get => (int)AnomalyNPC.AnomalyAI[1];
        set => AnomalyNPC.SetAnomalyAI(value, 1);
    }

    public Point Arena_TopLeft => new(Arena_TopLeftX, Arena_TopLeftY);

    public Vector2 Arena_TopLeftPosition => Arena_TopLeft.ToWorldCoordinates();

    public Vector2 GetArenaTilePosition(int i, int j) => (Arena_TopLeft + new Point(i, j)).ToWorldCoordinates();

    public Vector2 Arena_CenterPosition => GetArenaTilePosition(Data.ArenaSize / 2, Data.ArenaSize / 2);

    public AttackType CurrentAttack
    {
        get => (AttackType)(int)AnomalyNPC.AnomalyAI[2];
        set => AnomalyNPC.SetAnomalyAI((int)value, 2);
    }

    #endregion

    private const string localizationPrefix = CAMain.ModLocalizationPrefix + "DeveloperContents.Permafrost.";

    public override decimal Priority => 935m; //ICE

    public override bool ShouldProcess => ModNPC.permafrost;

    public override void SetDefaults()
    {
    }

    public override void OnSpawn(IEntitySource source)
    {
        NPC.dontTakeDamage = true;
    }

    #region AI
    public override bool PreAI()
    {
        StartUp();

        switch (CurrentAttack)
        {
            case AttackType.Welcome:
                Welcome();
                break;
        }

        return false;
    }


    #region 行为函数
    private void StartUp()
    {
        ModNPC.FrameType = FrameAnimationType.UpwardDraft;
        ModNPC.FrameChangeSpeed = 0.15f;

        CalamityGlobalNPC.SCal = NPC.whoAmI;
        ModNPC.HandleMusicVariables();

        if (Main.slimeRain)
        {
            Main.StopSlimeRain(true);
            CalamityNetcode.SyncWorld();
        }

        if (CalamityConfig.Instance.BossesStopWeather)
            Calamity.StopRain();

        #region Despawn
        if (!NPC.TargetClosestIfInvalid(true, Data.DespawnDistance))
        {
            if (SoundEngine.TryGetActiveSound(ModNPC.BulletHellRumbleSlot, out ActiveSound rumbleSound) && rumbleSound.IsPlaying)
                rumbleSound.Stop();

            ModNPC.canDespawn = true;

            NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0f, 0.065f);
            NPC.velocity = Vector2.Lerp(Vector2.UnitY * -4f, Vector2.Zero, (float)Math.Sin(MathHelper.Pi * NPC.Opacity));
            ModNPC.forcefieldOpacity = Utils.GetLerpValue(0.1f, 0.6f, NPC.Opacity, true);
            if (NPC.alpha >= 230)
            {
                if (DownedBossSystem.downedCalamitas)
                {
                    // Create a teleport line effect
                    Dust.QuickDustLine(NPC.Center, ModNPC.initialRitualPosition, 500f, Color.Cyan);
                    NPC.Center = ModNPC.initialRitualPosition;

                    // Make the town NPC spawn.
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        TOActivator.NewNPCAction<DILF>(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0f, 12f));
                }

                NPC.active = false;
                NPC.netUpdate = true;
            }

            for (int i = 0; i < MathHelper.Lerp(2f, 6f, 1f - NPC.Opacity); i++)
            {
                Dust brimstoneFire = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Square(-24f, 24f), DustID.Torch);
                brimstoneFire.color = Color.Cyan;
                brimstoneFire.velocity = Vector2.UnitY * -Main.rand.NextFloat(2f, 3.25f);
                brimstoneFire.scale = Main.rand.NextFloat(0.95f, 1.15f);
                brimstoneFire.noGravity = true;
            }
        }
        else
            ModNPC.canDespawn = false;
        #endregion

        #region Directioning
        bool currentlyCharging = NPC.ai[1] == 2f;
        if (!currentlyCharging && Math.Abs(Target.Center.X - NPC.Center.X) > 16f)
            NPC.spriteDirection = (Target.Center.X < NPC.Center.X).ToDirectionInt();
        #endregion

        #region Forcefield and shield
        // Shield effect rotation
        ModNPC.rotateToPlayer = ModNPC.rotateToPlayer.AngleLerp((Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation() + MathHelper.PiOver2, 0.04f);
        ModNPC.rotateAwayPlayer = ModNPC.rotateAwayPlayer.AngleLerp((Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation() - MathHelper.PiOver2, 0.04f);

        if (ModNPC.hitTimer > 0)
            ModNPC.hitTimer--;

        if (NPC.dontTakeDamage && !ModNPC.hasDoneDeathAnim) // Dust visuals for shield when immune
        {
            Vector2 sustVel = new Vector2(-78 * Main.rand.NextFloat(0.95f, 1.05f), 0).RotatedBy(ModNPC.rotateToPlayer + MathHelper.PiOver2).RotatedByRandom(1.4);
            Dust sust = Dust.NewDustPerfect(NPC.Center + sustVel, 269, sustVel * Main.rand.NextFloat(0.001f, 0.03f));
            sust.noGravity = true;
            sust.scale = Main.rand.NextFloat(0.5f, 0.9f);
            sust.alpha = 200;
            sust.color = Main.rand.NextBool() ? Color.Goldenrod : Color.Red;
        }

        Vector2 hitboxSize = new(ModNPC.forcefieldScale * 216f / 1.4142f);
        hitboxSize = Vector2.Max(hitboxSize, new Vector2(42, 44));
        if (NPC.Size != hitboxSize)
            NPC.Size = hitboxSize;
        bool shouldNotUseShield = ModNPC.bulletHellCounter2 % BulletHellDuration != 0 || ModNPC.attackCastDelay > 0
            || NPC.AnyNPCs(ModContent.NPCType<DevourerofGodsHead>()) || NPC.ai[0] == 1f || NPC.ai[0] == 2f;

        // Make the shield and forcefield fade away in SCal's acceptance phase.
        if (OceanNPC.LifeRatio <= 0.01f && ModNPC.hasDoneDeathAnim)
        {
            ModNPC.shieldOpacity = MathHelper.Lerp(ModNPC.shieldOpacity, 0f, 0.08f);
            ModNPC.forcefieldScale = MathHelper.Lerp(ModNPC.forcefieldScale, 0f, 0.08f);
        }

        // Summon a shield if the next attack will be a charge.
        // Make it go away if certain triggers happen during this, such as a bullet hell starting, however.
        else if ((ModNPC.willCharge && ModNPC.AttackCloseToBeingOver || NPC.ai[1] == 2f) && !shouldNotUseShield)
        {
            if (NPC.ai[1] != 2f)
            {
                float idealRotation = NPC.AngleTo(Target.Center);
                float angularOffset = Math.Abs(MathHelper.WrapAngle(ModNPC.shieldRotation - idealRotation));

                if (angularOffset > 0.04f)
                {
                    ModNPC.shieldRotation = ModNPC.shieldRotation.AngleLerp(idealRotation, 0.125f);
                    ModNPC.shieldRotation = ModNPC.shieldRotation.AngleTowards(idealRotation, 0.18f);
                }
            }
            else if (!ModNPC.permafrost)
            {
                // Emit dust off the skull at the position of its eye socket.
                for (float num6 = 1f; num6 < 16f; num6 += 1f)
                {
                    Dust dust = Dust.NewDustPerfect(NPC.Center, 182);
                    dust.position = Vector2.Lerp(NPC.position, NPC.oldPosition, num6 / 16f) + NPC.Size * 0.5f;
                    dust.position += ModNPC.shieldRotation.ToRotationVector2() * 42f;
                    dust.position += (ModNPC.shieldRotation - MathHelper.PiOver2).ToRotationVector2() * (float)Math.Cos(NPC.velocity.ToRotation()) * -4f;
                    dust.noGravity = true;
                    dust.velocity = NPC.velocity;
                    dust.color = Color.Red;
                    dust.scale = MathHelper.Lerp(0.6f, 0.85f, 1f - num6 / 16f);
                }
            }

            // Shrink the force-field since it looks strange when charging.
            ModNPC.forcefieldScale = MathHelper.Lerp(ModNPC.forcefieldScale, 0.45f, 0.08f);
            ModNPC.shieldOpacity = MathHelper.Lerp(ModNPC.shieldOpacity, 1f, 0.08f);
        }
        // Make the shield disappear if it is no longer relevant and regenerate the forcefield.
        else
        {
            ModNPC.shieldOpacity = MathHelper.Lerp(ModNPC.shieldOpacity, 0f, 0.08f);
            ModNPC.forcefieldScale = MathHelper.Lerp(ModNPC.forcefieldScale, 1f, 0.08f);
        }
        #endregion

        #region Arena
        if (!ModNPC.spawnArena && Main.netMode != NetmodeID.MultiplayerClient)
        {
            ModNPC.safeBox.X = ModNPC.spawnX = ModNPC.spawnXReset = (int)NPC.Center.X - Data.ArenaSize * 8;
            ModNPC.spawnX2 = ModNPC.spawnXReset2 = (int)NPC.Center.X + Data.ArenaSize * 8;
            ModNPC.safeBox.Y = ModNPC.spawnY = ModNPC.spawnYReset = (int)NPC.Center.Y - Data.ArenaSize * 8;
            ModNPC.safeBox.Width = Data.ArenaSize * 16;
            ModNPC.safeBox.Height = Data.ArenaSize * 16;
            ModNPC.spawnYAdd = 100;

            int safeBoxTilesX = (ModNPC.safeBox.X + ModNPC.safeBox.Width / 2) / 16;
            int safeBoxTilesY = (ModNPC.safeBox.Y + ModNPC.safeBox.Height / 2) / 16;
            int safeBoxTileWidth = Data.ArenaSize / 2 + 1;

            int minX = safeBoxTilesX - safeBoxTileWidth;
            int maxX = safeBoxTilesX + safeBoxTileWidth;
            int minY = safeBoxTilesY - safeBoxTileWidth;
            int maxY = safeBoxTilesY + safeBoxTileWidth;

            Arena_TopLeftX = minX;
            Arena_TopLeftY = minY;

            foreach ((Tile tile, int i, int j) in TOTileUtils.GetBorderTiles(minX, maxX, minY, maxY, 2))
            {
                if (!tile.HasTile)
                {
                    tile.TileType = (ushort)ModContent.TileType<ArenaTile>();
                    tile.Get<TileWallWireStateData>().HasTile = true;
                }

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                else
                    WorldGen.SquareTileFrame(i, j, true);
            }
        }
        #endregion

        #region Enrage and DR
        if (ModNPC.spawnArena && !Target.Hitbox.Intersects(ModNPC.safeBox))
        {
            float projectileVelocityMultCap = !Target.Hitbox.Intersects(ModNPC.safeBox) && ModNPC.spawnArena ? 2f : 1.5f;
            ModNPC.uDieLul = MathHelper.Clamp(ModNPC.uDieLul * 1.01f, 1f, projectileVelocityMultCap);
            ModNPC.protectionBoost = false;
            if (!Target.Hitbox.Intersects(ModNPC.safeBox))
                ModNPC.protectionBoost = true;
        }
        else
        {
            ModNPC.uDieLul = MathHelper.Clamp(ModNPC.uDieLul * 0.99f, 1f, 2f);
            ModNPC.protectionBoost = false;
        }
        CalamityNPC.CurrentlyEnraged = !Target.Hitbox.Intersects(ModNPC.safeBox);

        // Permafrost fucks mounts if you exit his arena.
        /*
        if (ModNPC.permafrost)
        {
            if (!player.Hitbox.Intersects(ModNPC.safeBox) && player.mount.Active)
            {
                player.ResetEffects();
                player.head = -1;
                player.body = -1;
                player.legs = -1;
                player.handon = -1;
                player.handoff = -1;
                player.back = -1;
                player.front = -1;
                player.shoe = -1;
                player.waist = -1;
                player.shield = -1;
                player.neck = -1;
                player.face = -1;
                player.balloon = -1;
                player.mount.Dismount(player);
            }
        }
        */

        // Set DR to be 99% and unbreakable if enraged. Boost DR during the 5th attack.
        if (ModNPC.protectionBoost && !ModNPC.gettingTired5)
        {
            CalamityNPC.DR = enragedDR;
            CalamityNPC.unbreakableDR = true;
        }
        else
        {
            CalamityNPC.DR = normalDR;
            CalamityNPC.unbreakableDR = false;
            if (ModNPC.startFifthAttack)
                CalamityNPC.DR *= 1.2f;
        }
        #endregion
    }

    private void Welcome()
    {
        NPC.dontTakeDamage = true;

        switch (Timer1++)
        {
            case 0:
                break;
            case 90:
                break;
            case 300:
                CurrentAttack = AttackType.NonSpell1;
                Timer1 = 0;
                break;
        }
    }
    #endregion

    #endregion

    #region Draw
    public delegate void Orig_DrawForcefield(SupremeCalamitas self, SpriteBatch spriteBatch);

    [DetourMethodTo<SupremeCalamitas>]
    public static void Detour_DrawForcefield(Orig_DrawForcefield orig, SupremeCalamitas self, SpriteBatch spriteBatch)
    {
        if (self.permafrost)
        {
            spriteBatch.EnterShaderRegion();

            float lifeRatio = self.NPC.Ocean().LifeRatio;

            if (lifeRatio < 0.05f)
                self.forcefieldOpacity = 0.75f;
            if (lifeRatio <= 0.01f)
                self.forcefieldOpacity = 0.6f;

            float flickerPower = 0f;
            if (lifeRatio < 0.6f)
                flickerPower += 0.1f;
            if (lifeRatio < 0.3f)
                flickerPower += 0.25f;
            if (self.postMusicHit)
                flickerPower += 0.61f;
            if (lifeRatio < 0.05f)
                flickerPower += Main.rand.NextFloat(0.7f, 1f);
            if (lifeRatio <= 0.01f)
                flickerPower += 0.08f;
            float opacity = self.forcefieldOpacity;
            opacity *= MathHelper.Lerp(1f, MathHelper.Max(1f - flickerPower, 0.56f), (float)Math.Pow(Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.Lerp(3f, 5f, flickerPower)), 24D));
            opacity *= self.musicSyncCounter is <= 0 and > -30 ? Utils.GetLerpValue(120, 0, self.musicSyncCounter, true) : 0.75f;

            Texture2D forcefieldTexture = ForcefieldTexture.Value;
            GameShaders.Misc["CalamityMod:SupremeShield"].UseImage1("Images/Misc/Perlin");

            Color forcefieldColor = Color.Cyan;
            Color secondaryForcefieldColor = Color.LightBlue;

            forcefieldColor *= opacity;
            secondaryForcefieldColor *= opacity;

            GameShaders.Misc["CalamityMod:SupremeShield"].UseSecondaryColor(secondaryForcefieldColor);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseColor(forcefieldColor);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseSaturation(1);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseOpacity(0.65f);
            GameShaders.Misc["CalamityMod:SupremeShield"].Apply();

            Texture2D centerTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/CentralGold").Value;

            Texture2D immuneTex = ModContent.Request<Texture2D>("CalamityMod/Particles/SemiCircularSmearVertical").Value;
            if (self.postMusicHit)
                spriteBatch.Draw(centerTexture, self.NPC.Center - Main.screenPosition, null, Color.White with { A = 0 } * opacity * 2f, self.rotateAwayPlayer, centerTexture.Size() * 0.5f, self.forcefieldScale * 0.088f * self.forcefieldPureVisualScale, SpriteEffects.None, 0f);
            if (!self.NPC.dontTakeDamage)
                spriteBatch.Draw(forcefieldTexture, self.NPC.Center - Main.screenPosition, null, Color.White * opacity, self.postMusicHit ? self.rotateToPlayer : 0, forcefieldTexture.Size() * 0.5f, self.forcefieldScale * 3f * self.forcefieldPureVisualScale, SpriteEffects.None, 0f);
            else
                spriteBatch.Draw(immuneTex, self.NPC.Center - Main.screenPosition, null, Color.White * opacity * 0.3f, self.rotateToPlayer, immuneTex.Size() * 0.5f, self.forcefieldScale * 1.35f * self.forcefieldPureVisualScale, SpriteEffects.None, 0f);
            spriteBatch.ExitShaderRegion();
        }
        else
            orig(self, spriteBatch);
    }

    public override bool PreDrawCalBossBar(BetterBossHealthBar.BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y)
    {
        newBar.DrawBaseBars(spriteBatch, x, y, null, Data.BlueColor * newBar.AnimationCompletionRatio2);
        newBar.DrawNPCName(spriteBatch, x, y, null,
            Data.BlueColor * newBar.AnimationCompletionRatio2,
            Data.NameColors.LerpMany(TOMathHelper.GetTimeSin(5f, 0.6f, 0f, true) * newBar.AnimationCompletionRatio2),
            Math.Clamp(OceanNPC.ActiveTime, 0f, 120f) / 80f + TOMathHelper.GetTimeSin(1f, 1f, 0f, true));
        newBar.DrawBigLifeText(spriteBatch, x, y);
        newBar.DrawExtraSmallText(spriteBatch, x, y);

        return false;
    }
    #endregion
}

public class PermafrostRitualDrama : CAProjectileOverride<SCalRitualDrama>
{
    public override decimal Priority => 935m; //ICE

    public override bool ShouldProcess => Projectile.ai[1] == 1f; //将召唤永冻的弹幕

    public override bool PreAI()
    {
        {
            if (Projectile.timeLeft == 689)
            {
                for (int i = 0; i < 2; i++)
                {
                    Particle bloom = new BloomParticle(Projectile.Center, Vector2.Zero, Color.Lerp(Color.LightBlue, Color.Cyan, 0.3f), 0f, 0.55f, 270, false);
                    GeneralParticleHandler.SpawnParticle(bloom);
                }
                Particle bloom2 = new BloomParticle(Projectile.Center, Vector2.Zero, Color.White, 0f, 0.5f, 270, false);
                GeneralParticleHandler.SpawnParticle(bloom2);
            }
            if (Projectile.timeLeft == 689 - 180)
            {
                Particle bloom = new BloomParticle(Projectile.Center, Vector2.Zero, new Color(121, 21, 77), 0f, 0.85f, 90, false);
                GeneralParticleHandler.SpawnParticle(bloom);
            }

            // If needed, these effects may continue after the ritual timer, to ensure that there are no awkward
            // background changes between the time it takes for SCal to appear after this projectile is gone.
            // If SCal is already present, this does not happen.
            if (!NPC.AnyNPCs(ModContent.NPCType<SupremeCalamitas>()))
            {
                SCalSky.OverridingIntensity = Utils.GetLerpValue(90f, TotalRitualTime - 25f, ModProjectile.Time, true);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Utils.GetLerpValue(90f, TotalRitualTime - 25f, ModProjectile.Time, true);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower *= Utils.GetLerpValue(3400f, 1560f, Main.LocalPlayer.Distance(Projectile.Center), true) * 4f;
            }

            // Summon SCal right before the ritual effect ends.
            // The projectile lingers a little longer, however, to ensure that desync delays in MP do not interfere with the background transition.
            if (ModProjectile.Time == TotalRitualTime - 1f)
                ModProjectile.SummonSCal();

            if (ModProjectile.Time >= TotalRitualTime)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && !NPC.AnyNPCs(ModContent.NPCType<SupremeCalamitas>()))
                    Projectile.Kill();
                return false;
            }

            int fireReleaseRate = ModProjectile.Time > 150f ? 2 : 1;
            for (int i = 0; i < fireReleaseRate; i++)
            {
                if (Main.rand.NextBool())
                {
                    float variance = Main.rand.NextFloat(-25f, 25f);
                    TOActivator.NewDustPerfectAction(Projectile.Center + new Vector2(variance, 20), DustID.RainbowMk2, d =>
                    {
                        d.velocity = -Vector2.UnitY.RotatedBy(variance * 0.02f) * Main.rand.NextFloat(1.1f, 2.1f) * (ModProjectile.Time * 0.023f);
                        d.color = Main.rand.NextBool() ? Color.Red : new Color(121, 21, 77);
                        d.scale = Main.rand.NextFloat(0.35f, 1.2f);
                        d.fadeIn = 0.7f;
                        d.noGravity = true;
                    });
                }
            }

            ModProjectile.Time++;

            return false;
        }
    }
}

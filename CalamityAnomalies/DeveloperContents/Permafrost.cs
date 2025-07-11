using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Skies;
using CalamityMod.Tiles;
using Terraria.Graphics.Shaders;
using static CalamityMod.NPCs.SupremeCalamitas.SupremeCalamitas;
using static CalamityMod.Projectiles.Boss.SCalRitualDrama;

namespace CalamityAnomalies.DeveloperContents;

public class Permafrost : CANPCBehavior<SupremeCalamitas>, ILocalizationPrefix
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

        public static Color NameColor => NameColors.LerpMany(TOMathHelper.GetTimeSin(0.5f, 0.6f, 0f, true));

        public const float DespawnDistance = 15000f;

        public const int ArenaSize = 201;
    }

    public Point Arena_TopLeft
    {
        get => AnomalyNPC.AnomalyAI2[0].p;
        set
        {
            if (AnomalyNPC.AnomalyAI2[0].p != value)
            {
                AnomalyNPC.AnomalyAI2[0].p = value;
                AnomalyNPC.AIChanged2[0] = true;
            }
        }
    }

    public Vector2 Arena_TopLeftPosition => Arena_TopLeft.ToWorldCoordinates();

    public Vector2 GetArenaTilePosition(int i, int j) => (Arena_TopLeft + new Point(i, j)).ToWorldCoordinates();

    public Vector2 Arena_CenterPosition => GetArenaTilePosition(Data.ArenaSize / 2, Data.ArenaSize / 2);

    public AttackType CurrentAttack
    {
        get => (AttackType)AnomalyNPC.AnomalyAI[0].i;
        set
        {
            int temp = (int)value;
            if (AnomalyNPC.AnomalyAI[0].i != temp)
            {
                AnomalyNPC.AnomalyAI[0].i = temp;
                AnomalyNPC.AIChanged[0] = true;
            }
        }
    }

    #endregion 枚举、数值、属性、AI状态

    public string LocalizationPrefix => CAMain.ModLocalizationPrefix + "DeveloperContents.Permafrost.";

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

    private void StartUp()
    {
        NPC.damage = 0;

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
            CalamityMod_.StopRain();

        #region 目标与脱战
        if (!NPC.TargetClosestIfInvalid(true, Data.DespawnDistance))
        {
            if (SoundEngine.TryGetActiveSound(ModNPC.BulletHellRumbleSlot, out ActiveSound rumbleSound) && rumbleSound.IsPlaying)
                rumbleSound.Stop();

            ModNPC.canDespawn = true;

            NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0f, 0.065f);
            NPC.velocity = Vector2.Lerp(Vector2.UnitY * -4f, Vector2.Zero, (float)Math.Sin(MathHelper.Pi * NPC.Opacity));
            ModNPC.forcefieldOpacity = Utils.GetLerpValue(0.3f, 1f, NPC.Opacity, true);
            if (NPC.alpha >= 230)
            {
                if (DownedBossSystem.downedCalamitas)
                {
                    // Create a teleport line effect
                    Dust.QuickDustLine(NPC.Center, ModNPC.initialRitualPosition, 500f, Color.Cyan);
                    NPC.Center = ModNPC.initialRitualPosition;

                    // Make the town NPC spawn.
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NPC.NewNPCAction<DILF>(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0f, 12f));
                }

                NPC.active = false;
                NPC.netUpdate = true;
            }

            for (int i = 0; i < MathHelper.Lerp(2f, 6f, 1f - NPC.Opacity); i++)
            {
                Dust.NewDustPerfectAction(NPC.Center + Main.rand.NextVector2Square(-24f, 24f), DustID.BlueTorch, iceFire =>
                {
                    iceFire.velocity = Vector2.UnitY * -Main.rand.NextFloat(2f, 3.25f);
                    iceFire.color = Color.Cyan;
                    iceFire.scale = Main.rand.NextFloat(0.95f, 1.15f);
                    iceFire.noGravity = true;
                });
            }
        }
        else
            ModNPC.canDespawn = false;
        #endregion 目标与脱战

        #region 判定方向
        bool currentlyCharging = NPC.ai[1] == 2f;
        if (!currentlyCharging && Math.Abs(Target.Center.X - NPC.Center.X) > 16f)
            NPC.spriteDirection = (Target.Center.X < NPC.Center.X).ToDirectionInt();
        #endregion 判定方向

        #region 力场和护盾
        // Shield effect rotation
        ModNPC.rotateToPlayer = ModNPC.rotateToPlayer.AngleLerp((Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation() + MathHelper.PiOver2, 0.04f);
        ModNPC.rotateAwayPlayer = ModNPC.rotateAwayPlayer.AngleLerp((Target.Center - NPC.Center).SafeNormalize(Vector2.UnitY).ToRotation() - MathHelper.PiOver2, 0.04f);

        if (ModNPC.hitTimer > 0)
            ModNPC.hitTimer--;

        if (NPC.dontTakeDamage && !ModNPC.hasDoneDeathAnim) // Dust visuals for shield when immune
        {
            Vector2 sustVel = new Vector2(-78 * Main.rand.NextFloat(0.95f, 1.05f), 0).RotatedBy(ModNPC.rotateToPlayer + MathHelper.PiOver2).RotatedByRandom(1.4);
            Dust.NewDustPerfectAction(NPC.Center + sustVel, DustID.Sandnado, sust =>
            {
                sust.velocity = sustVel * Main.rand.NextFloat(0.001f, 0.03f);
                sust.alpha = 200;
                sust.color = Main.rand.NextBool(3) ? Color.Goldenrod : CAMain.AnomalyUltramundaneColor;
                sust.scale = Main.rand.NextFloat(0.5f, 0.9f);
                sust.noGravity = true;
            });
        }

        Vector2 hitboxSize = new(ModNPC.forcefieldScale * 216f / 1.4142f);
        hitboxSize = Vector2.Max(hitboxSize, new Vector2(42, 44));
        if (NPC.Size != hitboxSize)
            NPC.Size = hitboxSize;
        bool shouldNotUseShield = ModNPC.bulletHellCounter2 % BulletHellDuration != 0 || ModNPC.attackCastDelay > 0
            || NPC.ai[0] == 1f || NPC.ai[0] == 2f;

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
        #endregion 力场和护盾

        #region 竞技场
        if (!ModNPC.spawnArena && Main.netMode != NetmodeID.MultiplayerClient)
        {
            ModNPC.spawnX2 = ModNPC.spawnXReset2 = (int)NPC.Center.X + Data.ArenaSize * 8;
            ModNPC.safeBox.X = ModNPC.spawnX = ModNPC.spawnXReset = (int)NPC.Center.X - Data.ArenaSize * 8;
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

            Arena_TopLeft = new Point(minX, minY);

            foreach ((Tile tile, int i, int j) in TOTileUtils.GetBorderTiles(minX, maxX, minY, maxY, 2))
            {
                if (!tile.HasTile)
                {
                    tile.SetTileType<ArenaTile>();
                    tile.Get<TileWallWireStateData>().HasTile = true;
                }

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                else
                    WorldGen.SquareTileFrame(i, j, true);
            }

            ModNPC.spawnArena = true;
        }
        #endregion 竞技场

        #region 激怒和伤害减免
        if (ModNPC.spawnArena && !Target.Hitbox.Intersects(ModNPC.safeBox))
        {
            float projectileVelocityMultCap = !Target.Hitbox.Intersects(ModNPC.safeBox) && ModNPC.spawnArena ? 2f : 1.5f;
            ModNPC.uDieLul = MathHelper.Clamp(ModNPC.uDieLul * 1.01f, 1f, projectileVelocityMultCap);
            ModNPC.protectionBoost = !Target.Hitbox.Intersects(ModNPC.safeBox);
        }
        else
        {
            ModNPC.uDieLul = MathHelper.Clamp(ModNPC.uDieLul * 0.99f, 1f, 2f);
            ModNPC.protectionBoost = false;
        }
        CalamityNPC.CurrentlyEnraged = !Target.Hitbox.Intersects(ModNPC.safeBox);

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
        #endregion 激怒和伤害减免
    }

    private void Welcome()
    {
        NPC.dontTakeDamage = true;

        switch (Timer1++)
        {
            case 180:
                if (TOMain.GeneralClient)
                    TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "Welcome1", Color.LightCyan);
                break;
            case 300:
                if (TOMain.GeneralClient)
                    TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "Welcome2", Color.LightCyan);
                break;
            case 480:
                if (TOMain.GeneralClient)
                    TOLocalizationUtils.ChatLocalizedText(LocalizationPrefix + "Beginning", Color.LightCyan);

                GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, Color.Cyan, new Vector2(1f), 0, 0.1f, 7f, 30));
                GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, CAMain.AnomalyUltramundaneColor * 0.8f, new Vector2(2f), 0, 0.05f, 6f, 36));
                GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, Vector2.Zero, Color.LightCyan, new Vector2(1f), 0, 0.05f, 4f, 45));
                for (int i = 0; i < 100; i++)
                {
                    Vector2 dustVel = new PolarVector2(15f, Main.rand.NextFloat(100f));
                    Dust.NewDustPerfectAction(NPC.Center + dustVel * 3f, Main.rand.NextBool(4) ? DustID.BlueTorch : DustID.IceTorch, d =>
                    {
                        d.velocity = dustVel * Main.rand.NextFloat(0.3f, 1.3f);
                        d.scale = Main.rand.NextFloat(2f, 3.2f);
                        d.noGravity = true;
                    });
                }
                SoundEngine.PlaySound(BulletHellEndSound, NPC.Center);
                SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, Target.Center);

                CurrentAttack = AttackType.NonSpell1;
                Timer1 = 0;
                break;
        }
    }
    #endregion AI

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
            MiscShaderData miscShaderData = GameShaders.Misc["CalamityMod:SupremeShield"];
            miscShaderData.UseImage1("Images/Misc/Perlin");

            Color forcefieldColor = Color.Cyan;
            Color secondaryForcefieldColor = Color.SkyBlue;

            forcefieldColor *= opacity;
            secondaryForcefieldColor *= opacity;

            miscShaderData.UseSecondaryColor(secondaryForcefieldColor);
            miscShaderData.UseColor(forcefieldColor);
            miscShaderData.UseSaturation(1);
            miscShaderData.UseOpacity(0.65f);
            miscShaderData.Apply();

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
        newBar.DrawMainBar(spriteBatch, x, y);
        newBar.DrawComboBar(spriteBatch, x, y);
        newBar.DrawSeperatorBar(spriteBatch, x, y, Data.BlueColor * newBar.AnimationCompletionRatio2);
        newBar.DrawNPCName(spriteBatch, x, y, null,
            Data.BlueColor * newBar.AnimationCompletionRatio2,
            Data.NameColor * newBar.AnimationCompletionRatio2,
            Math.Clamp(OceanNPC.ActiveTime, 0f, 360f) / 240f + TOMathHelper.GetTimeSin(0.5f, 1f, TOMathHelper.PiOver3, true) + OceanNPC.LifeRatioReverse / 2f);
        newBar.DrawBigLifeText(spriteBatch, x, y);
        newBar.DrawExtraSmallText(spriteBatch, x, y);

        return false;
    }
    #endregion Draw
}

public class PermafrostRitualDrama : CAProjectileBehavior<SCalRitualDrama>
{
    private const string localizationPrefix = CAMain.ModLocalizationPrefix + "DeveloperContents.Permafrost.";

    public override decimal Priority => 935m; //ICE

    public override bool ShouldProcess => Projectile.ai[1] == 1f; //将召唤永冻的弹幕

    public override bool PreAI()
    {
        CalamityPlayer calamityPlayer = Main.LocalPlayer.Calamity();

        if (Projectile.timeLeft == 689)
        {
            for (int i = 0; i < 2; i++)
                GeneralParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Blue, Color.Cyan, 0.7f), 0f, 0.55f, 270, false));
            GeneralParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.White, 0f, 0.5f, 270, false));
        }

        if (Projectile.timeLeft == 689 - 180)
            GeneralParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, CAMain.AnomalyUltramundaneColor, 0f, 0.85f, 90, false));

        // If needed, these effects may continue after the ritual timer, to ensure that there are no awkward
        // background changes between the time it takes for SCal to appear after this projectile is gone.
        // If SCal is already present, this does not happen.
        if (!NPC.AnyNPCs<SupremeCalamitas>())
        {
            SCalSky.OverridingIntensity = Utils.GetLerpValue(90f, TotalRitualTime - 25f, ModProjectile.Time, true);
            calamityPlayer.GeneralScreenShakePower = Utils.GetLerpValue(90f, TotalRitualTime - 25f, ModProjectile.Time, true);
            calamityPlayer.GeneralScreenShakePower *= Utils.GetLerpValue(3400f, 1560f, Main.LocalPlayer.Distance(Projectile.Center), true) * 4f;
        }

        // Summon SCal right before the ritual effect ends.
        // The projectile lingers a little longer, however, to ensure that desync delays in MP do not interfere with the background transition.
        if (ModProjectile.Time == TotalRitualTime - 1f)
        {
            // Summon Permafrost.
            // All the other acoustic and visual effects can happen client-side.
            if (TOMain.GeneralClient)
            {
                Vector2 spawnPosition = Projectile.Center - new Vector2(53f, 39f);
                NPC.NewNPCAction<SupremeCalamitas>(NPC.GetBossSpawnSource(Player.FindClosest(spawnPosition, 1, 1)), spawnPosition, action: n =>
                {
                    TOLocalizationUtils.ChatLocalizedText(localizationPrefix + "Spawn", Color.Lerp(Color.Blue, Color.Cyan, 0.7f));
                    n.GetModNPC<SupremeCalamitas>().permafrost = true;
                });
            }

            // Make sound.
            SoundEngine.PlaySound(Cryogen.DeathSound, Projectile.Center);

            // Make a sudden screen shake.
            calamityPlayer.GeneralScreenShakePower = Utils.GetLerpValue(3400f, 1560f, Main.LocalPlayer.Distance(Projectile.Center), true) * 16f;

            // Generate a dust explosion at the ritual's position.
            for (int i = 0; i < 90; i++)
            {
                Dust.NewDustPerfectAction(Projectile.Center, DustID.IceGolem, d =>
                {
                    d.velocity = new PolarVector2(Main.rand.NextFloat(1.5f, 36f), Main.rand.NextFloat(100f));
                    d.scale = Main.rand.NextFloat(1.2f, 2.3f);
                    d.noGravity = true;
                });
            }
            for (int i = 0; i < 40; i++)
            {
                Vector2 sparkVel = new PolarVector2(Main.rand.NextFloat(2f, 22f), Main.rand.NextFloat(100f));
                GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center + sparkVel * 2, sparkVel, false, 120, Main.rand.NextFloat(1.55f, 2.75f), Color.Cyan, true, true));
            }
            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Cyan, new Vector2(2f), 0, 0f, 4f, 55));
            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, CAMain.AnomalyUltramundaneColor * 0.8f, new Vector2(2f), 0, 0f, 2.8f, 58));
            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.LightCyan, new Vector2(2f), 0, 0f, 2f, 60));
        }

        if (ModProjectile.Time >= TotalRitualTime)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient && !NPC.AnyNPCs<SupremeCalamitas>())
                Projectile.Kill();
            return false;
        }

        int fireReleaseRate = ModProjectile.Time > 150f ? 2 : 1;
        for (int i = 0; i < fireReleaseRate; i++)
        {
            if (Main.rand.NextBool())
            {
                float variance = Main.rand.NextFloat(-25f, 25f);
                Dust.NewDustPerfectAction(Projectile.Center + new Vector2(variance, 20), DustID.RainbowMk2, d =>
                {
                    d.velocity = new PolarVector2(ModProjectile.Time * 0.023f * Main.rand.NextFloat(1.1f, 2.1f), variance * 0.02f - MathHelper.PiOver2);
                    d.color = Main.rand.NextBool() ? Color.Cyan : CAMain.AnomalyUltramundaneColor;
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

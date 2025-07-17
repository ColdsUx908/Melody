using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee;
using static CalamityMod.Items.Weapons.Melee.Exoblade;

namespace CalamityAnomalies.Tweaks._5_2_PostYharon;

/* 星流之刃
 * 
 * 伤害提升至999（原灾厄：915）。
 * 攻击速度提升15%。
 * 使用时间调整至10（原灾厄：49）（避免一些奇怪的卡手）。
 * 剑气伤害倍率提升至0.55（原灾厄：0.35），追踪开始时间延迟降低至12（原灾厄：24），可穿墙追踪。
 * 冲刺后可进行强化左键攻击的时间提升至60（原灾厄：37），冲刺冷却调整至30（原灾厄：60）。
 * 强化左键攻击大小乘数提升至2（原灾厄：1.5），剑气命中时会产生伤害倍率为0.4的爆炸。
 * 
 * 神光
 * 强化左键攻击大小乘数提升至4，挥砍生成的剑气数量提升至6（原灾厄：4）。
 * 在愚人节期间，剑气伤害x3，大小x1.5，速度x1.2。
 */

public sealed class Exoblade_Tweak : CAItemTweak<Exoblade>, ILocalizationPrefix
{
    public string LocalizationPrefix => CAMain.TweakLocalizationPrefix + "5.2.Exoblade.";

    public override void SetStaticDefaults()
    {
        BeamNoHomeTime = 12;
        OpportunityForBigSlash = 60 * 3;
        BigSlashUpscaleFactor = 2f;
        NotTrueMeleeDamagePenalty = 0.55f;
        LungeCooldown = 30 * 3;
    }

    public override void SetDefaults()
    {
        Item.damage = 999;
        Item.useTime = 10;
        Item.useAnimation = 10;
    }

    public override void UpdateInventory(Player player)
    {
        BigSlashUpscaleFactor = player.name == "神光" ? 4f : 2f;
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[ApplyingType] = Projectile.ActiveProjectiles.Any(p => p.Owner == player && p.ModProjectile is ExobladeProj);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        ApplyCATweakColorToDamage();
    }
}

public sealed class ExobladeProj_Detour : ModProjectileDetour<ExobladeProj>
{
    private const float BladeLength = 180f;

    public delegate void Orig_DoBehavior_Swinging(ExobladeProj self);

    public static void Detour_DoBehavior_Swinging(Orig_DoBehavior_Swinging orig, ExobladeProj self)
    {
        Projectile projectile = self.Projectile;
        bool god = projectile.Owner.name == "神光";

        int newSwingTime = (int)(self.SwingTime * 0.85f);
        if (projectile.timeLeft > newSwingTime)
            projectile.timeLeft = newSwingTime;

        if (projectile.timeLeft == (int)(self.SwingTime * 0.55f))
            SoundEngine.PlaySound(self.PerformingPowerfulSlash ? BigSwingSound : SwingSound, projectile.Center);

        Lighting.AddLight(projectile.Owner.MountedCenter + self.SwordDirection * 100, Color.Lerp(Color.GreenYellow, Color.DeepPink, MathF.Pow(self.Progression, 3)).ToVector3() * 1.6f * MathF.Sin(self.Progression * MathHelper.Pi));

        // Decide the scale of the sword.
        if (projectile.scale < self.IdealSize)
            projectile.scale = MathHelper.Lerp(projectile.scale, self.IdealSize, 0.08f);

        //Make the sword get smaller near the end of the slash
        if (!projectile.Owner.channel && self.Progression > 0.7f)
            projectile.scale = (0.5f + 0.5f * MathF.Sqrt(1f - (self.Progression - 0.7f) / 0.3f)) * self.IdealSize;

        if (Main.rand.NextFloat(3f) < self.RiskOfDust)
        {
            Dust.NewDustPerfectAction<AuricBarDust>(projectile.Owner.MountedCenter + self.SwordDirection * BladeLength * projectile.scale * MathF.Sqrt(Main.rand.NextFloat(0.5f, 1f)), auric =>
            {
                auric.velocity = self.SwordDirection.RotatedBy(-MathHelper.PiOver2 * self.Direction) * 2f;
                auric.noGravity = true;
                auric.alpha = 10;
                auric.scale = 0.5f;
            });
        }

        if (Main.rand.NextFloat() < self.RiskOfDust)
        {
            Dust.NewDustPerfectAction(projectile.Owner.MountedCenter + self.SwordDirection * BladeLength * projectile.scale * MathF.Sqrt(Main.rand.NextFloat(0.2f, 1f)), DustID.RainbowMk2, must =>
            {
                must.velocity = self.SwordDirection.RotatedBy(MathHelper.PiOver2 * self.Direction) * 2.6f;
                must.color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.9f);
                must.scale = 0.3f;
                must.fadeIn = Main.rand.NextFloat() * 1.2f;
                must.noGravity = true;
            });
        }

        // Create a bunch of homing beams.
        int beamShootStart = (int)(self.SwingTime * 0.6f);
        int beamShootPeriod = (int)(self.SwingTime * 0.35f);
        int beamShootEnd = beamShootStart + beamShootPeriod;
        beamShootPeriod /= BeamsPerSwing + (self.PerformingPowerfulSlash && god).ToInt() * 2;
        if (projectile.Owner == Main.LocalPlayer && self.Timer >= beamShootStart && self.Timer < beamShootEnd && (self.Timer - beamShootStart) % beamShootPeriod == 0f)
        {
            Vector2 boltVelocity = projectile.velocity.RotatedByRandom(MathHelper.PiOver4 * 0.3);
            boltVelocity *= projectile.Owner.ActiveItem().shootSpeed;
            int boltDamage = (int)(self.Projectile.damage * NotTrueMeleeDamagePenalty);
            Projectile.NewProjectileAction<Exobeam>(projectile.GetSource_FromAI(), projectile.Center + boltVelocity * 5f, boltVelocity, boltDamage, projectile.knockBack / 3f, projectile.owner, p =>
            {
                if (self.PerformingPowerfulSlash)
                {
                    p.ai[1] = 1f;
                    if (god && TOWorld.AprilFools)
                    {
                        p.damage *= 3;
                        p.scale *= 1.5f;
                        p.velocity *= 1.2f;
                    }
                }
            });
        }
    }

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_DoBehavior_Swinging);
    }
}

public sealed class Exobeam_Tweak : CAProjectileTweak<Exobeam>
{
    public const float ExplosionDamageFactor2 = 0.4f;

    public override void SetDefaults()
    {
        Projectile.timeLeft = 360;
    }

    public override bool PreAI()
    {
        // Aim very, very quickly at targets.
        // This takes a small amount of time to happen, to allow the blade to go in its intended direction before immediately racing
        // towards the nearest target.
        ref int targetIndex = ref ModProjectile.TargetIndex;
        if (ModProjectile.Time >= BeamNoHomeTime)
        {
            if (targetIndex >= 0)
            {
                NPC target = Main.npc[targetIndex];
                if (!target.active || !target.CanBeChasedBy())
                    targetIndex = -1;
                else
                {
                    Vector2 idealVelocity = Projectile.SafeDirectionTo(target.Center) * (Projectile.velocity.Length() + 6.5f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealVelocity, 0.08f);
                }
            }

            if (targetIndex == -1)
            {
                NPC potentialTarget = Projectile.Center.ClosestNPCAt(1600f, true);
                if (potentialTarget != null)
                    targetIndex = potentialTarget.whoAmI;
                else
                    Projectile.velocity *= 0.99f;
            }
        }

        Projectile.rotation = Projectile.velocity.ToRotation();

        if (Main.rand.NextBool())
        {
            Dust.NewDustPerfectAction(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.RainbowMk2, must =>
            {
                must.velocity = Projectile.velocity * -2.6f;
                must.color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.9f);
                must.scale = 0.3f;
                must.fadeIn = Main.rand.NextFloat(1.2f);
                must.noGravity = true;
            });
        }

        Projectile.scale = Utils.GetLerpValue(0f, 0.1f, Projectile.timeLeft / 600f, true);

        if (Projectile.FinalExtraUpdate())
            ModProjectile.Time++;

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.ai[1] == 1f && Main.myPlayer == Projectile.owner)
        {
            SoundEngine.PlaySound(BigHitSound, Projectile.Center, null);
            Projectile.NewProjectileAction<Exoboom>(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero, (int)(Projectile.damage * ExplosionDamageFactor2), 0f, Projectile.owner);
        }
    }
}

/*
using CalamityAnomalies.Items.ItemRarities;

namespace CalamityAnomalies.DeveloperContents;

public class ImmortalCeless : LegendaryItem, ILocalizedModType
{
    #region 传奇
    public int Phase_1 { get; private set; } = 1; //Max: 6
    public int Phase_2 { get; private set; } = 1; //Max: 9
    public int Phase_3 { get; private set; } = 1; //Max: 21

    public override void SetPhase(Player player)
    {
        CAPlayer.DownedBoss downedGet = player.Anomaly().DownedBossCalamity;

        if (downedGet.PrimordialWyrm)
        {
            Phase_1 = 6;
            Phase_2 = 9;
            Phase_3 = 21;
        }
        else if (downedGet.DoG)
        {
            Phase_1 = 5;
            Phase_2 = 8;
            if (downedGet.Focus)
                Phase_3 = 20;
            else if (downedGet.LastBoss)
                Phase_3 = 19;
            else if (downedGet.Yharon)
                Phase_3 = 18;
            else
                Phase_3 = 17;
        }
        else if (downedGet.Signus)
        {
            Phase_1 = 4;
            Phase_2 = 7;
            if (downedGet.Polterghast)
                Phase_3 = 16;
            else
                Phase_3 = 15;
        }
        else if (downedGet.Frost)
        {
            Phase_1 = 3;
            if (downedGet.LunaticCultist)
            {
                Phase_2 = 6;
                if (downedGet.Providence)
                    Phase_3 = 14;
                else if (downedGet.MoonLord)
                    Phase_3 = 13;
                else
                    Phase_3 = 12;
            }
            else if (downedGet.Leviathan)
            {
                Phase_2 = 5;
                if (downedGet.Golem)
                    Phase_3 = 11;
                else
                    Phase_3 = 10;
            }
            else
            {
                Phase_2 = 4;
                Phase_3 = 9;
            }
        }
        else if (downedGet.Cryogen)
        {
            Phase_1 = 2;
            Phase_2 = 3;
            if (downedGet.CalamitasClone)
                Phase_3 = 8;
            else if (downedGet.MechBossAll)
                Phase_3 = 7;
            else
                Phase_3 = 6;
        }
        else
        {
            Phase_1 = 1;
            if (downedGet.Deerclops)
            {
                Phase_2 = 2;
                if (downedGet.WallOfFlesh)
                    Phase_3 = 5;
                else
                    Phase_3 = 4;
            }
            else
            {
                Phase_2 = 1;
                if (downedGet.EvilBoss2)
                    Phase_3 = 3;
                else if (downedGet.EvilBoss)
                    Phase_3 = 2;
                else
                    Phase_3 = 1;
            }
        }
    }

    public override void SetPower(Player player) => HasPower = player.Ocean().Celesgod;
    #endregion

    private int ProjAmount
    {
        get
        {
            if (HasPower)
            {
                return 10;
            }
            else
            {
                return 5;
            }
        }
    }

    //private static readonly int projVoid = ModContent.ProjectileType<ImmortalVoidRain>();
    private static readonly int ProjIce = ModContent.ProjectileType<ImmortalIceRain>();
    private static readonly int ProjBlood = ModContent.ProjectileType<ImmortalBloodRain>();
    //private static readonly int godType = ModContent.ProjectileType<ImmortalGod>();

    public new string LocalizationCategory => "DeveloperContents";

    public override void SetDefaults()
    {
        Item.width = 124;
        Item.height = 52;
        Item.damage = 8;
        Item.DamageType = DamageClass.Magic;
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = TOMain.CelestialPrice;
        Item.knockBack = 1f;
        Item.useTime = Item.useAnimation = 6;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.autoReuse = true;
        Item.shoot = ProjIce;
        Item.shootSpeed = 1f;
    }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        //Item.staff[Item.type] = true;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override void UpdateInventory(Player player)
    {
        SetPhase(player);
        SetPower(player);
    }

    public override bool? UseItem(Player player)
    {
        return base.UseItem(player);
    }

    public override void HoldItem(Player player)
    {
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse == 2)
        {
            Projectile.NewProjectileAction(source, position, velocity.ToCustomLength(10f), ProjBlood, damage * 3, knockback * 3f, -1, p => p.velocity = new PolarVector2(15f, Main.rand.NextFloat(0f, MathHelper.TwoPi)));
        }
        else
        {
            Projectile.RotatedProj(ProjAmount, MathHelper.TwoPi / ProjAmount, source, player.Center, new Vector2(0f, -15f), type, damage, knockback, -1, p => p.ai[2] = 15f); //ai[2]传递速度信息

            float offsetangle;
            for (int i = 0; i < ProjAmount * 2; i++)
            {
                float velocity2 = Main.rand.NextFloat(4f, 10f);
                int t = Main.rand.Next(i, i * 4);
                offsetangle = (float)Math.Pow(t + 1, 2) + t * 3;
                Vector2 velocity3 = new PolarVector2(velocity2, offsetangle);
                Projectile.NewProjectileAction(source, player.Center, velocity3, type, damage, knockback, -1, p => p.ai[2] = velocity2); //ai[2]传递速度信息
            }
        }

        return false;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {

    }

    public override float UseTimeMultiplier(Player player) => player.altFunctionUse == 2 ? 1f : 10f;
}

public class ImmortalIceRain : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "DeveloperContents";

    public override string Texture => CAMain.TexturePrefix + "Touhou/Ice2";

    private const float homingDistance = 4000f;

    public int Phase
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public int Timer
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public float InitialVelocity => Projectile.ai[2];

    public NPC NPCTarget
    {
        get
        {
            int temp = (int)Projectile.localAI[0];
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }
        set => Projectile.localAI[0] = value?.whoAmI ?? -1;
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
        TOGlobalProjectile oceanProjectile = Projectile.Ocean();
        oceanProjectile.RotationOffset = MathHelper.PiOver2;
        oceanProjectile.AlwaysRotating = true;

        NPCTarget = null;
    }

    public override void AI()
    {
        Timer++;

        Lighting.AddLight((int)((Projectile.position.X + Projectile.width / 2) / 16f), (int)((Projectile.position.Y + Projectile.height / 2) / 16f), 175f / 255f, 1f, 1f);

        switch (Phase)
        {
            case 0: //逐渐减速
                Projectile.velocity.Modulus = InitialVelocity * (20 - Timer) / 20;
                if (Timer == 20)
                {
                    Phase = 1;
                    Timer = 0;
                }
                break;
            case 1: //停顿10帧后向上飞出
                if (Timer > 10)
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
                    Projectile.SetVelocityandRotation(new Vector2(0, -velocity2));
                    Projectile.extraUpdates = 1;
                    Phase = 2;
                    Timer = 0;
                }
                break;
            case 2: //寻找目标并追踪；在追踪超过3秒后加速
                if (!Projectile.Homing(NPCTarget))
                    NPCTarget = TOKinematic.GetNPCTarget(Projectile.Center, homingDistance, true, true, PriorityType.LifeMax);
                if (Timer > 360)
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
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Snow, action: d =>
            {
                d.noGravity = true;
                d.noLight = true;
                d.scale = 0.7f;
            });
        }
    }

    public override bool? CanHitNPC(NPC target)
    {
        if (TOMain.BossActive && !target.TOBoss)
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

public class ImmortalBloodRain : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "DeveloperContents";

    public override string Texture => CAMain.TexturePrefix + "Touhou/Ice3";

    private const float homingDistance = 10000f;

    public int Phase
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public int Timer
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public Player PlayerTarget
    {
        get
        {
            int temp = (int)Projectile.ai[2];
            return temp is >= 0 and < Main.maxPlayers ? Main.player[temp] : null;
        }
        set => Projectile.ai[2] = value?.whoAmI ?? -1;
    }

    public NPC NPCTarget
    {
        get
        {
            int temp = (int)Projectile.ai[2] - 300;
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }
        set => Projectile.ai[2] = (value?.whoAmI ?? -301) + 300;
    }

    private static readonly HashSet<int> InstantKill =
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
        TOGlobalProjectile oceanProjectile = Projectile.Ocean();
        oceanProjectile.RotationOffset = MathHelper.PiOver2;
        oceanProjectile.AlwaysRotating = true;

        PlayerTarget = null;
        NPCTarget = null;
    }

    public override void AI()
    {
        Lighting.AddLight((int)((Projectile.position.X + Projectile.width / 2) / 16f), (int)((Projectile.position.Y + Projectile.height / 2) / 16f), 175f / 255f, 1f, 1f);

        Timer++;
        if (!Projectile.Homing(PlayerTarget))
        {
            if ((PlayerTarget = TOKinematic.GetPvPPlayerTarget(Main.player[Projectile.owner], Projectile.Center, homingDistance, true, PriorityType.LifeMax)) != null)
                return;
            if (!Projectile.Homing(NPCTarget))
                NPCTarget = TOKinematic.GetNPCTarget(Projectile.Center, homingDistance, true, true, PriorityType.LifeMax);
        }
        if (Timer > 360)
            Projectile.velocity *= 1.0013f;
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
            Dust.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Snow, d =>
            {
                d.noGravity = true;
                d.noLight = true;
                d.scale = 0.7f;
            });
        }
    }

    public override bool? CanHitNPC(NPC target)
    {
        if (TOMain.BossActive && !target.TOBoss)
            return false;
        return true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        Player player = Main.player[Projectile.owner];
        if (!player.Ocean().Celesgod)
            return;

        if (InstantKill.Contains(target.type)) //待补充
            modifiers.SetInstantKillBetter(target);
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
*/
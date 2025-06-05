using Transoceanic.GlobalInstances;

namespace CalamityAnomalies.Projectiles.LegendaryItems;

public class ImmortalBloodRain : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.LegendaryItems";

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
        Projectile.Ocean().AlwaysRotating = true;

        PlayerTarget = null;
        NPCTarget = null;
    }

    public override void AI()
    {
        Lighting.AddLight((int)((Projectile.position.X + Projectile.width / 2) / 16f), (int)((Projectile.position.Y + Projectile.height / 2) / 16f), 175f / 255f, 1f, 1f);

        switch (Phase)
        {
            case 0: //初始化
                Projectile.VelocityToRotation();
                Phase = 1;
                Timer = 0;
                break;
            case 1: //寻找目标并追踪，优先追踪玩家；在追踪超过3秒后加速，速度显著更高
                Timer++;
                if (!Projectile.Homing(PlayerTarget))
                {
                    if ((PlayerTarget = TOKinematic.GetPvPPlayerTarget(Main.player[Projectile.owner], Projectile.Center, homingDistance, true, PriorityType.LifeMax)) != null)
                        break;
                    if (!Projectile.Homing(NPCTarget))
                        NPCTarget = TOKinematic.GetNPCTarget(Projectile.Center, homingDistance, true, true, PriorityType.LifeMax);
                }
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
            TOActivator.NewDustAction(Projectile.Center, Projectile.width, Projectile.height, DustID.Snow, d =>
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

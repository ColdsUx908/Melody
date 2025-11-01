using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class KingSlimeJewelSapphire_Anomaly : AnomalyNPCBehavior<KingSlimeJewelSapphire>
{
    public const float DespawnDistance = 5000f;
    public int BuffedShootCooldownTime => HasEnteredPhase2 ? 300 : 240;

    public bool HasEnteredPhase2
    {
        get => NPC.ai[2] == 1f;
        set => NPC.ai[2] = value.ToInt();
    }

    public bool CanAttack
    {
        get => NPC.ai[3] != 1f;
        set => NPC.ai[3] = (!value).ToInt();
    }

    #region 宝石
    /// <summary>
    /// 王冠绿宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public unsafe NPC JewelEmerald
    {
        get => Main.npc[AnomalyNPC.AnomalyAI32[0].bytes[0]];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[0].bytes[0] != temp)
            {
                AnomalyNPC.AnomalyAI32[0].bytes[0] = temp;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 判定绿宝石实例是否有效。
    /// <br/>该方法假定本NPC的Master有效。
    /// </summary>
    /// <param name="jewel"></param>
    /// <returns></returns>
    public bool IsEmeraldValid(NPC jewel) => jewel.active && jewel.ModNPC is KingSlimeJewelEmerald && jewel.Ocean().TryGetMaster(NPCID.KingSlime, out NPC emeraldMaster) && emeraldMaster == OceanNPC.Master;
    public bool JewelEmeraldAlive => IsEmeraldValid(JewelEmerald);
    public bool JewelEmeraldPhase2 => JewelEmeraldAlive && JewelHandler.CheckIfPhase2(JewelEmerald);

    /// <summary>
    /// 王冠红宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <c>DummyNPC</c>。
    /// </summary>
    public unsafe NPC JewelRuby
    {
        get => Main.npc[AnomalyNPC.AnomalyAI32[0].bytes[1]];
        set
        {
            byte temp = (byte)value.whoAmI;
            if (AnomalyNPC.AnomalyAI32[0].bytes[1] != temp)
            {
                AnomalyNPC.AnomalyAI32[0].bytes[1] = temp;
                AnomalyNPC.AIChanged32[0] = true;
            }
        }
    }
    /// <summary>
    /// 判定红宝石实例是否有效。
    /// <br/>该方法假定本NPC的Master有效。
    /// </summary>
    /// <param name="jewel"></param>
    /// <returns></returns>
    public bool IsRubyValid(NPC jewel) => jewel.active && jewel.ModNPC is KingSlimeJewelRuby && jewel.Ocean().TryGetMaster(NPCID.KingSlime, out NPC rubyMaster) && rubyMaster == OceanNPC.Master;
    public bool JewelRubyAlive => IsRubyValid(JewelRuby);
    public bool JewelRubyPhase2 => JewelRubyAlive && JewelHandler.CheckIfPhase2(JewelRuby);
    #endregion 宝石

    public override void SetDefaults()
    {
        NPC.width = 28;
        NPC.height = 28;
        JewelEmerald = NPC.DummyNPC;
        JewelRuby = NPC.DummyNPC;
    }

    public override bool PreAI()
    {
        if (!OceanNPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            JewelHandler.Despawn(NPC);
            return false;
        }

        Lighting.AddLight(NPC.Center, 0f, 0f, 1f);
        NPC.TargetClosestIfInvalid(true, DespawnDistance);

        bool shouldFindEmerald = !JewelEmeraldAlive;
        bool shouldFindRuby = !JewelRubyAlive;
        if (shouldFindEmerald || shouldFindRuby)
        {
            foreach (NPC npc in NPC.ActiveNPCs)
            {
                if (shouldFindEmerald && IsEmeraldValid(npc))
                    JewelEmerald = npc;
                else if (shouldFindRuby && IsRubyValid(npc))
                    JewelRuby = npc;
            }
        }

        JewelHandler.Movement(NPC, Vector2.Lerp(Target.Center, master.Center, 0.35f), 17f, 17f, 0.2f, 300f, -300f, 300f, -300f);

        if (CanAttack)
            Timer1++;
        if (Timer1 >= BuffedShootCooldownTime)
        {
            Timer1 = 0;
            if (JewelEmeraldAlive)
            {
                CreateDustBetweenJewels(JewelEmerald);
                BuffedShoot_Emerald();
            }
            if (JewelRubyAlive)
            {
                CreateDustBetweenJewels(JewelRuby);
                BuffedShoot_Ruby();
            }
        }

        NPC.netUpdate = true;
        return false;

        void CreateDustBetweenJewels(NPC otherJewel)
        {
            int maxDustIterations = (int)Vector2.Distance(NPC.Center, otherJewel.Center); //distance
            int maxDust = 100;
            int dustDivisor = Math.Max(maxDustIterations / maxDust, 2);

            Vector2 dustLineStart = NPC.Center;
            Vector2 dustLineEnd = otherJewel.Center;
            Vector2 spinningpoint = new Vector2(0f, -1f).RotatedByRandom();
            for (int i = 0; i < maxDustIterations; i++)
            {
                if (i % dustDivisor == 0)
                {
                    Vector2 position = Vector2.Lerp(dustLineStart, dustLineEnd, i / (float)maxDustIterations);
                    Dust.NewDustAction(position, 0, 0, Main.zenithWorld ? DustID.GemTopaz : DustID.GemSapphire, Vector2.Zero, d =>
                    {
                        d.position = position;
                        d.velocity = spinningpoint.RotatedBy(MathHelper.TwoPi * i / maxDustIterations) * (0.9f + Main.rand.NextFloat() * 0.2f);
                        d.noGravity = true;
                        if (Main.rand.NextBool())
                        {
                            d.scale = 0.5f;
                            d.fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                        }
                        Dust d2 = Dust.CloneDust(d);
                        d2.scale *= 0.5f;
                        d2.fadeIn *= 0.5f;
                    });
                }
            }
        }

        void BuffedShoot_Emerald()
        {
            SoundEngine.PlaySound(SoundID.Item38, JewelEmerald.Center);
            for (int i = 0; i < 30; i++)
            {
                JewelHandler.SpawnParticle(JewelEmerald, Main.rand.NextFloat(4f, 8f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
                if (Main.rand.NextProbability(0.6f))
                    JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
            }
            if (!TOWorld.GeneralClient)
                return;

            int type = Main.zenithWorld ? ModContent.ProjectileType<JewelProjectile>() : ModContent.ProjectileType<KingSlimeJewelEmeraldClone>();
            Projectile.NewProjectileAction(JewelEmerald.GetSource_FromAI(), JewelEmerald.Center, JewelEmerald.GetVelocityTowards(JewelEmerald.PlayerTarget, 24f), type, JewelEmerald.GetProjectileDamage(type), 0f, Main.myPlayer, p =>
            {
                if (!Main.zenithWorld)
                    p.VelocityToRotation(MathHelper.PiOver2);
            });
        }

        void BuffedShoot_Ruby()
        {
            SoundEngine.PlaySound(SoundID.Item38, JewelRuby.Center);
            for (int i = 0; i < 30; i++)
            {
                JewelHandler.SpawnParticle(JewelRuby, Main.rand.NextFloat(4f, 8f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
                if (Main.rand.NextProbability(0.6f))
                    JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(3f, 6f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
            }
            if (!TOWorld.GeneralClient)
                return;

            int type = Main.zenithWorld ? ModContent.ProjectileType<KingSlimeJewelEmeraldClone>() : ModContent.ProjectileType<JewelProjectile>();
            int damage = JewelRuby.GetProjectileDamage(type);
            int amount1 = (HasEnteredPhase2 || JewelRubyPhase2) ? (Main.zenithWorld ? 3 : 5) : (Main.zenithWorld ? 4 : 8);
            int amount2 = (HasEnteredPhase2 || JewelRubyPhase2) ? (Main.zenithWorld ? 8 : 3) : (Main.zenithWorld ? 12 : 3);
            Projectile.RotatedProj(amount1, MathHelper.TwoPi / amount1, JewelRuby.GetSource_FromAI(), JewelRuby.Center, JewelRuby.GetVelocityTowards(JewelRuby.PlayerTarget, 18f), type, damage, 0f, Main.myPlayer, p => BuffedRubyProjectileAction(p, 1));
            Projectile.RotatedProj(amount2, MathHelper.TwoPi / amount2, JewelRuby.GetSource_FromAI(), JewelRuby.Center, JewelRuby.GetVelocityTowards(JewelRuby.PlayerTarget, 12f).RotatedByRandom(), type, damage, 0f, Main.myPlayer, p => BuffedRubyProjectileAction(p));

            void BuffedRubyProjectileAction(Projectile p, int type = 0)
            {
                if (HasEnteredPhase2 || JewelRubyPhase2)
                    p.velocity.Modulus *= 0.9f;
                if (Main.zenithWorld)
                {
                    p.velocity.Modulus *= Main.rand.NextFloat(1.2f, TOWorld.LegendaryMode ? 1.5f : 1.3f);
                    if (TOWorld.LegendaryMode)
                        p.velocity.Rotation += Main.rand.NextFloat(-0.2f, 0.2f);
                    p.VelocityToRotation(MathHelper.PiOver2);
                    p.timeLeft = (int)(p.timeLeft * (TOWorld.LegendaryMode ? 2.25f : 1.5f));
                    if (type > 0)
                    {
                        p.timeLeft *= 2;
                        p.GetModProjectile<KingSlimeJewelEmeraldClone>().ShouldChangeDirection = true;
                    }
                }
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float timeLeftGateValue = 45f;
        float gateValue = BuffedShootCooldownTime - timeLeftGateValue;
        float ratio = Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        if (CAClientConfig.Instance.AuxiliaryVisualEffects && ratio > 0f)
            JewelHandler.DrawAttackEffect(spriteBatch, screenPos, NPC, ratio, 150f, 0.4f);
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC, ratio);
        return false;
    }

    public override bool CheckDead()
    {
        if (CAWorld.AnomalyUltramundane)
        {
            NPC.life = 1;
            NPC.active = true;
            if (!HasEnteredPhase2)
                JewelHandler.EnterPhase2(NPC);
            return false;
        }
        return true;
    }
}

using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Events;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.Providence;
using Terraria;

namespace CalamityAnomalies.GlobalInstances;

public class CAGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    #region Data
    private const int AISlot = 32;
    private const int AISlot2 = 16;

    public DataUnion32[] AnomalyAI { get; } = new DataUnion32[AISlot];
    public DataUnion64[] AnomalyAI2 { get; } = new DataUnion64[AISlot2];

    public ref Bits32 AIChanged => ref AnomalyAI[^1].bits;
    public ref Bits64 AIChanged2 => ref AnomalyAI2[^1].bits;

    private DataUnion32[] InternalAnomalyAI { get; } = new DataUnion32[AISlot];
    private DataUnion64[] InternalAnomalyAI2 { get; } = new DataUnion64[AISlot2];

    private ref Bits32 InternalAIChanged => ref AnomalyAI[^1].bits;
    private ref Bits64 InternalAIChanged2 => ref AnomalyAI2[^1].bits;

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        CAGlobalNPC clone = (CAGlobalNPC)base.Clone(from, to);

        Array.Copy(AnomalyAI, clone.AnomalyAI, AISlot);
        Array.Copy(AnomalyAI2, clone.AnomalyAI2, AISlot2);
        Array.Copy(InternalAnomalyAI, clone.InternalAnomalyAI, AISlot);
        Array.Copy(InternalAnomalyAI, clone.InternalAnomalyAI, AISlot2);

        return clone;
    }
    #endregion Data

    #region 额外数据
    public bool NeverTrippy
    {
        get => InternalAnomalyAI[0].bits[0];
        set
        {
            if (InternalAnomalyAI[0].bits[0] != value)
            {
                InternalAnomalyAI[0].bits[0] = value;
                InternalAIChanged[0] = true;
            }
        }
    }

    public bool ShouldRunAnomalyAI
    {
        get => InternalAnomalyAI[0].bits[1];
        set
        {
            if (InternalAnomalyAI[0].bits[1] != value)
            {
                InternalAnomalyAI[0].bits[1] = value;
                InternalAIChanged[0] = true;
            }
        }
    }

    public int AnomalyKilltime
    {
        get => InternalAnomalyAI[1].i;
        private set
        {
            if (InternalAnomalyAI[1].i != value)
            {
                InternalAnomalyAI[1].i = value;
                InternalAIChanged[1] = true;
            }
        }
    }

    public int AnomalyAITimer
    {
        get => InternalAnomalyAI[2].i;
        private set
        {
            if (InternalAnomalyAI[2].i != value)
            {
                InternalAnomalyAI[2].i = value;
                InternalAIChanged[2] = true;
            }
        }
    }

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public int AnomalyUltraAITimer
    {
        get => InternalAnomalyAI[3].i;
        private set
        {
            if (InternalAnomalyAI[3].i != value)
            {
                InternalAnomalyAI[3].i = value;
                InternalAIChanged[3] = true;
            }
        }
    }

    public int AnomalyUltraBarTimer
    {
        get => InternalAnomalyAI[4].i;
        private set
        {
            if (InternalAnomalyAI[4].i != value)
            {
                InternalAnomalyAI[4].i = value;
                InternalAIChanged[4] = true;
            }
        }
    }

    public int BossRushAITimer
    {
        get => InternalAnomalyAI[5].i;
        private set
        {
            if (InternalAnomalyAI[5].i != value)
            {
                InternalAnomalyAI[5].i = value;
                InternalAIChanged[5] = true;
            }
        }
    }
    #endregion 额外数据

    #region Defaults
    public override void SetStaticDefaults()
    {
        foreach (CANPCOverride npcOverride in CAOverrideHelper.NPCOverrides.Values)
            npcOverride.SetStaticDefaults();
    }

    public override void SetDefaults(NPC npc)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.SetDefaults();
    }

    public override void SetDefaultsFromNetId(NPC npc)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.SetDefaultsFromNetId();
    }
    #endregion Defaults

    #region Active
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            npcOverride.OnSpawn(source);
        }
    }

    public override bool CheckActive(NPC npc)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.CheckActive())
                return false;
        }

        return true;
    }

    public override bool CheckDead(NPC npc)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.CheckDead())
                return false;
        }

        return true;
    }

    public override bool SpecialOnKill(NPC npc)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            if (npcOverride.SpecialOnKill())
                return true;
        }

        return false;
    }

    public override bool PreKill(NPC npc)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.PreKill())
                return false;
        }

        return true;
    }

    public override void OnKill(NPC npc)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            npcOverride.OnKill();
        }

        if (npc.ModNPC is EidolonWyrmHead && !DownedBossSystem.downedPrimordialWyrm)
            DownedBossSystem.downedPrimordialWyrm = true;

        foreach (Player player in Player.ActivePlayers)
        {
            player.Anomaly().DownedBossCalamity.BossesOnKill(npc);
        }
    }
    #endregion Active

    #region AI
    public override bool PreAI(NPC npc)
    {
        bool result = true;

        CalamityGlobalNPC calamityNPC = npc.Calamity();

        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            //禁用灾厄动态伤害减免。
            if (calamityNPC.KillTime >= 1 && calamityNPC.AITimer < calamityNPC.KillTime)
                calamityNPC.AITimer = calamityNPC.KillTime;

            // Disable netOffset effects.
            npc.netOffset = Vector2.Zero;

            // Disable the effects of certain unpredictable freeze debuffs.
            // Time Bolt and a few other weapon-specific debuffs are not counted here since those are more deliberate weapon mechanics.
            // That said, I don't know a single person who uses Time Bolt so it's probably irrelevant either way lol.
            npc.buffImmune[ModContent.BuffType<Eutrophication>()] = true;
            npc.buffImmune[ModContent.BuffType<GalvanicCorrosion>()] = true;
            npc.buffImmune[ModContent.BuffType<GlacialState>()] = true;
            npc.buffImmune[ModContent.BuffType<TemporalSadness>()] = true;
            npc.buffImmune[BuffID.Webbed] = true;

            if (CAWorld.Anomaly)
            {
                AnomalyAITimer++;
                if (CAWorld.AnomalyUltramundane)
                {
                    AnomalyUltraAITimer++;
                    AnomalyUltraBarTimer = Math.Clamp(AnomalyUltraBarTimer + 1, 0, 120);
                }
                else
                {
                    AnomalyUltraAITimer = 0;
                    AnomalyUltraBarTimer = Math.Clamp(AnomalyUltraBarTimer - 4, 0, 120);
                }
            }
            else
                AnomalyAITimer = 0;

            if (!npcOverride.PreAI())
                result = false;
        }

        if (CAWorld.BossRush)
            BossRushAITimer++;
        else
            BossRushAITimer = 0;

        return result;
    }

    public override void AI(NPC npc)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.AI();
    }

    public override void PostAI(NPC npc)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.PostAI();
    }
    #endregion AI

    #region Draw
    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.FindFrame(frameHeight);
    }

    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        if (Main.LocalPlayer.Calamity().trippy && !NeverTrippy)
            return TOMain.DiscoColor;

        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            Color? result = npcOverride.GetAlpha(drawColor);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.PreDraw(spriteBatch, screenPos, drawColor))
                return false;
        }

        return true;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.PostDraw(spriteBatch, screenPos, drawColor);
    }

    public override void DrawBehind(NPC npc, int index)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.DrawBehind(index);
    }

    public override void BossHeadSlot(NPC npc, ref int index)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.BossHeadSlot(ref index);
    }

    public override void BossHeadRotation(NPC npc, ref float rotation)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.BossHeadRotation(ref rotation);
    }

    public override void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.BossHeadSpriteEffects(ref spriteEffects);
    }
    #endregion Draw

    #region Hit
    public delegate float Orig_DRMath(CalamityGlobalNPC self, NPC npc, float DR);
    public static Orig_DRMath OrigMethod_CustomDRMath { get; private set; } = null;
    public static Orig_DRMath OrigMethod_DefaultDRMath { get; private set; } = null;

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.HitEffect(hit);
    }

    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            bool? result = npcOverride.CanBeHitByItem(player, item);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            bool? result = npcOverride.CanCollideWithPlayerMeleeAttack(player, item, meleeAttackHitbox);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);
        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();
        if (item.TryGetOverride(out CAItemOverride itemOverride, "ModifyHitNPC_DR"))
            itemOverride.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR));
        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR, 0f, 1f);

        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.ModifyHitByItem(player, item, ref modifiers);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.OnHitByItem(player, item, hit, damageDone);
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            bool? result = npcOverride.CanBeHitByProjectile(projectile);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);
        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride, "ModifyHitNPC_DR"))
            projectileOverride.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR));
        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR, 0f, 1f);

        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.ModifyHitByProjectile(projectile, ref modifiers);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.OnHitByProjectile(projectile, hit, damageDone);
    }

    public override bool CanBeHitByNPC(NPC npc, NPC attacker)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.CanBeHitByNPC(attacker))
                return false;
        }

        return true;
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.ModifyIncomingHit(ref modifiers);
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.CanHitPlayer(target, ref cooldownSlot))
                return false;
        }

        return true;
    }

    public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.OnHitPlayer(target, hurtInfo);
    }

    public override bool CanHitNPC(NPC npc, NPC target)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.CanHitNPC(target))
                return false;
        }

        return true;
    }

    public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
            npcOverride.OnHitNPC(target, hit);
    }

    public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
    {
        if (npc.TryGetOverride(out CANPCOverride npcOverride))
        {
            if (!npcOverride.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox))
                return false;
        }

        return true;
    }

    public static float GetBaseDR(NPC npc)
    {
        CalamityGlobalNPC calamityNPC = npc.Calamity();
        return calamityNPC.unbreakableDR ? calamityNPC.DR : (calamityNPC.customDR ? OrigMethod_CustomDRMath : OrigMethod_DefaultDRMath).Invoke(calamityNPC, npc, calamityNPC.DR);
    }

    public static float GetTimedDR(NPC npc, float baseDR)
    {
        if (CAWorld.BossRush || BossRushEvent.BossRushActive)
            return 0f;

        float timedDR = 0f;
        CalamityGlobalNPC calamityNPC = npc.Calamity();
        int killTime = calamityNPC.KillTime;
        int aiTimer = calamityNPC.AITimer;

        bool isNightProvidence = npc.ModNPC is Providence && !Main.IsItDay();
        bool isDayEmpress = npc.type == NPCID.HallowBoss && NPC.ShouldEmpressBeEnraged();

        if (killTime > 0 && aiTimer < killTime && !BossRushEvent.BossRushActive && (isNightProvidence || isDayEmpress))
        {
            const float tdrFactor = 10f;

            float extraDRLimit = (1f - baseDR) * tdrFactor / 2f;
            float lifeRatio = (float)npc.life / npc.lifeMax;
            float killTimeRatio = (float)aiTimer / killTime;
            float extraDRRatio = Math.Max(1f - lifeRatio - killTimeRatio, 0f);

            timedDR = extraDRLimit * extraDRRatio / (1 + extraDRRatio);
        }

        return timedDR;
    }
    #endregion Hit

    #region Net
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        Dictionary<int, float> aiToSend = [];
        for (int i = 0; i < AISlot - 1; i++)
        {
            if (AIChanged[i])
                aiToSend[i] = AnomalyAI[i].f;
        }
        binaryWriter.Write(aiToSend.Count);
        foreach ((int index, float value) in aiToSend)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged = default;

        Dictionary<int, double> aiToSend2 = [];
        for (int i = 0; i < AISlot2 - 1; i++)
        {
            if (AIChanged2[i])
                aiToSend2[i] = AnomalyAI2[i].d;
        }
        binaryWriter.Write(aiToSend2.Count);
        foreach ((int index, double value) in aiToSend2)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged2 = default;

        Dictionary<int, float> aiToSend3 = [];
        for (int i = 0; i < AISlot - 1; i++)
        {
            if (InternalAIChanged[i])
                aiToSend[i] = InternalAnomalyAI[i].f;
        }
        binaryWriter.Write(aiToSend.Count);
        foreach ((int index, float value) in aiToSend3)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        InternalAIChanged = default;

        Dictionary<int, double> aiToSend4 = [];
        for (int i = 0; i < AISlot2 - 1; i++)
        {
            if (AIChanged2[i])
                aiToSend2[i] = InternalAnomalyAI2[i].d;
        }
        binaryWriter.Write(aiToSend2.Count);
        foreach ((int index, double value) in aiToSend4)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        InternalAIChanged2 = default;
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        int recievedAICount = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount; i++)
            AnomalyAI[binaryReader.ReadInt32()].f = binaryReader.ReadSingle();

        int recievedAICount2 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount2; i++)
            AnomalyAI2[binaryReader.ReadInt32()].d = binaryReader.ReadDouble();

        int recievedAICount3 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount3; i++)
            InternalAnomalyAI[binaryReader.ReadInt32()].f = binaryReader.ReadSingle();

        int recievedAICount4 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount4; i++)
            InternalAnomalyAI2[binaryReader.ReadInt32()].d = binaryReader.ReadDouble();
    }
    #endregion Net

    public class Loader : ITOLoader
    {
        void ITOLoader.PostSetupContent()
        {
            Type type = typeof(CalamityGlobalNPC);
            OrigMethod_CustomDRMath = type.GetMethod("CustomDRMath", TOReflectionUtils.UniversalBindingFlags).CreateDelegate<Orig_DRMath>();
            OrigMethod_DefaultDRMath = type.GetMethod("DefaultDRMath", TOReflectionUtils.UniversalBindingFlags).CreateDelegate<Orig_DRMath>();
        }

        void ITOLoader.OnModUnload()
        {
            OrigMethod_DefaultDRMath = null;
            OrigMethod_CustomDRMath = null;
        }
    }
}

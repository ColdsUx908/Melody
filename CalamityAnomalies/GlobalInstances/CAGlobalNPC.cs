using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.NPCs.Abyss;

namespace CalamityAnomalies.GlobalInstances;

public class CAGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public int AnomalyKilltime { get; private set; } = 0;

    public bool ShouldRunAnomalyAI { get; set; } = true;

    public int AnomalyAITimer { get; private set; } = 0;

    public int AnomalyUltraAITimer { get; private set; } = 0;

    public int AnomalyUltraBarTimer { get; private set; } = 0;

    public bool IsRunningAnomalyAI => AnomalyAITimer > 0;

    public int BossRushAITimer { get; private set; } = 0;

    private const int MaxAISlots = 64;

    /// <summary>
    /// 额外的AI槽位，共64个。
    /// </summary>
    public float[] AnomalyAI { get; } = new float[MaxAISlots];

    public bool[] AIChanged { get; } = new bool[MaxAISlots];

    public void SetAnomalyAI(float value, int index)
    {
        AnomalyAI[index] = value;
        AIChanged[index] = true;
    }

    public void SetAnomalyAI(float value, Index index)
    {
        AnomalyAI[index] = value;
        AIChanged[index] = true;
    }

    public bool GetAnomalyAIBit(int index, byte bitPosition) => BitOperation.GetBit((int)AnomalyAI[index], bitPosition);

    public bool GetAnomalyAIBit(Index index, byte bitPosition) => BitOperation.GetBit((int)AnomalyAI[index], bitPosition);

    public void SetAnomalyAIBit(bool value, int index, byte bitPosition) => SetAnomalyAI(BitOperation.SetBit((int)AnomalyAI[index], bitPosition, value), index);

    public void SetAnomalyAIBit(bool value, Index index, byte bitPosition) => SetAnomalyAI(BitOperation.SetBit((int)AnomalyAI[index], bitPosition, value), index);

    public bool NeverTrippy { get; set; } = false;

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

            if (!npcOverride.PreAI())
                return false;
        }

        AnomalyAITimer = 0;

        if (CAWorld.BossRush)
            BossRushAITimer++;
        else
            BossRushAITimer = 0;

        return true;
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
    #endregion Hit

    #region Net
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.SendAI(AnomalyAI, AIChanged, binaryWriter);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        TONetUtils.ReceiveAI(AnomalyAI, binaryReader);
    }
    #endregion Net
}

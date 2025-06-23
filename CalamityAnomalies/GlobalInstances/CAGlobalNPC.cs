using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Events;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.Providence;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;

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
        foreach (CANPCBehavior npcBehavior in CABehaviorHelper.NPCBehaviors)
            npcBehavior.SetStaticDefaults();
    }

    public override void SetDefaults(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.SetDefaults();
    }

    public override void SetDefaultsFromNetId(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.SetDefaultsFromNetId();
    }

    public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ApplyDifficultyAndPlayerScaling(numPlayers, balance, bossAdjustment);
    }

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.SetBestiary(database, bestiaryEntry);
    }

    public override void ModifyTypeName(NPC npc, ref string typeName)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ModifyTypeName(ref typeName);
    }

    public override void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ModifyHoverBoundingBox(ref boundingBox);
    }

    public override void ResetEffects(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ResetEffects();
    }
    #endregion Defaults

    #region Lifetime
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.OnSpawn(source);
    }

    public override bool CheckActive(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (!npcBehavior.CheckActive())
                return false;
        }

        return true;
    }

    public override bool CheckDead(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (!npcBehavior.CheckDead())
                return false;
        }

        return true;
    }

    public override bool SpecialOnKill(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (npcBehavior.SpecialOnKill())
                return true;
        }

        return false;
    }

    public override bool PreKill(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreKill())
                return false;
        }

        return true;
    }

    public override void OnKill(NPC npc)
    {
        if (npc.ModNPC is EidolonWyrmHead && !DownedBossSystem.downedPrimordialWyrm)
            DownedBossSystem.downedPrimordialWyrm = true;

        foreach (Player player in Player.ActivePlayers)
        {
            player.Anomaly().DownedBossCalamity.BossesOnKill(npc);
        }

        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.OnKill();
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ModifyNPCLoot(npcLoot);
    }
    #endregion Lifetime

    #region AI
    public override bool PreAI(NPC npc)
    {
        bool result = true;

        CalamityGlobalNPC calamityNPC = npc.Calamity();

        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
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

            if (!npcBehavior.PreAI())
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
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.AI();
    }

    public override void PostAI(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.PostAI();
    }
    #endregion AI

    #region Draw
    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.FindFrame(frameHeight);
    }

    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        if (Main.LocalPlayer.Calamity().trippy && !NeverTrippy)
            return TOMain.DiscoColor;

        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            Color? result = npcBehavior.GetAlpha(drawColor);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.DrawEffects(ref drawColor);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreDraw(spriteBatch, screenPos, drawColor))
                return false;
        }

        return true;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.PostDraw(spriteBatch, screenPos, drawColor);
    }

    public override void DrawBehind(NPC npc, int index)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.DrawBehind(index);
    }

    public override void BossHeadSlot(NPC npc, ref int index)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.BossHeadSlot(ref index);
    }

    public override void BossHeadRotation(NPC npc, ref float rotation)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.BossHeadRotation(ref rotation);
    }

    public override void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.BossHeadSpriteEffects(ref spriteEffects);
    }

    public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.DrawHealthBar(hbPosition, ref scale, ref position);
            if (result is not null)
                return result;
        }

        return null;
    }
    #endregion Draw

    #region Hit
    public delegate float Orig_DRMath(CalamityGlobalNPC self, NPC npc, float DR);
    public static Orig_DRMath OrigMethod_CustomDRMath { get; private set; } = null;
    public static Orig_DRMath OrigMethod_DefaultDRMath { get; private set; } = null;

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.HitEffect(hit);
    }

    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanBeHitByItem(player, item);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanCollideWithPlayerMeleeAttack(player, item, meleeAttackHitbox);
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
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior, "ModifyHitNPC_DR"))
            itemBehavior.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR));
        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR, 0f, 1f);

        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ModifyHitByItem(player, item, ref modifiers);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.OnHitByItem(player, item, hit, damageDone);
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanBeHitByProjectile(projectile);
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
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior, "ModifyHitNPC_DR"))
            projectileBehavior.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR));
        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR, 0f, 1f);

        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ModifyHitByProjectile(projectile, ref modifiers);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.OnHitByProjectile(projectile, hit, damageDone);
    }

    public override bool CanBeHitByNPC(NPC npc, NPC attacker)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (!npcBehavior.CanBeHitByNPC(attacker))
                return false;
        }

        return true;
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ModifyIncomingHit(ref modifiers);
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (!npcBehavior.CanHitPlayer(target, ref cooldownSlot))
                return false;
        }

        return true;
    }

    public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.OnHitPlayer(target, hurtInfo);
    }

    public override bool CanHitNPC(NPC npc, NPC target)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (!npcBehavior.CanHitNPC(target))
                return false;
        }

        return true;
    }

    public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.OnHitNPC(target, hit);
    }

    public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (!npcBehavior.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox))
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

    #region SpecialEffects
    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.UpdateLifeRegen(ref damage);
    }

    public override bool? CanFallThroughPlatforms(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanFallThroughPlatforms();
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool? CanBeCaughtBy(NPC npc, Item item, Player player)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanBeCaughtBy(item, player);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void OnCaughtBy(NPC npc, Player player, Item item, bool failed)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.OnCaughtBy(player, item, failed);
    }

    public override bool? CanChat(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanChat();
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void GetChat(NPC npc, ref string chat)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.GetChat(ref chat);
    }

    public override bool PreChatButtonClicked(NPC npc, bool firstButton)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (!npcBehavior.PreChatButtonClicked(firstButton))
                return false;
        }

        return true;
    }

    public override void OnChatButtonClicked(NPC npc, bool firstButton)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.OnChatButtonClicked(firstButton);
    }

    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ModifyActiveShop(shopName, items);
    }

    public override bool? CanGoToStatue(NPC npc, bool toKingStatue)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            bool? result = npcBehavior.CanGoToStatue(toKingStatue);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void OnGoToStatue(NPC npc, bool toKingStatue)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.OnGoToStatue(toKingStatue);
    }

    public override void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackStrength(ref damage, ref knockback);
    }

    public override void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackCooldown(ref cooldown, ref randExtraCooldown);
    }

    public override void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackProj(ref projType, ref attackDelay);
    }

    public override void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackProjSpeed(ref multiplier, ref gravityCorrection, ref randomOffset);
    }

    public override void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackShoot(ref inBetweenShots);
    }

    public override void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackMagic(ref auraLightMultiplier);
    }

    public override void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.TownNPCAttackSwing(ref itemWidth, ref itemHeight);
    }

    public override void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.DrawTownAttackGun(ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);
    }

    public override void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.DrawTownAttackSwing(ref item, ref itemFrame, ref itemSize, ref scale, ref offset);
    }

    public override bool NeedSaving(NPC npc)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            if (npcBehavior.NeedSaving())
                return true;
        }

        return false;
    }

    public override void SaveData(NPC npc, TagCompound tag)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.SaveData(tag);
    }

    public override void LoadData(NPC npc, TagCompound tag)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.LoadData(tag);
    }

    public override int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
        {
            int? result = npcBehavior.PickEmote(closestPlayer, emoteList, otherAnchor);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.ChatBubblePosition(ref position, ref spriteEffects);
    }

    public override void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.PartyHatPosition(ref position, ref spriteEffects);
    }

    public override void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        if (npc.TryGetBehavior(out CANPCBehavior npcBehavior))
            npcBehavior.EmoteBubblePosition(ref position, ref spriteEffects);
    }
    #endregion SpecialEffects

    #region NotOverriden
    public override void ModifyGlobalLoot(GlobalLoot globalLoot) { }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) { }

    public override void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY) { }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) { }

    public override void SpawnNPC(int npc, int tileX, int tileY) { }

    public override void ModifyShop(NPCShop shop) { }

    public override void SetupTravelShop(int[] shop, ref int nextSlot) { }

    public override void BuffTownNPC(ref float damageMult, ref int defense) { }
    #endregion NotOverriden

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

    public class Loader : IResourceLoader
    {
        void IResourceLoader.PostSetupContent()
        {
            Type type = typeof(CalamityGlobalNPC);
            OrigMethod_CustomDRMath = type.GetMethod("CustomDRMath", TOReflectionUtils.UniversalBindingFlags).CreateDelegate<Orig_DRMath>();
            OrigMethod_DefaultDRMath = type.GetMethod("DefaultDRMath", TOReflectionUtils.UniversalBindingFlags).CreateDelegate<Orig_DRMath>();
        }

        void IResourceLoader.OnModUnload()
        {
            OrigMethod_DefaultDRMath = null;
            OrigMethod_CustomDRMath = null;
        }
    }
}

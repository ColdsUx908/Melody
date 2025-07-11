using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Events;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.Providence;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;

namespace CalamityAnomalies.GlobalInstances;

public sealed class CAGlobalNPC : GlobalNPCWithBehavior<CANPCBehavior>, IResourceLoader
{
    protected override SingleEntityBehaviorSet<NPC, CANPCBehavior> BehaviorSet => CABehaviorHelper.NPCBehaviors;

    #region Data
    private const int AISlot = 33;
    private const int AISlot2 = 17;

    public Union32[] AnomalyAI { get; } = new Union32[AISlot];
    public Union64[] AnomalyAI2 { get; } = new Union64[AISlot2];

    public ref Bits32 AIChanged => ref AnomalyAI[^1].bits;
    public ref Bits64 AIChanged2 => ref AnomalyAI2[^1].bits;

    private Union32[] InternalAnomalyAI { get; } = new Union32[AISlot];
    private Union64[] InternalAnomalyAI2 { get; } = new Union64[AISlot2];

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
    #endregion Data

    #region Defaults
    public override void SetStaticDefaults() => base.SetStaticDefaults();

    public override void SetDefaults(NPC npc) => base.SetDefaults(npc);

    public override void SetDefaultsFromNetId(NPC npc) => base.SetDefaultsFromNetId(npc);

    public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment) => base.ApplyDifficultyAndPlayerScaling(npc, numPlayers, balance, bossAdjustment);

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) => base.SetBestiary(npc, database, bestiaryEntry);

    public override void ModifyTypeName(NPC npc, ref string typeName) => base.ModifyTypeName(npc, ref typeName);

    public override void ModifyHoverBoundingBox(NPC npc, ref Rectangle boundingBox) => base.ModifyHoverBoundingBox(npc, ref boundingBox);

    public override ITownNPCProfile ModifyTownNPCProfile(NPC npc) => base.ModifyTownNPCProfile(npc);

    public override void ModifyNPCNameList(NPC npc, List<string> nameList) => base.ModifyNPCNameList(npc, nameList);

    public override void ResetEffects(NPC npc) => base.ResetEffects(npc);
    #endregion Defaults

    #region Lifetime
    public override void OnSpawn(NPC npc, IEntitySource source) => base.OnSpawn(npc, source);

    public override bool CheckActive(NPC npc) => base.CheckActive(npc);

    public override bool CheckDead(NPC npc) => base.CheckDead(npc);

    public override bool SpecialOnKill(NPC npc) => base.SpecialOnKill(npc);

    public override bool PreKill(NPC npc) => base.PreKill(npc);

    public override void OnKill(NPC npc)
    {
        if (npc.ModNPC is EidolonWyrmHead && !DownedBossSystem.downedPrimordialWyrm)
            DownedBossSystem.downedPrimordialWyrm = true;

        foreach (Player player in Player.ActivePlayers)
        {
            player.Anomaly().DownedBossCalamity.BossesOnKill(npc);
        }

        base.OnKill(npc);
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) => base.ModifyNPCLoot(npc, npcLoot);
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

    public override void AI(NPC npc) => base.AI(npc);

    public override void PostAI(NPC npc) => base.PostAI(npc);
    #endregion AI

    #region Draw
    public override void FindFrame(NPC npc, int frameHeight) => base.FindFrame(npc, frameHeight);

    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        if (Main.LocalPlayer.Calamity().trippy && !NeverTrippy)
            return TOMain.DiscoColor;

        return base.GetAlpha(npc, drawColor);
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) => base.DrawEffects(npc, ref drawColor);

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => base.PreDraw(npc, spriteBatch, screenPos, drawColor);

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => base.PostDraw(npc, spriteBatch, screenPos, drawColor);

    public override void DrawBehind(NPC npc, int index) => base.DrawBehind(npc, index);

    public override void BossHeadSlot(NPC npc, ref int index) => base.BossHeadSlot(npc, ref index);

    public override void BossHeadRotation(NPC npc, ref float rotation) => base.BossHeadRotation(npc, ref rotation);

    public override void BossHeadSpriteEffects(NPC npc, ref SpriteEffects spriteEffects) => base.BossHeadSpriteEffects(npc, ref spriteEffects);

    public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position) => base.DrawHealthBar(npc, hbPosition, ref scale, ref position);
    #endregion Draw

    #region Hit
    public delegate float Orig_DRMath(CalamityGlobalNPC self, NPC npc, float DR);
    public static Orig_DRMath OrigMethod_CustomDRMath { get; private set; } = null;
    public static Orig_DRMath OrigMethod_DefaultDRMath { get; private set; } = null;

    public override void HitEffect(NPC npc, NPC.HitInfo hit) => base.HitEffect(npc, hit);

    public override bool? CanBeHitByItem(NPC npc, Player player, Item item) => base.CanBeHitByItem(npc, player, item);

    public override bool? CanCollideWithPlayerMeleeAttack(NPC npc, Player player, Item item, Rectangle meleeAttackHitbox) => base.CanCollideWithPlayerMeleeAttack(npc, player, item, meleeAttackHitbox);

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);
        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();
        if (item.TryGetBehavior(out CAItemBehavior itemBehavior, nameof(CAItemBehavior.ModifyHitNPC_DR)))
            itemBehavior.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR));
        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR, 0f, 1f);

        base.ModifyHitByItem(npc, player, item, ref modifiers);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) => base.OnHitByItem(npc, player, item, hit, damageDone);

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile) => base.CanBeHitByProjectile(npc, projectile);

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);
        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior, nameof(CAProjectileBehavior.ModifyHitNPC_DR)))
            projectileBehavior.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR));
        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR, 0f, 1f);

        base.ModifyHitByProjectile(npc, projectile, ref modifiers);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) => base.OnHitByProjectile(npc, projectile, hit, damageDone);

    public override bool CanBeHitByNPC(NPC npc, NPC attacker) => base.CanBeHitByNPC(npc, attacker);

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) => base.ModifyIncomingHit(npc, ref modifiers);

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => base.CanHitPlayer(npc, target, ref cooldownSlot);

    public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers) => base.ModifyHitPlayer(npc, target, ref modifiers);

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) => base.OnHitPlayer(npc, target, hurtInfo);

    public override bool CanHitNPC(NPC npc, NPC target) => base.CanHitNPC(npc, target);

    public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers) => base.ModifyHitNPC(npc, target, ref modifiers);

    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit) => base.OnHitNPC(npc, target, hit);

    public override bool ModifyCollisionData(NPC npc, Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) => base.ModifyCollisionData(npc, victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);

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
    public override void UpdateLifeRegen(NPC npc, ref int damage) => base.UpdateLifeRegen(npc, ref damage);

    public override bool? CanFallThroughPlatforms(NPC npc) => base.CanFallThroughPlatforms(npc);

    public override bool? CanBeCaughtBy(NPC npc, Item item, Player player) => base.CanBeCaughtBy(npc, item, player);

    public override void OnCaughtBy(NPC npc, Player player, Item item, bool failed) => base.OnCaughtBy(npc, player, item, failed);

    public override bool? CanChat(NPC npc) => base.CanChat(npc);

    public override void GetChat(NPC npc, ref string chat) => base.GetChat(npc, ref chat);

    public override bool PreChatButtonClicked(NPC npc, bool firstButton) => base.PreChatButtonClicked(npc, firstButton);

    public override void OnChatButtonClicked(NPC npc, bool firstButton) => base.OnChatButtonClicked(npc, firstButton);

    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items) => base.ModifyActiveShop(npc, shopName, items);

    public override bool? CanGoToStatue(NPC npc, bool toKingStatue) => base.CanGoToStatue(npc, toKingStatue);

    public override void OnGoToStatue(NPC npc, bool toKingStatue) => base.OnGoToStatue(npc, toKingStatue);

    public override bool ModifyDeathMessage(NPC npc, ref NetworkText customText, ref Color color) => base.ModifyDeathMessage(npc, ref customText, ref color);

    public override void TownNPCAttackStrength(NPC npc, ref int damage, ref float knockback) => base.TownNPCAttackStrength(npc, ref damage, ref knockback);

    public override void TownNPCAttackCooldown(NPC npc, ref int cooldown, ref int randExtraCooldown) => base.TownNPCAttackCooldown(npc, ref cooldown, ref randExtraCooldown);

    public override void TownNPCAttackProj(NPC npc, ref int projType, ref int attackDelay) => base.TownNPCAttackProj(npc, ref projType, ref attackDelay);

    public override void TownNPCAttackProjSpeed(NPC npc, ref float multiplier, ref float gravityCorrection, ref float randomOffset) => base.TownNPCAttackProjSpeed(npc, ref multiplier, ref gravityCorrection, ref randomOffset);

    public override void TownNPCAttackShoot(NPC npc, ref bool inBetweenShots) => base.TownNPCAttackShoot(npc, ref inBetweenShots);

    public override void TownNPCAttackMagic(NPC npc, ref float auraLightMultiplier) => base.TownNPCAttackMagic(npc, ref auraLightMultiplier);

    public override void TownNPCAttackSwing(NPC npc, ref int itemWidth, ref int itemHeight) => base.TownNPCAttackSwing(npc, ref itemWidth, ref itemHeight);

    public override void DrawTownAttackGun(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) => base.DrawTownAttackGun(npc, ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);

    public override void DrawTownAttackSwing(NPC npc, ref Texture2D item, ref Rectangle itemFrame, ref int itemSize, ref float scale, ref Vector2 offset) => base.DrawTownAttackSwing(npc, ref item, ref itemFrame, ref itemSize, ref scale, ref offset);

    public override int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) => base.PickEmote(npc, closestPlayer, emoteList, otherAnchor);

    public override void ChatBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) => base.ChatBubblePosition(npc, ref position, ref spriteEffects);

    public override void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) => base.PartyHatPosition(npc, ref position, ref spriteEffects);

    public override void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) => base.EmoteBubblePosition(npc, ref position, ref spriteEffects);
    #endregion SpecialEffects

    #region WorldSaving
    public override bool NeedSaving(NPC npc) => base.NeedSaving(npc);

    public override void SaveData(NPC npc, TagCompound tag) => base.SaveData(npc, tag);

    public override void LoadData(NPC npc, TagCompound tag) => base.LoadData(npc, tag);
    #endregion WorldSaving

    #region NotSingle
    public override void ModifyGlobalLoot(GlobalLoot globalLoot) { }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) { }

    public override void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY) { }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) { }

    public override void SpawnNPC(int npc, int tileX, int tileY) { }

    public override void ModifyShop(NPCShop shop) { }

    public override void SetupTravelShop(int[] shop, ref int nextSlot) { }

    public override void BuffTownNPC(ref float damageMult, ref int defense) { }
    #endregion NotSingle

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

using CalamityAnomalies.Tweaks._5_2_PostYharon;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.GreatSandShark;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using Terraria.GameInput;

namespace CalamityAnomalies.GlobalInstances;

public sealed class CAPlayer : ModPlayerWithBehavior<CAPlayerBehavior>
{
    public override GlobalEntityBehaviorSet<Player, CAPlayerBehavior> BehaviorSet => CABehaviorHelper.PlayerBehaviors;

    public bool AntiEPBPlayer { get; set; } = false;

    public PlayerDownedBossCalamity DownedBossCalamity { get; private set; } = new();
    public PlayerDownedBossCalamity DownedBossAnomaly { get; private set; } = new();

    /// <summary>
    /// 提升玩家翅膀飞行时间的乘区。
    /// <br/>每个索引独立计算。
    /// </summary>
    public AddableFloat[] WingTimeMaxMultipliers { get; } = new AddableFloat[3];

    public int YharimsGift { get; set; } = 0;

    public YharimsGift_Tweak.CurrentBuff YharimsGiftBuff { get; set; } = YharimsGift_Tweak.CurrentBuff.None;

    public override ModPlayer Clone(Player newEntity)
    {
        CAPlayer clone = (CAPlayer)base.Clone(newEntity);

        clone.AntiEPBPlayer = AntiEPBPlayer;
        clone.DownedBossCalamity = DownedBossCalamity;
        clone.DownedBossAnomaly = DownedBossAnomaly;
        Array.Copy(WingTimeMaxMultipliers, clone.WingTimeMaxMultipliers, WingTimeMaxMultipliers.Length);
        clone.YharimsGift = YharimsGift;
        clone.YharimsGiftBuff = YharimsGiftBuff;

        return clone;
    }

    public override void SetStaticDefaults() => base.SetStaticDefaults();

    public override void Initialize() => base.Initialize();

    public override void ResetEffects()
    {
        AntiEPBPlayer = false;
        for (int i = 0; i < WingTimeMaxMultipliers.Length; i++)
            WingTimeMaxMultipliers[i] = AddableFloat.Zero;
        if (YharimsGift > 0)
            YharimsGift--;

        base.ResetEffects();
    }

    public override void ResetInfoAccessories() => base.ResetInfoAccessories();

    public override void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer) => base.RefreshInfoAccessoriesFromTeamPlayers(otherPlayer);

    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) => base.ModifyMaxStats(out health, out mana);

    public override void UpdateDead() => base.UpdateDead();

    public override void PreSaveCustomData() => base.PreSaveCustomData();

    public override void SaveData(TagCompound tag)
    {
        DownedBossCalamity.SaveData(tag, "PlayerDownedBossCalamity");
        DownedBossAnomaly.SaveData(tag, "PlayerDownedBossAnomaly");
        tag["YharimsGiftBuff"] = (int)YharimsGiftBuff;

        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        DownedBossCalamity.LoadData(tag, "PlayerDownedBossCalamity");
        DownedBossAnomaly.LoadData(tag, "PlayerDownedBossAnomaly");
        if (tag.TryGet("YharimsGiftBuff", out int yharimsGiftBuff))
            YharimsGiftBuff = (YharimsGift_Tweak.CurrentBuff)yharimsGiftBuff;
        else
            YharimsGiftBuff = YharimsGift_Tweak.CurrentBuff.None;

        base.LoadData(tag);
    }

    public override void PreSavePlayer() => base.PreSavePlayer();

    public override void PostSavePlayer() => base.PostSavePlayer();

    public override void CopyClientState(ModPlayer targetCopy) => base.CopyClientState(targetCopy);

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) => base.SyncPlayer(toWho, fromWho, newPlayer);

    public override void SendClientChanges(ModPlayer clientPlayer) => base.SendClientChanges(clientPlayer);

    public override void UpdateBadLifeRegen() => base.UpdateBadLifeRegen();

    public override void UpdateLifeRegen() => base.UpdateLifeRegen();

    public override void NaturalLifeRegen(ref float regen) => base.NaturalLifeRegen(ref regen);

    public override void UpdateAutopause() => base.UpdateAutopause();

    public override void PreUpdate() => base.PreUpdate();

    public override void ProcessTriggers(TriggersSet triggersSet) => base.ProcessTriggers(triggersSet);

    public override void ArmorSetBonusActivated() => base.ArmorSetBonusActivated();

    public override void ArmorSetBonusHeld(int holdTime) => base.ArmorSetBonusHeld(holdTime);

    public override void SetControls() => base.SetControls();

    public override void PreUpdateBuffs() => base.PreUpdateBuffs();

    public override void PostUpdateBuffs() => base.PostUpdateBuffs();

    public override void UpdateEquips() => base.UpdateEquips();

    public override void PostUpdateEquips() => base.PostUpdateEquips();

    public override void UpdateVisibleAccessories() => base.UpdateVisibleAccessories();

    public override void UpdateVisibleVanityAccessories() => base.UpdateVisibleVanityAccessories();

    public override void UpdateDyes() => base.UpdateDyes();

    public override void PostUpdateMiscEffects()
    {
        base.PostUpdateEquips();

        if (Player.wingTimeMax > 0)
        {
            float multiplier = 1f;
            foreach (AddableFloat wingTimeMaxMultiplier in WingTimeMaxMultipliers)
                multiplier *= (1f + wingTimeMaxMultiplier.Value);
            Player.wingTimeMax = (int)(Player.wingTimeMax * multiplier);
        }
    }

    public override void PostUpdateRunSpeeds() => base.PostUpdateRunSpeeds();

    public override void PreUpdateMovement() => base.PreUpdateMovement();

    public override void PostUpdate() => base.PostUpdate();

    public override void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration) => base.ModifyExtraJumpDurationMultiplier(jump, ref duration);

    public override bool CanStartExtraJump(ExtraJump jump) => base.CanStartExtraJump(jump);

    public override void OnExtraJumpStarted(ExtraJump jump, ref bool playSound) => base.OnExtraJumpStarted(jump, ref playSound);

    public override void OnExtraJumpEnded(ExtraJump jump) => base.OnExtraJumpEnded(jump);

    public override void OnExtraJumpRefreshed(ExtraJump jump) => base.OnExtraJumpRefreshed(jump);

    public override void ExtraJumpVisuals(ExtraJump jump) => base.ExtraJumpVisuals(jump);

    public override bool CanShowExtraJumpVisuals(ExtraJump jump) => base.CanShowExtraJumpVisuals(jump);

    public override void OnExtraJumpCleared(ExtraJump jump) => base.OnExtraJumpCleared(jump);

    public override void FrameEffects() => base.FrameEffects();

    public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => base.ImmuneTo(damageSource, cooldownCounter, dodgeable);

    public override bool FreeDodge(Player.HurtInfo info) => base.FreeDodge(info);

    public override bool ConsumableDodge(Player.HurtInfo info) => base.ConsumableDodge(info);

    public override void ModifyHurt(ref Player.HurtModifiers modifiers) => base.ModifyHurt(ref modifiers);

    public override void OnHurt(Player.HurtInfo info) => base.OnHurt(info);

    public override void PostHurt(Player.HurtInfo info) => base.PostHurt(info);

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource) => base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) => base.Kill(damage, hitDirection, pvp, damageSource);

    public override bool PreModifyLuck(ref float luck) => base.PreModifyLuck(ref luck);

    public override void ModifyLuck(ref float luck) => base.ModifyLuck(ref luck);

    public override bool PreItemCheck() => base.PreItemCheck();

    public override void PostItemCheck() => base.PostItemCheck();

    public override float UseTimeMultiplier(Item item) => base.UseTimeMultiplier(item);

    public override float UseAnimationMultiplier(Item item) => base.UseAnimationMultiplier(item);

    public override float UseSpeedMultiplier(Item item) => base.UseSpeedMultiplier(item);

    public override void GetHealLife(Item item, bool quickHeal, ref int healValue) => base.GetHealLife(item, quickHeal, ref healValue);

    public override void GetHealMana(Item item, bool quickHeal, ref int healValue) => base.GetHealMana(item, quickHeal, ref healValue);

    public override void ModifyManaCost(Item item, ref float reduce, ref float mult) => base.ModifyManaCost(item, ref reduce, ref mult);

    public override void OnMissingMana(Item item, int neededMana) => base.OnMissingMana(item, neededMana);

    public override void OnConsumeMana(Item item, int manaConsumed) => base.OnConsumeMana(item, manaConsumed);

    public override void ModifyWeaponDamage(Item item, ref StatModifier damage) => base.ModifyWeaponDamage(item, ref damage);

    public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback) => base.ModifyWeaponKnockback(item, ref knockback);

    public override void ModifyWeaponCrit(Item item, ref float crit) => base.ModifyWeaponCrit(item, ref crit);

    public override bool CanConsumeAmmo(Item weapon, Item ammo) => base.CanConsumeAmmo(weapon, ammo);

    public override void OnConsumeAmmo(Item weapon, Item ammo) => base.OnConsumeAmmo(weapon, ammo);

    public override bool CanShoot(Item item) => base.CanShoot(item);

    public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => base.ModifyShootStats(item, ref position, ref velocity, ref type, ref damage, ref knockback);

    public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => base.Shoot(item, source, position, velocity, type, damage, knockback);

    public override void MeleeEffects(Item item, Rectangle hitbox) => base.MeleeEffects(item, hitbox);

    public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) => base.EmitEnchantmentVisualsAt(projectile, boxPosition, boxWidth, boxHeight);

    public override bool? CanCatchNPC(NPC target, Item item) => base.CanCatchNPC(target, item);

    public override void OnCatchNPC(NPC npc, Item item, bool failed) => base.OnCatchNPC(npc, item, failed);

    public override void ModifyItemScale(Item item, ref float scale) => base.ModifyItemScale(item, ref scale);

    public override void OnHitAnything(float x, float y, Entity victim) => base.OnHitAnything(x, y, victim);

    public override bool CanHitNPC(NPC target) => base.CanHitNPC(target);

    public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, NPC target) => base.CanMeleeAttackCollideWithNPC(item, meleeAttackHitbox, target);

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => base.ModifyHitNPC(target, ref modifiers);

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => base.OnHitNPC(target, hit, damageDone);

    public override bool? CanHitNPCWithItem(Item item, NPC target) => base.CanHitNPCWithItem(item, target);

    public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) => base.ModifyHitNPCWithItem(item, target, ref modifiers);

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) => base.OnHitNPCWithItem(item, target, hit, damageDone);

    public override bool? CanHitNPCWithProj(Projectile proj, NPC target) => base.CanHitNPCWithProj(proj, target);

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) => base.ModifyHitNPCWithProj(proj, target, ref modifiers);

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) => base.OnHitNPCWithProj(proj, target, hit, damageDone);

    public override bool CanHitPvp(Item item, Player target) => base.CanHitPvp(item, target);

    public override bool CanHitPvpWithProj(Projectile proj, Player target) => base.CanHitPvpWithProj(proj, target);

    public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) => base.CanBeHitByNPC(npc, ref cooldownSlot);

    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers) => base.ModifyHitByNPC(npc, ref modifiers);

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) => base.OnHitByNPC(npc, hurtInfo);

    public override bool CanBeHitByProjectile(Projectile proj) => base.CanBeHitByProjectile(proj);

    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) => base.ModifyHitByProjectile(proj, ref modifiers);

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) => base.OnHitByProjectile(proj, hurtInfo);

    public override void ModifyFishingAttempt(ref FishingAttempt attempt) => base.ModifyFishingAttempt(ref attempt);

    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) => base.CatchFish(attempt, ref itemDrop, ref npcSpawn, ref sonar, ref sonarPosition);

    public override void ModifyCaughtFish(Item fish) => base.ModifyCaughtFish(fish);

    public override bool? CanConsumeBait(Item bait) => base.CanConsumeBait(bait);

    public override void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel) => base.GetFishingLevel(fishingRod, bait, ref fishingLevel);

    public override void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems) => base.AnglerQuestReward(rareMultiplier, rewardItems);

    public override void GetDyeTraderReward(List<int> rewardPool) => base.GetDyeTraderReward(rewardPool);

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) => base.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) => base.ModifyDrawInfo(ref drawInfo);

    public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions) => base.ModifyDrawLayerOrdering(positions);

    public override void HideDrawLayers(PlayerDrawSet drawInfo) => base.HideDrawLayers(drawInfo);

    public override void ModifyScreenPosition() => base.ModifyScreenPosition();

    public override void ModifyZoom(ref float zoom) => base.ModifyZoom(ref zoom);

    public override void PlayerConnect() => base.PlayerConnect();

    public override void PlayerDisconnect() => base.PlayerDisconnect();

    public override void OnEnterWorld() => base.OnEnterWorld();

    public override void OnRespawn() => base.OnRespawn();

    public override bool ShiftClickSlot(Item[] inventory, int context, int slot) => base.ShiftClickSlot(inventory, context, slot);

    public override bool HoverSlot(Item[] inventory, int context, int slot) => base.HoverSlot(inventory, context, slot);

    public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item) => base.PostSellItem(vendor, shopInventory, item);

    public override bool CanSellItem(NPC vendor, Item[] shopInventory, Item item) => base.CanSellItem(vendor, shopInventory, item);

    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) => base.PostBuyItem(vendor, shopInventory, item);

    public override bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item) => base.CanBuyItem(vendor, shopInventory, item);

    public override bool CanUseItem(Item item) => base.CanUseItem(item);

    public override bool? CanAutoReuseItem(Item item) => base.CanAutoReuseItem(item);

    public override bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText) => base.ModifyNurseHeal(nurse, ref health, ref removeDebuffs, ref chatText);

    public override void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price) => base.ModifyNursePrice(nurse, health, removeDebuffs, ref price);

    public override void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price) => base.PostNurseHeal(nurse, health, removeDebuffs, price);

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) => base.AddStartingItems(mediumCoreDeath);

    public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath) => base.ModifyStartingInventory(itemsByMod, mediumCoreDeath);

    public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback) => base.AddMaterialsForCrafting(out itemConsumedCallback);

    public override bool OnPickup(Item item) => base.OnPickup(item);

    public override bool CanBeTeleportedTo(Vector2 teleportPosition, string context) => base.CanBeTeleportedTo(teleportPosition, context);

    public override void OnEquipmentLoadoutSwitched(int oldLoadoutIndex, int loadoutIndex) => base.OnEquipmentLoadoutSwitched(oldLoadoutIndex, loadoutIndex);
}

public class PlayerDownedBossCalamity : PlayerDownedBoss
{
    public bool DesertScourge { get; set; } = false;
    public bool Crabulon { get; set; } = false;
    public bool EvilBoss2 { get; set; } = false;
    public bool HiveMind { get; set; } = false;
    public bool Perforator { get; set; } = false;
    public bool SlimeGod { get; set; } = false;
    public bool Cryogen { get; set; } = false;
    public bool AquaticScourge { get; set; } = false;
    public bool BrimstoneElemental { get; set; } = false;
    public bool CalamitasClone { get; set; } = false;
    public bool Leviathan { get; set; } = false;
    public bool AstrumAureus { get; set; } = false;
    public bool Goliath { get; set; } = false;
    public bool Ravager { get; set; } = false;
    public bool AstrumDeus { get; set; } = false;
    public bool Guardians { get; set; } = false;
    public bool Dragonfolly { get; set; } = false;
    public bool Providence { get; set; } = false;
    public bool CeaselessVoid { get; set; } = false;
    public bool StormWeaver { get; set; } = false;
    public bool Signus { get; set; } = false;
    public bool Polterghast { get; set; } = false;
    public bool BommerDuke { get; set; } = false;
    public bool DoG { get; set; } = false;
    public bool Yharon { get; set; } = false;
    public bool Ares { get; set; } = false;
    public bool Thanatos { get; set; } = false;
    public bool ArtemisAndApollo { get; set; } = false;
    public bool ExoMechs
    {
        get;
        set
        {
            field = value;
            if (value)
            {
                LastBoss = true;
                if (Calamitas)
                    Focus = true;
            }
        }
    } = false;
    public bool Calamitas
    {
        get;
        set
        {
            field = value;
            if (value)
            {
                LastBoss = true;
                if (ExoMechs)
                    Focus = true;
            }
        }
    } = false;
    /// <summary>
    /// 单锁（击败星流巨械和灾厄之一）。
    /// </summary>
    public bool LastBoss { get; set; } = false;
    /// <summary>
    /// 万物的焦点。
    /// </summary>
    public bool Focus { get; set; } = false;
    public bool PrimordialWyrm { get; set; } = false;

    public bool GreatSandShark { get; set; } = false;
    public bool GiantClam { get; set; } = false;
    public bool GiantClamHardmode { get; set; } = false;
    /// <summary>
    /// 峭咽潭。
    /// </summary>
    public bool CragmawMire { get; set; } = false;
    /// <summary>
    /// 渊海狂鲨。
    /// </summary>
    public bool Mauler { get; set; } = false;
    /// <summary>
    /// 辐核骇兽。
    /// </summary>
    public bool NuclearTerror { get; set; } = false;

    public bool EoCAcidRain { get; set; } = false;
    public bool AquaticScourgeAcidRain { get; set; } = false;

    public bool BossRush { get; set; } = false;

    /// <summary>
    /// 使玩家跟随世界Boss击败状态。
    /// </summary>
    public override void WorldPolluted()
    {
        base.WorldPolluted();

        //灾厄添加的原版Boss跟踪
        if (DownedBossSystem.downedDreadnautilus)
            Dreadnautilus = true;
        if (DownedBossSystem.downedBetsy)
            Betsy = true;

        //灾厄Boss
        if (DownedBossSystem.downedDesertScourge)
            DesertScourge = true;
        if (DownedBossSystem.downedCrabulon)
            Crabulon = true;
        if (DownedBossSystem.downedHiveMind)
            HiveMind = true;
        if (DownedBossSystem.downedPerforator)
            Perforator = true;
        EvilBoss2 = HiveMind || Perforator;
        if (DownedBossSystem.downedSlimeGod)
            SlimeGod = true;
        if (DownedBossSystem.downedCryogen)
            Cryogen = true;
        if (DownedBossSystem.downedAquaticScourge)
            AquaticScourge = true;
        if (DownedBossSystem.downedBrimstoneElemental)
            BrimstoneElemental = true;
        if (DownedBossSystem.downedCalamitasClone)
            CalamitasClone = true;
        if (DownedBossSystem.downedLeviathan)
            Leviathan = true;
        if (DownedBossSystem.downedAstrumAureus)
            AstrumAureus = true;
        if (DownedBossSystem.downedPlaguebringer)
            Goliath = true;
        if (DownedBossSystem.downedRavager)
            Ravager = true;
        if (DownedBossSystem.downedAstrumDeus)
            AstrumDeus = true;
        if (DownedBossSystem.downedGuardians)
            Guardians = true;
        if (DownedBossSystem.downedDragonfolly)
            Dragonfolly = true;
        if (DownedBossSystem.downedProvidence)
            Providence = true;
        if (DownedBossSystem.downedCeaselessVoid)
            CeaselessVoid = true;
        if (DownedBossSystem.downedStormWeaver)
            StormWeaver = true;
        if (DownedBossSystem.downedSignus)
            Signus = true;
        if (DownedBossSystem.downedPolterghast)
            Polterghast = true;
        if (DownedBossSystem.downedBoomerDuke)
            BommerDuke = true;
        if (DownedBossSystem.downedDoG)
            DoG = true;
        if (DownedBossSystem.downedYharon)
            Yharon = true;
        if (DownedBossSystem.downedAres)
            Ares = true;
        if (DownedBossSystem.downedThanatos)
            Thanatos = true;
        if (DownedBossSystem.downedArtemisAndApollo)
            ArtemisAndApollo = true;
        if (DownedBossSystem.downedExoMechs)
            ExoMechs = true;
        if (DownedBossSystem.downedCalamitas)
            Calamitas = true;
        if (DownedBossSystem.downedPrimordialWyrm)
            PrimordialWyrm = true;

        //灾厄迷你Boss
        if (DownedBossSystem.downedGSS)
            GreatSandShark = true;
        if (DownedBossSystem.downedCLAM)
            GiantClam = true;
        if (DownedBossSystem.downedCLAMHardMode)
            GiantClamHardmode = true;
        if (DownedBossSystem.downedCragmawMire)
            CragmawMire = true;
        if (DownedBossSystem.downedMauler)
            Mauler = true;
        if (DownedBossSystem.downedNuclearTerror)
            NuclearTerror = true;

        //灾厄事件
        if (DownedBossSystem.downedEoCAcidRain)
            EoCAcidRain = true;
        if (DownedBossSystem.downedAquaticScourgeAcidRain)
            AquaticScourgeAcidRain = true;
        if (DownedBossSystem.downedBossRush)
            BossRush = true;
    }

    public override void SaveData(TagCompound tag, string key)
    {
        List<string> downed = [];
        SaveDataToList(downed);
        tag[key] = downed;
    }

    public override void SaveDataToList(List<string> downed)
    {
        base.SaveDataToList(downed);

        if (DesertScourge)
            downed.Add("DesertScourge");
        if (Crabulon)
            downed.Add("Crabulon");
        if (EvilBoss2)
            downed.Add("EvilBoss2");
        if (HiveMind)
            downed.Add("HiveMind");
        if (Perforator)
            downed.Add("Perforator");
        if (SlimeGod)
            downed.Add("SlimeGod");
        if (Cryogen)
            downed.Add("Cryogen");
        if (AquaticScourge)
            downed.Add("AquaticScourge");
        if (BrimstoneElemental)
            downed.Add("BrimstoneElemental");
        if (CalamitasClone)
            downed.Add("CalamitasClone");
        if (Leviathan)
            downed.Add("Leviathan");
        if (AstrumAureus)
            downed.Add("AstrumAureus");
        if (Goliath)
            downed.Add("Plaguebringer");
        if (Ravager)
            downed.Add("Ravager");
        if (AstrumDeus)
            downed.Add("AstrumDeus");
        if (Guardians)
            downed.Add("Guardians");
        if (Dragonfolly)
            downed.Add("Dragonfolly");
        if (Providence)
            downed.Add("Providence");
        if (CeaselessVoid)
            downed.Add("CeaselessVoid");
        if (StormWeaver)
            downed.Add("StormWeaver");
        if (Signus)
            downed.Add("Signus");
        if (Polterghast)
            downed.Add("Polterghast");
        if (BommerDuke)
            downed.Add("BommerDuke");
        if (DoG)
            downed.Add("DoG");
        if (Yharon)
            downed.Add("Yharon");
        if (Ares)
            downed.Add("Ares");
        if (Thanatos)
            downed.Add("Thanatos");
        if (ArtemisAndApollo)
            downed.Add("ArtemisAndApollo");
        if (ExoMechs)
            downed.Add("ExoMechs");
        if (Calamitas)
            downed.Add("Calamitas");
        if (PrimordialWyrm)
            downed.Add("PrimordialWyrm");

        if (GreatSandShark)
            downed.Add("GreatSandShark");
        if (GiantClam)
            downed.Add("GiantClam");
        if (GiantClamHardmode)
            downed.Add("GiantClamHardmode");
        if (CragmawMire)
            downed.Add("CragmawMire");
        if (Mauler)
            downed.Add("Mauler");
        if (NuclearTerror)
            downed.Add("NuclearTerror");

        if (EoCAcidRain)
            downed.Add("EoCAcidRain");
        if (AquaticScourgeAcidRain)
            downed.Add("AquaticScourgeAcidRain");
        if (BossRush)
            downed.Add("BossRush");
    }

    public override void LoadData(TagCompound tag, string key) => LoadDataFromIList(tag.GetList<string>(key));

    public override void LoadDataFromIList(IList<string> downedLoaded)
    {
        base.LoadDataFromIList(downedLoaded);

        if (downedLoaded.Contains("DesertScourge"))
            DesertScourge = true;
        if (downedLoaded.Contains("Crabulon"))
            Crabulon = true;
        if (downedLoaded.Contains("HiveMind"))
            HiveMind = true;
        if (downedLoaded.Contains("Perforator"))
            Perforator = true;
        if (downedLoaded.Contains("EvilBoss2"))
            EvilBoss2 = true;
        if (downedLoaded.Contains("SlimeGod"))
            SlimeGod = true;
        if (downedLoaded.Contains("Cryogen"))
            Cryogen = true;
        if (downedLoaded.Contains("AquaticScourge"))
            AquaticScourge = true;
        if (downedLoaded.Contains("BrimstoneElemental"))
            BrimstoneElemental = true;
        if (downedLoaded.Contains("CalamitasClone"))
            CalamitasClone = true;
        if (downedLoaded.Contains("Leviathan"))
            Leviathan = true;
        if (downedLoaded.Contains("AstrumAureus"))
            AstrumAureus = true;
        if (downedLoaded.Contains("Plaguebringer"))
            Goliath = true;
        if (downedLoaded.Contains("Ravager"))
            Ravager = true;
        if (downedLoaded.Contains("AstrumDeus"))
            AstrumDeus = true;
        if (downedLoaded.Contains("Guardians"))
            Guardians = true;
        if (downedLoaded.Contains("Dragonfolly"))
            Dragonfolly = true;
        if (downedLoaded.Contains("Providence"))
            Providence = true;
        if (downedLoaded.Contains("CeaselessVoid"))
            CeaselessVoid = true;
        if (downedLoaded.Contains("StormWeaver"))
            StormWeaver = true;
        if (downedLoaded.Contains("Signus"))
            Signus = true;
        if (downedLoaded.Contains("Polterghast"))
            Polterghast = true;
        if (downedLoaded.Contains("BommerDuke"))
            BommerDuke = true;
        if (downedLoaded.Contains("DoG"))
            DoG = true;
        if (downedLoaded.Contains("Yharon"))
            Yharon = true;
        if (downedLoaded.Contains("Ares"))
            Ares = true;
        if (downedLoaded.Contains("Thanatos"))
            Thanatos = true;
        if (downedLoaded.Contains("ArtemisAndApollo"))
            ArtemisAndApollo = true;
        if (downedLoaded.Contains("ExoMechs"))
            ExoMechs = true;
        if (downedLoaded.Contains("Calamitas"))
            Calamitas = true;
        if (downedLoaded.Contains("PrimordialWyrm"))
            PrimordialWyrm = true;

        if (downedLoaded.Contains("GreatSandShark"))
            GreatSandShark = true;
        if (downedLoaded.Contains("GiantClam"))
            GiantClam = true;
        if (downedLoaded.Contains("GiantClamHardmode"))
            GiantClamHardmode = true;
        if (downedLoaded.Contains("CragmawMire"))
            CragmawMire = true;
        if (downedLoaded.Contains("Mauler"))
            Mauler = true;
        if (downedLoaded.Contains("NuclearTerror"))
            NuclearTerror = true;

        if (downedLoaded.Contains("EoCAcidRain"))
            EoCAcidRain = true;
        if (downedLoaded.Contains("AquaticScourgeAcidRain"))
            AquaticScourgeAcidRain = true;
        if (downedLoaded.Contains("BossRush"))
            BossRush = true;
    }

    /// <summary>
    /// 击杀Boss时的处理。
    /// </summary>
    /// <param name="npc"></param>
    public void BossesOnKill(NPC npc)
    {
        switch (npc.ModNPC)
        {
            case null:
                switch (npc.type)
                {
                    // 原版Boss
                    case NPCID.KingSlime:
                        KingSlime = true;
                        break;
                    case NPCID.EyeofCthulhu:
                        EyeOfCthulhu = true;
                        break;
                    case NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail when npc.boss:
                        EaterOfWorld = true;
                        break;
                    case NPCID.BrainofCthulhu:
                        BrainOfCthulhu = true;
                        break;
                    case NPCID.QueenBee:
                        QueenBee = true;
                        break;
                    case NPCID.SkeletronHead:
                        Skeletron = true;
                        break;
                    case NPCID.Deerclops:
                        Deerclops = true;
                        break;
                    case NPCID.WallofFlesh:
                        WallOfFlesh = true;
                        break;
                    case NPCID.TheDestroyer:
                        Destroyer = true;
                        break;
                    case int _ when TONPCUtils.IsDefeatingTwins(npc):
                        Twins = true;
                        break;
                    case NPCID.SkeletronPrime:
                        SkeletronPrime = true;
                        break;
                    case NPCID.Plantera:
                        Plantera = true;
                        break;
                    case NPCID.Golem:
                        Golem = true;
                        break;
                    case NPCID.CultistBoss:
                        LunaticCultist = true;
                        break;
                    case NPCID.MoonLordCore:
                        MoonLord = true;
                        break;

                    // 原版事件Boss
                    case NPCID.MourningWood:
                        MourningWood = true;
                        break;
                    case NPCID.Pumpking:
                        Pumpking = true;
                        break;
                    case NPCID.Everscream:
                        Everscream = true;
                        break;
                    case NPCID.SantaNK1:
                        SantaNK1 = true;
                        break;
                    case NPCID.IceQueen:
                        IceQueen = true;
                        break;
                    case NPCID.DD2Betsy:
                        Betsy = true;
                        break;
                    case NPCID.BloodNautilus:
                        Dreadnautilus = true;
                        break;
                    case NPCID.LunarTowerSolar:
                        SolarTower = true;
                        break;
                    case NPCID.LunarTowerVortex:
                        VortexTower = true;
                        break;
                    case NPCID.LunarTowerNebula:
                        NebulaTower = true;
                        break;
                    case NPCID.LunarTowerStardust:
                        StardustTower = true;
                        break;
                }
                break;
            //灾厄Boss
            case DesertScourgeHead:
                DesertScourge = true;
                break;
            case Crabulon _:
                Crabulon = true;
                break;
            case HiveMind _:
                HiveMind = true;
                break;
            case PerforatorHive:
                Perforator = true;
                break;
            case SlimeGodCore:
                SlimeGod = true;
                break;
            case Cryogen _:
                Cryogen = true;
                break;
            case AquaticScourgeHead:
                AquaticScourge = true;
                break;
            case BrimstoneElemental _:
                BrimstoneElemental = true;
                break;
            case CalamitasClone _:
                CalamitasClone = true;
                break;
            case var _ when CAUtils.IsDefeatingLeviathan(npc):
                Leviathan = true;
                break;
            case AstrumAureus _:
                AstrumAureus = true;
                break;
            case PlaguebringerGoliath:
                Goliath = true;
                break;
            case RavagerBody:
                Ravager = true;
                break;
            case AstrumDeusHead:
                AstrumDeus = true;
                break;
            case var _ when CAUtils.IsDefeatingProfanedGuardians(npc):
                Guardians = true;
                break;
            case Bumblefuck:
                Dragonfolly = true;
                break;
            case Providence _:
                Providence = true;
                break;
            case CeaselessVoid _:
                CeaselessVoid = true;
                break;
            case StormWeaverHead:
                StormWeaver = true;
                break;
            case Signus _:
                Signus = true;
                break;
            case Polterghast _:
                Polterghast = true;
                break;
            case OldDuke:
                BommerDuke = true;
                break;
            case DevourerofGodsHead:
                DoG = true;
                break;
            case Yharon _:
                Yharon = true;
                break;
            case var _ when CAUtils.IsDefeatingExoMechs(npc):
                ExoMechs = true;
                break;
            case SupremeCalamitas:
                Calamitas = true;
                break;
            case EidolonWyrmHead:
                PrimordialWyrm = true;
                break;
            //灾厄迷你Boss
            case GreatSandShark _:
                GreatSandShark = true;
                break;
            case Clam:
                GiantClam = true;
                if (Main.hardMode)
                    GiantClamHardmode = true;
                break;
            case CragmawMire _:
                CragmawMire = true;
                break;
            case Mauler _:
                Mauler = true;
                break;
            case NuclearTerror _:
                NuclearTerror = true;
                break;
        }
    }
}
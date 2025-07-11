using CalamityAnomalies.Publicizer.CalamityMod.NPCs;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Projectiles.Melee;

namespace CalamityAnomalies.Tweaks._5_1_PostDoG;

/* 鬼妖村正
 * 改动
 * 
 * 1. 全局
 * 1) 怒气或肾上腺素开启时，攻击速度翻倍，但非大型挥砍仅造成30%伤害。
 * 2) 忽视敌人200%防御、90%基础伤害减免和所有动态伤害减免。
 * 
 * 2. 无彩蛋
 * 击败犽戎后，基础伤害提升至3000，大小提升50%。
 * 
 * 3. 彩蛋（玩家名含"Jetstream Sam"）
 * 1) 大小翻倍。
 * 1) 基础伤害改为15，但随击败Boss提升倍率：（所有倍率提升相加后作用于基础伤害）
 *  括号内数值：（总倍率, 基础伤害, 传奇复仇加强下的基础伤害）
 *   克苏鲁之眼 - 20%（1.2x, 18, 24）
 *   世界吞噬者或克苏鲁之脑 - 40%（1.6x, 24, 32）
 *   腐巢意志或血肉宿主 - 60%（2.2x, 33, 44）
 *   骷髅王 - 80%（3x, 45, 60）
 *   史莱姆之神 - 100%（4x, 60, 80）
 *   血肉墙 - 400%（8x, 120, 160）
 *   灾厄之影 - 100%（9x, 135, 180）
 *   世纪之花 - 200%（11x, 165, 220）
 *   石巨人 - 300%（14x, 210, 280）
 *   拜月教邪教徒 - 400%（18x, 270, 360）
 *   月亮领主 - 3200%（50x, 750, 1000）
 *   亵渎天神 - 2000%（70x, 1050, 1400）
 *   噬魂幽花 - 2000%（90x, 1350, 1800）
 *   神明吞噬者 - 6000%（150x, 2250, 3000）
 *   犽戎 - 10000%（250x, 3750, 5000）
 *   星流巨械 - 5000%（300x, 4500, 6000）
 *   灾厄 - 5000%（350x, 5250, 7000）
 *   同时击败星流巨械和灾厄 - 15000%（500x, 7500, 10000）
 * 2) 若处于BossRush事件中，基础伤害锁定为20001（真实力量回归！）。
 * 
 * 4. 彩蛋（传奇复仇加强）
 * 1) 基础伤害提升1/3（20）。
 * 2) 击退翻倍（13）。
 * 3) 怒气和肾上腺素同时开启时，大小再翻倍，为原版的四倍。
 * 4) 对下列敌怪具有特殊效果：
 *   彩虹史莱姆 - 秒杀
 *   黄沙恶虫 - 300%伤害
 *   真菌孢子（生物） - 秒杀
 *   噬魂怪 - 秒杀
 *   沼泽之眼 - 秒杀
 *   魔教徒 - 秒杀
 *   火焰小鬼 - 秒杀
 *   毁灭者 - 150%伤害
 *   探测怪 - 200%伤害
 *   机械骷髅王手臂 - 200%伤害
 *   极地之灵的冰川护盾 - 秒杀
 *   极地之灵 - 150%伤害
 *   渊海灾虫 - 200%伤害
 *   硫磺火元素 - 200%伤害
 *   小硫火灵 - 200%伤害
 *   灾厄之影 - 150%伤害
 *   灾难，灾祸构造体 - 250%伤害
 *   世纪之花触手（脱离前&脱离后） - 200%伤害
 *   阿娜希塔的冰护盾 - 秒杀
 *   阿娜希塔 - 130%伤害
 *   利维坦 - 150%伤害
 *   深海吞食者 - 300%伤害
 *   白金星舰 - 130%伤害
 *   小白星 - 秒杀
 *   石巨人之拳 - 200%伤害
 *   石巨人头（脱离前&脱离后） - 150%伤害
 *   幻影弓龙 - 秒杀
 *   月亮领主心脏 - 130%伤害
 *   神石守卫 - 200%伤害
 *   统御守卫 - 150%伤害
 *   痴愚金龙 - 秒杀（璀璨华焰或至尊山猪（GFB）存活时）
 *   癫狂龙裔 - 秒杀
 *   风暴编织者 - 无视头部和身体的减伤 未完成
 *   暗能量 - 150%伤害
 *   无尽虚空 - 秒杀（生命值低于20%时）
 *   花灵（噬魂幽花召唤） - 秒杀
 *   硫海遗爵 - 清除其速度，并击退一段距离 未完成
 *   神明吞噬者 - 200%伤害
 *   星宇护卫 - 600%伤害
 *   塔纳托斯 - 150%伤害
 *   阿尔忒弥斯和阿波罗 - 130%伤害
 *   硫磺火心 - 200%伤害
 *   灾坟魔物头部和尾部（GFB）- 150%伤害
 *   灾难，灾祸 - 130%伤害
 */

public sealed class Murasama_Tweak : CAItemTweak<Murasama>
{
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        if (!Murasama_Utils.IsSam(player))
        {
            if (DownedBossSystem.downedYharon)
                damage.Base = -Item.damage + 3000f;
            return;
        }

        float finalDamage;
        if (CAWorld.RealBossRushEventActive)
            finalDamage = 20001f;
        else
        {
            float baseDamage = CAWorld.LR ? 20f : 15f;
            float multiplier = 1f;
            if (NPC.downedBoss1)
                multiplier += 0.2f;
            if (NPC.downedBoss2)
                multiplier += 0.4f;
            if (CAUtils.DownedEvilBossT2)
                multiplier += 0.6f;
            if (NPC.downedBoss3)
                multiplier += 0.8f;
            if (DownedBossSystem.downedSlimeGod)
                multiplier += 1f;
            if (Main.hardMode)
                multiplier += 4f;
            if (DownedBossSystem.downedCalamitasClone)
                multiplier += 1f;
            if (NPC.downedPlantBoss)
                multiplier += 2f;
            if (NPC.downedGolemBoss)
                multiplier += 3f;
            if (NPC.downedAncientCultist)
                multiplier += 4f;
            if (NPC.downedMoonlord)
                multiplier += 32f;
            if (DownedBossSystem.downedProvidence)
                multiplier += 20f;
            if (DownedBossSystem.downedPolterghast)
                multiplier += 20f;
            if (DownedBossSystem.downedDoG)
                multiplier += 60f;
            if (DownedBossSystem.downedYharon)
                multiplier += 100f;
            if (DownedBossSystem.downedExoMechs)
                multiplier += 50f;
            if (DownedBossSystem.downedCalamitas)
                multiplier += 50f;
            if (DownedBossSystem.downedExoMechs && DownedBossSystem.downedCalamitas)
                multiplier += 150f;
            finalDamage = baseDamage * multiplier;
        }

        damage.Base = -Item.damage + finalDamage;
    }

    public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback)
    {
        if (Murasama_Utils.IsSam(player) && CAWorld.LR)
            knockback *= 2f;
    }

    public override bool? CanHitNPC(Player player, NPC target)
    {
        if (Murasama_Utils.IsSam(player))
        {
            if (Main.zenithWorld && target.ModNPC is HiveTumor && NPC.AnyNPCs<HiveTumor>())
                return false;
        }

        return null;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
    }
}

public sealed class Murasama_Detour : ModItemDetour<Murasama>
{
    public override bool Detour_PreDrawInInventory(Orig_PreDrawInInventory orig, Murasama self, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture;

        if (Murasama_Utils.Unlocked(Main.LocalPlayer))
        {
            //0 = 6 frames, 8 = 3 frames]
            texture = ModContent.Request<Texture2D>(self.Texture).Value;
            spriteBatch.Draw(texture, position, self.Item.GetCurrentFrame(ref self.frame, ref self.frameCounter, 2, 13), Color.White, 0f, origin, scale, SpriteEffects.None, 0);
        }
        else
        {
            texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/MurasamaSheathed").Value;
            spriteBatch.Draw(texture, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0);
        }

        return false;
    }

    public override bool Detour_PreDrawInWorld(Orig_PreDrawInWorld orig, Murasama self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Texture2D texture;

        if (Murasama_Utils.Unlocked(Main.LocalPlayer))
        {
            texture = ModContent.Request<Texture2D>(self.Texture).Value;
            spriteBatch.Draw(texture, self.Item.position - Main.screenPosition, self.Item.GetCurrentFrame(ref self.frame, ref self.frameCounter, 2, 13), lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
        else
        {
            texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/MurasamaSheathed").Value;
            spriteBatch.Draw(texture, self.Item.position - Main.screenPosition, null, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }

        return false;
    }

    public override void Detour_PostDrawInWorld(Orig_PostDrawInWorld orig, Murasama self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/MurasamaGlow").Value;
        spriteBatch.Draw(texture, self.Item.position - Main.screenPosition, self.Item.GetCurrentFrame(ref self.frame, ref self.frameCounter, 2, 13, false), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
    }

    public override bool Detour_CanUseItem(Orig_CanUseItem orig, Murasama self, Player player) => player.ownedProjectileCounts[self.Item.shoot] < 1 && Murasama_Utils.Unlocked(player);
}

public sealed class MurasamaSlash_Tweak : CAProjectileTweak<MurasamaSlash>
{
    public int OriginalDamage
    {
        get => AnomalyProjectile.AnomalyAI[0].i;
        set
        {
            if (AnomalyProjectile.AnomalyAI[0].i != value)
            {
                AnomalyProjectile.AnomalyAI[0].i = value;
                AnomalyProjectile.AIChanged[0] = true;
            }
        }
    }

    public override void SetDefaults()
    {
        if (Murasama_Utils.IsSam(Owner))
            Projectile.ArmorPenetration += 200;
    }

    public override bool PreAI()
    {
        bool isSam = Murasama_Utils.IsSam(Owner);
        CalamityPlayer calamityPlayer = Owner.Calamity();
        bool rageModeActive = calamityPlayer.rageModeActive;
        bool adrenalineModeActive = calamityPlayer.adrenalineModeActive;

        Projectile.scale = isSam ? (CAWorld.LR && rageModeActive && adrenalineModeActive ? 4f : 2f) : (DownedBossSystem.downedYharon ? 1.5f : 1f);
        Projectile.extraUpdates = rageModeActive || adrenalineModeActive ? 1 : 0;

        if (ModProjectile.time == 0)
        {
            OriginalDamage = Projectile.damage;
            Projectile.frame = Main.zenithWorld ? 6 : 10;
            Projectile.alpha = 0;
            ModProjectile.time++;
        }

        if (ModProjectile.Slash2)
        {
            SoundEngine.PlaySound(Murasama.Swing with { Pitch = -0.1f }, Projectile.Center);
            if (ModProjectile.hitCooldown == 0)
                ModProjectile.Slashing = true;
            Projectile.numHits = 0;
        }
        else if (ModProjectile.Slash3)
        {
            SoundEngine.PlaySound(Murasama.BigSwing with { Pitch = 0f }, Projectile.Center);
            if (ModProjectile.hitCooldown == 0)
                ModProjectile.Slashing = true;
            Projectile.numHits = 0;
        }
        else if (ModProjectile.Slash1)
        {
            SoundEngine.PlaySound(Murasama.Swing with { Pitch = -0.05f }, Projectile.Center);
            if (ModProjectile.hitCooldown == 0)
                ModProjectile.Slashing = true;
            Projectile.numHits = 0;
        }
        else
            ModProjectile.Slashing = false;

        if (Projectile.frameCounter % 3 == 0)
        {
            switch (Projectile.frame)
            {
                case 5:
                    Projectile.damage = OriginalDamage;
                    break;
                case 7:
                    Projectile.damage = rageModeActive || adrenalineModeActive ? (int)(OriginalDamage * 0.3f) : OriginalDamage;
                    break;
            }
        }

        //Frames and crap
        Projectile.frameCounter++;
        if (Projectile.frameCounter % 3 == 0)
            Projectile.frame = (Projectile.frame + 1) % Main.projFrames[ModProjectile.Type];

        Vector2 origin = Projectile.Center + Projectile.velocity * 3f;
        Lighting.AddLight(origin, Color.Red.ToVector3() * (ModProjectile.Slashing == true ? 3.5f : 2f));

        Vector2 playerRotatedPoint = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
        if (Projectile.OnOwnerClient)
        {
            if (!Owner.CantUseHoldout())
                ModProjectile.HandleChannelMovement(Owner, playerRotatedPoint);
            else
            {
                ModProjectile.hitCooldown = Main.zenithWorld ? 0 : 8;
                Projectile.Kill();
            }
        }

        // Rotation and directioning.
        if (ModProjectile.Slashing || ModProjectile.Slash1)
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.direction == -1).ToInt() * MathHelper.Pi;

        float velocityAngle = Projectile.velocity.ToRotation();
        Projectile.direction = (Math.Cos(velocityAngle) > 0).ToDirectionInt();

        // Positioning close to the end of the player's arm.
        float offset = 80f * Projectile.scale;
        Projectile.Center = playerRotatedPoint + velocityAngle.ToRotationVector2() * offset;

        // Sprite and player directioning.
        Owner.ChangeDir(Projectile.direction);

        // Prevents the projectile from dying
        Projectile.timeLeft = 2;

        // Player item-based field manipulation.
        Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        Owner.heldProj = Projectile.whoAmI;
        Owner.itemTime = 2;
        Owner.itemAnimation = 2;

        return false;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        if (Murasama_Utils.IsSam(Owner))
        {
            int hitboxBonus = ModProjectile.Slash3 ? 130 : 70;
            hitbox.Inflate(hitboxBonus, hitboxBonus);
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!Murasama_Utils.IsSam(Owner))
            return;

        if (target.Ravager)
            modifiers.SourceDamage *= 3f;

        if (!CAWorld.LR)
            return;

        switch (target.ModNPC)
        {
            case null:
                switch (target.type)
                {
                    case NPCID.RainbowSlime when NPC.AnyNPCs(NPCID.KingSlime):
                    case NPCID.EaterofSouls when NPC.AnyNPCs(NPCID.EaterofWorldsHead):
                    case NPCID.DiabolistWhite when NPC.AnyNPCs(NPCID.SkeletronHead):
                    case NPCID.FireImp when NPC.AnyNPCs(NPCID.WallofFlesh):
                    case int _ when target.CultistDragon:
                        modifiers.SetInstantKillBetter(target);
                        break;
                    case int _ when target.Destroyer:
                        modifiers.SourceDamage *= 1.5f;
                        break;
                    case NPCID.Probe:
                        modifiers.SourceDamage *= NPC.AnyNPCs(NPCID.SkeletronPrime) ? 4f : 2f;
                        break;
                    case int _ when target.SkeletronPrimeHand:
                        modifiers.SourceDamage *= 2f;
                        break;
                    case NPCID.PlanterasTentacle:
                        modifiers.SourceDamage *= 2;
                        break;
                    case int _ when target.GolemFist:
                        modifiers.SourceDamage *= 2f;
                        break;
                    case NPCID.GolemHead or NPCID.GolemHeadFree:
                        modifiers.SourceDamage *= 1.5f;
                        break;
                    case NPCID.MoonLordCore:
                        modifiers.SourceDamage *= 1.3f;
                        break;
                }
                break;
            case CrabShroom:
            case HiveBlob or HiveBlob2 or DankCreeper or DarkHeart:
            case CryogenShield:
            case AnahitasIceShield:
            case AureusSpawn:
            case Bumblefuck when CAUtils.PermaFrostActive || (NPC.AnyNPCs(out _, out Yharon yharon) && new Yharon_Publicizer(yharon).startSecondAI):
            case Bumblefuck2:
            case CeaselessVoid when target.Ocean().LifeRatio < 0.2f:
            case PhantomFuckYou:
                modifiers.SetInstantKillBetter(target);
                break;
            case var _ when target.DesertNuisance || target.DesertNuisanceYoung:
                modifiers.SourceDamage *= 3f;
                break;
            case Cryogen:
                modifiers.SourceDamage *= 1.5f;
                break;
            case var _ when target.AquaticScourge:
                modifiers.SourceDamage *= 2f;
                break;
            case BrimstoneElemental:
                modifiers.SourceDamage *= 2f;
                break;
            case Brimling:
                modifiers.SourceDamage *= 2f;
                break;
            case CalamitasClone:
                modifiers.SourceDamage *= 1.5f;
                break;
            case Catastrophe or Cataclysm:
                modifiers.SourceDamage *= 2.5f;
                break;
            case Anahita:
                modifiers.SourceDamage *= 1.3f;
                break;
            case Leviathan:
                modifiers.SourceDamage *= 1.5f;
                break;
            case AquaticAberration:
                modifiers.SourceDamage *= 3f;
                break;
            case AstrumAureus:
                modifiers.SourceDamage *= 1.5f;
                break;
            case ProfanedGuardianDefender:
                modifiers.SourceDamage *= 2f;
                break;
            case ProfanedGuardianCommander:
                modifiers.SourceDamage *= 1.5f;
                break;
            case var _ when target.StormWeaver:
                break;
            case DarkEnergy:
                modifiers.SourceDamage *= 1.5f;
                break;
            case OldDuke:
                break;
            case var _ when target.DoG:
                modifiers.SourceDamage *= 2f;
                break;
            case var _ when target.CosmicGuardian:
                modifiers.SourceDamage *= 6f;
                break;
            case var _ when target.Thanatos:
                modifiers.SourceDamage *= 1.5f;
                break;
            case var _ when target.ExoTwins:
                modifiers.SourceDamage *= 1.3f;
                break;
            case BrimstoneHeart:
                modifiers.SourceDamage *= 2f;
                break;
            case SepulcherHead or SepulcherTail when Main.zenithWorld:
                modifiers.SourceDamage *= 1.5f;
                break;
            case SupremeCatastrophe or SupremeCataclysm:
                modifiers.SourceDamage *= 1.3f;
                break;
        }
    }

    public override void ModifyHitNPC_DR(NPC npc, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier)
    {
        if (Murasama_Utils.IsSam(Owner))
        {
            baseDRModifier.Base *= 0f;
            timedDRModifier *= 0f;
        }
        else if (baseDR <= 0.95f)
            baseDRModifier.Base -= 0.95f;
    }
}

public static class Murasama_Utils
{
    public static bool IsSam(Player player) => player.name == "Jetstream Sam";

    public static bool Unlocked(Player player) => DownedBossSystem.downedDoG || IsSam(player) || TOMain.IsDEBUGPlayer(player);
}

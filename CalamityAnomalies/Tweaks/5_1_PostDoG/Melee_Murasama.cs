using System;
using System.Collections.Generic;
using CalamityAnomalies.Override;
using CalamityAnomalies.Utilities;
using CalamityMod;
using CalamityMod.CalPlayer;
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
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic.Core.GameData.Utilities;
using Transoceanic.Core.IL;
using Transoceanic.Core.MathHelp;
using Transoceanic.GlobalInstances;

namespace CalamityAnomalies.Tweaks._5_1_PostDoG;

//鬼妖村正

public sealed class MurasamaOverride : CAItemTweak<Murasama>
{
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        if (!MurasamaUtils.IsSam(player))
        {
            if (DownedBossSystem.downedYharon)
            {
                damage.Base = -Item.damage + 3000f;
                return;
            }
        }

        float finalDamage = CAWorld.LR ? 20f : 15f;
        float multiplier = 1f;
        if (NPC.downedBoss1)
            multiplier += 0.2f;
        if (NPC.downedBoss2)
            multiplier += 0.4f;
        if (CANPCUtils.DownedEvilBossT2)
            multiplier += 0.6f;
        if (NPC.downedBoss3)
            multiplier += 0.8f;
        if (DownedBossSystem.downedSlimeGod)
            multiplier += 1f;
        if (Main.hardMode)
            multiplier += 4f;
        if (DownedBossSystem.downedCalamitasClone || NPC.downedPlantBoss)
            multiplier += 3f;
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
            multiplier += 80f;
        if (DownedBossSystem.downedExoMechs)
            multiplier += 50f;
        if (DownedBossSystem.downedCalamitas)
            multiplier += 50f;

        damage.Base = -Item.damage + finalDamage * multiplier;
    }

    public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback)
    {
        if (MurasamaUtils.IsSam(player) && CAWorld.LR)
            knockback *= 2f;
    }

    public override bool? CanHitNPC(Player player, NPC target)
    {
        if (MurasamaUtils.IsSam(player))
        {
            if (Main.zenithWorld && target.ModNPC is HiveTumor && TONPCUtils.AnyNPCs<HiveTumor>())
                return false;
        }

        return null;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
    }
}

public sealed class MurasamaSlashOverride : CAProjectileTweak<MurasamaSlash>
{
    public override bool PreAI()
    {
        bool isSam = MurasamaUtils.IsSam(Owner);
        CalamityPlayer calamityPlayer = Owner.Calamity();
        bool rageModeActive = calamityPlayer.rageModeActive;
        bool adrenalineModeActive = calamityPlayer.adrenalineModeActive;

        Projectile.scale = isSam ? (CAWorld.LR && rageModeActive && adrenalineModeActive ? 4f : 2f) : 1f;

        if (rageModeActive || adrenalineModeActive)
            Projectile.extraUpdates = 1;

        if (ModProjectile.time == 0)
        {
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
                    Projectile.damage = Projectile.damage * 2;
                    break;
                case 7:
                    Projectile.damage = (int)(Projectile.damage * 0.5f);
                    break;
            }
        }

        //Frames and crap
        Projectile.frameCounter++;
        if (Projectile.frameCounter % 3 == 0)
        {
            Projectile.frame = (Projectile.frame + 1) % Main.projFrames[ModProjectile.Type];
        }

        Vector2 origin = Projectile.Center + Projectile.velocity * 3f;
        Lighting.AddLight(origin, Color.Red.ToVector3() * (ModProjectile.Slashing == true ? 3.5f : 2f));

        Vector2 playerRotatedPoint = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
        if (Main.myPlayer == Projectile.owner)
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
        {
            float velocityAngle = Projectile.velocity.ToRotation();
            Projectile.rotation = velocityAngle + (Projectile.direction == -1).ToInt() * MathHelper.Pi;
        }
        float velocityAngle2 = Projectile.velocity.ToRotation();
        Projectile.direction = (Math.Cos(velocityAngle2) > 0).ToDirectionInt();

        // Positioning close to the end of the player's arm.
        float offset = 80f * Projectile.scale;
        Projectile.Center = playerRotatedPoint + velocityAngle2.ToRotationVector2() * offset;

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

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!MurasamaUtils.IsSam(Owner))
            return;

        if (target.IsRavager())
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
                    case int _ when target.IsCultistDragon():
                        modifiers.SetInstantKill2(target);
                        break;
                    case int _ when target.IsDestroyer():
                        modifiers.SourceDamage *= 1.5f;
                        break;
                    case NPCID.Probe:
                        modifiers.SourceDamage *= NPC.AnyNPCs(NPCID.SkeletronPrime) ? 4f : 2f;
                        break;
                    case int _ when target.IsSkeletronPrimeHand():
                        modifiers.SourceDamage *= 2f;
                        break;
                    case NPCID.PlanterasTentacle:
                        modifiers.SourceDamage *= 2;
                        break;
                    case int _ when target.IsGolemFist():
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
            case Bumblefuck when CANPCUtils.CirrusActive || (TONPCUtils.AnyNPCs<Yharon>(out NPC yharon) && yharon.Ocean().LifeRatio <= 0.55f):
            case Bumblefuck2:
            case CeaselessVoid when target.Ocean().LifeRatio < 0.2f:
            case PhantomFuckYou:
                modifiers.SetInstantKill2(target);
                break;
            case var _ when target.IsDesertNuisance() || target.IsDesertNuisanceYoung():
                modifiers.SourceDamage *= 3f;
                break;
            case Cryogen:
                modifiers.SourceDamage *= 1.5f;
                break;
            case var _ when target.IsAquaticScourge():
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
            case var _ when target.IsStormWeaver():
                break;
            case DarkEnergy:
                modifiers.SourceDamage *= 1.5f;
                break;
            case OldDuke:
                break;
            case var _ when target.IsDoG():
                modifiers.SourceDamage *= 2f;
                break;
            case var _ when target.IsCosmicGuardian():
                modifiers.SourceDamage *= 6f;
                break;
            case var _ when target.IsThanatos():
                modifiers.SourceDamage *= 1.5f;
                break;
            case var _ when target.IsExoTwins():
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

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return base.Colliding(projHitbox, targetHitbox);
    }
}

[TODetour(typeof(Murasama))]
public sealed class On_Murasama
{
    internal delegate bool Orig_PreDrawInInventory(Murasama self, SpriteBatch spriteBatch, Vector2 position, Rectangle frameI, Color drawColor, Color itemColor, Vector2 origin, float scale);

    internal static bool Detour_PreDrawInInventory(Orig_PreDrawInInventory orig, Murasama self, SpriteBatch spriteBatch, Vector2 position, Rectangle frameI, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture;

        if (MurasamaUtils.Unlocked(Main.LocalPlayer))
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

    internal delegate bool Orig_PreDrawInWorld(Murasama self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI);

    internal static bool Detour_PreDrawInWorld(Orig_PreDrawInWorld orig, Murasama self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Texture2D texture;

        if (MurasamaUtils.Unlocked(Main.LocalPlayer))
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

    internal delegate void Orig_PostDrawInWorld(Murasama self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI);

    internal static void Detour_PostDrawInWorld(Orig_PostDrawInWorld orig, Murasama self, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if (!MurasamaUtils.Unlocked(Main.LocalPlayer))
            return;

        Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/MurasamaGlow").Value;
        spriteBatch.Draw(texture, self.Item.position - Main.screenPosition, self.Item.GetCurrentFrame(ref self.frame, ref self.frameCounter, 2, 13, false), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
    }

    internal delegate bool Orig_CanUseItem(Murasama self, Player player);

    internal static bool Detour_CanUseItem(Orig_CanUseItem orig, Murasama self, Player player)
    {
        if (player.ownedProjectileCounts[self.Item.shoot] > 0)
            return false;

        return MurasamaUtils.Unlocked(player);
    }
}

[TODetour(typeof(MurasamaSlash))]
public sealed class On_MurasamaSlash
{
    internal delegate bool Orig_PreDraw(MurasamaSlash self, ref Color lightColor);

    internal static bool Detour_PreDraw(Orig_PreDraw orig, MurasamaSlash self, ref Color lightColor)
    {
        if (self.Projectile.frameCounter <= 1)
            return false;

        Player owner = Main.player[self.Projectile.owner];
        bool isSam = MurasamaUtils.IsSam(owner);
        CalamityPlayer calamityPlayer = owner.Calamity();
        bool rageModeActive = calamityPlayer.rageModeActive;
        bool adrenalineModeActive = calamityPlayer.adrenalineModeActive;

        float scale = isSam ? (CAWorld.LR && rageModeActive && adrenalineModeActive ? 4f : 2f) : 1f;

        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[self.Projectile.type].Value;
        Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[self.Type], frameY: self.Projectile.frame);
        Vector2 origin = frame.Size() * 0.5f;
        SpriteEffects spriteEffects = self.Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Main.EntitySpriteDraw(texture, self.Projectile.Center - Main.screenPosition + (self.Projectile.velocity * 0.3f) + new Vector2(0, -32).RotatedBy(self.Projectile.rotation), frame, Color.White, self.Projectile.rotation, origin, scale, spriteEffects, 0);
        return false;
    }
}

public static class MurasamaUtils
{
    public static bool IsSam(Player player) => player.name.Contains("Jetstream Sam");

    public static bool Unlocked(Player player) => DownedBossSystem.downedDoG || IsSam(player);
}

/*
 * 鬼妖村正（传奇复仇彩蛋加强）对下列敌怪具有特殊效果：
 * 彩虹史莱姆 - 秒杀
 * 黄沙恶虫 - 300%伤害
 * 真菌孢子（生物） - 秒杀
 * 噬魂怪 - 秒杀
 * 沼泽之眼 - 秒杀
 * 魔教徒 - 秒杀
 * 火焰小鬼 - 秒杀
 * 毁灭者 - 150%伤害
 * 探测怪 - 200%伤害
 * 机械骷髅王手臂 - 200%伤害
 * 极地之灵的冰川护盾 - 秒杀
 * 极地之灵 - 150%伤害
 * 渊海灾虫 - 200%伤害
 * 硫磺火元素 - 200%伤害
 * 小硫火灵 - 200%伤害
 * 灾厄之影 - 150%伤害
 * 灾难，灾祸构造体 - 250%伤害
 * 世纪之花触手（脱离前&脱离后） - 200%伤害
 * 阿娜希塔的冰护盾 - 秒杀
 * 阿娜希塔 - 130%伤害
 * 利维坦 - 150%伤害
 * 深海吞食者 - 300%伤害
 * 白金星舰 - 130%伤害
 * 小白星 - 秒杀
 * 石巨人之拳 - 200%伤害
 * 石巨人头（脱离前&脱离后） - 150%伤害
 * 幻影弓龙 - 秒杀
 * 月亮领主心脏 - 130%伤害
 * 神石守卫 - 200%伤害
 * 统御守卫 - 150%伤害
 * 痴愚金龙 - 秒杀（仅在璀璨华焰或至尊山猪（GFB）存活时）
 * 癫狂龙裔 - 秒杀
 * 风暴编织者 - 无视头部和身体的减伤 TODO
 * 暗能量 - 150%伤害
 * 无尽虚空 - 若低于20%生命值，则秒杀
 * 花灵（噬魂幽花召唤） - 秒杀
 * 硫海遗爵 - 清除其速度，并击退一段距离 TODO
 * 神明吞噬者 - 200%伤害
 * 星宇护卫 - 600%伤害
 * 塔纳托斯 - 150%伤害
 * 阿尔忒弥斯和阿波罗 - 130%伤害
 * 硫磺火心 - 200%伤害
 * 灾坟魔物头部和尾部（GFB）- 150%伤害
 * 灾难，灾祸 - 130%伤害
 */

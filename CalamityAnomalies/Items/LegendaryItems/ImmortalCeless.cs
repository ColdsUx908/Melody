using System;
using CalamityAnomalies.Items.ItemRarities;
using CalamityAnomalies.Projectiles.LegendaryItems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core;
using Transoceanic.Data;
using Transoceanic.Systems;
using TransoceanicCalamity.Core;
using TransoceanicCalamity.Systems;

namespace CalamityAnomalies.Items.LegendaryItems;

public class ImmortalCeless : LegendaryItem, ILocalizedModType
{
    #region 传奇
    public int Phase_1 { get; private set; } = 1; //Max: 6
    public int Phase_2 { get; private set; } = 1; //Max: 9
    public int Phase_3 { get; private set; } = 1; //Max: 21

    public override void SetPhase(Player player)
    {
        PlayerDownedBossCalamity downedGet = player.OceanCal().PlayerDownedBossCalamity;

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

    private int projAmount = 5;

    private static readonly int projVoid = ModContent.ProjectileType<ImmortalVoidRain>();
    private static readonly int projIce = ModContent.ProjectileType<ImmortalIceRain>();
    private static readonly int projBlood = ModContent.ProjectileType<ImmortalBloodRain>();
    //private static readonly int godType = ModContent.ProjectileType<ImmortalGod>();

    public new string LocalizationCategory => "Items.LegendaryItems";

    public override void SetDefaults()
    {
        Item.width = 124;
        Item.height = 52;
        Item.damage = 8;
        Item.DamageType = DamageClass.Magic;
        Item.rare = ModContent.RarityType<Celestial>();
        Item.value = TOMain.celestialValue;
        Item.knockBack = 1f;
        Item.useTime = Item.useAnimation = 6;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.autoReuse = true;
        Item.shoot = projIce;
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
        /*if (player.ownedProjectileCounts[godType] < 1)
        {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, godType, damage, 0, -1, 0, 0, 0);
        }*/
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse == 2)
        {
            Projectile.NewProjectile(source, position, TOMathHelper.ToCustomLength(velocity, 10f), projBlood, damage * 3, knockback * 3f, -1, 0, 0, 0);
        }
        else
        {
            TOEntityActivator.RotatedProj(projAmount, MathHelper.TwoPi / projAmount, source, player.Center, new Vector2(0f, -15f), type, damage, knockback, -1, p => p.ai[2] = 15f); //ai[2]传递速度信息

            float offsetangle;
            for (int i = 0; i < projAmount * 2; i++)
            {
                float velocity2 = Main.rand.NextFloat(4f, 10f);
                int t = Main.rand.Next(i, i * 4);
                offsetangle = (float)Math.Pow(t + 1, 2) + t * 3;
                Vector2 velocity3 = new PolarVector2(velocity2, offsetangle);
                Projectile.NewProjectile(source, player.Center, velocity3, type, damage, knockback, -1, 0, 0, velocity2);
            }
        }

        return false;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {

    }

    public override float UseTimeMultiplier(Player player) => player.altFunctionUse == 2 ? 1f : 10f;
}
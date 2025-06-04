using CalamityAnomalies.Items.ItemRarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.GlobalInstances;
using Transoceanic.Systems;
using Transoceanic.Visual;

namespace CalamityAnomalies.Items.LegendaryItems;

[AutoloadEquip(EquipType.Wings)]
public class AscendantWings : LegendaryItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.LegendaryItems";
    public override void SetStaticDefaults() => ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(30, 14.5f, 5.5f);
    public override void SetDefaults()
    {
        Item.width = 54;
        Item.height = 54;
        Item.value = TOMain.CelestialPrice;
        Item.rare = ModContent.RarityType<Celestial>();
        Item.accessory = true;
    }

    public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
        ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        ascentWhenFalling = 0.5f;
        ascentWhenRising = 0.3f;
        maxCanAscendMultiplier = 1f;
        maxAscentMultiplier = 5f;
        constantAscend = 0.2f;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.noFallDmg = true;

        //神佑
        if (player.Ocean().Celesgod)
        {
            //禁用额外跳跃
            player.jump = 0;
            //无限飞行时间
            player.wingTime = 230;
            //免疫扭曲debuff
            player.buffImmune[BuffID.VortexDebuff] = true;
            //正常重力
            player.gravity = 0.4f;
            //无视液体
            player.wet = false;

            /*
            Preferences configpath = new Preferences(TOGlobal.ConfigPath);
            configpath?.Load();
            if (!configpath.Get("Hitbox", false))
            {
                Rectangle hitbox = player.Hitbox;
                hitbox.X += player.Hitbox.Width / 2;
                hitbox.Y += player.Hitbox.Height / 2;
                hitbox.Width = 1;
                hitbox.Height = 1;
            }
            */


            if (player.TryingToHoverDown)
            {
            }

            if (player.TryingToHoverUp)
            {
                Player.jumpSpeed *= 2f;
            }
        }
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        TODrawUtils.InventoryCustomSize(spriteBatch, position, frame, drawColor, origin,
            TextureAssets.Item[Type].Value, 0.35f, new(0f, 0f));
        return false;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityAnomalies.Override;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override Color? GetAlpha(Item item, Color lightColor)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            Color? result = itemOverride.GetAlpha(lightColor);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI))
                return false;
        }

        return true;
    }

    public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
        Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
        {
            if (!itemOverride.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale))
                return false;
        }

        return true;
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
        Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (item.TryGetOverride(out CAItemOverride itemOverride))
            itemOverride.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }
}

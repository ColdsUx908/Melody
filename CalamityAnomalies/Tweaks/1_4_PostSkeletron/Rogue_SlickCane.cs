using CalamityAnomalies.Override;
using CalamityMod.Projectiles.Rogue;
using Terraria;
using Terraria.ID;

namespace CalamityAnomalies.Tweaks._1_4_PostSkeletron;

//盗贼手杖

public sealed class SlickCaneProjectileOverride : CAProjectileTweak<SlickCaneProjectile>
{
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (CAWorld.LR && target.type == NPCID.WallofFleshEye)
            modifiers.SetCrit();
    }
}

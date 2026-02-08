using CalamityMod.Projectiles.Rogue;

namespace CalamityAnomalies.Tweaks;

/* 盗贼手杖
 * 改动
 * 在LR难度下，击中血肉墙眼睛时必定暴击。
 */

public sealed class SlickCaneProjectile_Tweak : CAProjectileTweak<SlickCaneProjectile>
{
    public override CAGamePhase Phase => CAGamePhase.PostSkeletron;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (CASharedData.LR && target.type == NPCID.WallofFleshEye)
            modifiers.SetCrit();
    }
}

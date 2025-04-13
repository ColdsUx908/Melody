using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TransoceanicCalamity.Systems;

namespace TransoceanicCalamity.GlobalEntity.GlobalProjectiles;

public partial class TOCGlobalProjectile : GlobalProjectile
{
    public override bool PreAI(Projectile projectile)
    {
        return base.PreAI(projectile);
    }

    public override void PostAI(Projectile projectile)
    {
        //反额外更新七彩矢
        if (projectile.type == ProjectileID.HallowBossRainbowStreak && projectile.extraUpdates > 0 && AntiEPBSystem.AntiEPBGeneral)
        {
            projectile.extraUpdates = 0;
            projectile.netUpdate = true;
            //Main.NewText($"DEBUG: 检测到额外更新七彩矢（{projectile.whoAmI}），已处理");
        }
    }
}

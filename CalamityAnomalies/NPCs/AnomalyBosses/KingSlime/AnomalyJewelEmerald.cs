using CalamityMod.NPCs.NormalNPCs;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.NPCs.AnomalyBosses.KingSlime;

public class AnomalyJewelEmerald : NPCOverride
{
    public override int OverrideNPCType => ModContent.NPCType<KingSlimeJewelEmerald>();

    public override DisableCalamityMethods DisableCalamityMethods => DisableCalamityMethods.None;

    public override bool DisableAI => false;

    public override void AnomalyAI()
    {
        return; //TODO: 移植未完成

        //如果找不到所属史莱姆王，直接脱战
        if (!OceanNPC.HasMaster)
        {
            NPC.life = 0;
            NPC.HitEffect();
            NPC.active = false;
            NPC.netUpdate = true;
            return;
        }

        NPC master = Main.npc[OceanNPC.Master];

        Lighting.AddLight(NPC.Center, 0f, 0.8f, 0f);
    }
}

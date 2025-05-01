using CalamityMod.NPCs.NormalNPCs;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.Contents.AnomalyNPCs.KingSlime;

public class AnomalyJewelEmerald : AnomalyNPCOverride
{
    public override int OverrideNPCType => ModContent.NPCType<KingSlimeJewelEmerald>();

    public override bool AllowOrigMethod(OrigMethodType type) => type switch
    {
        OrigMethodType.AI => true,
        _ => true,
    };

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

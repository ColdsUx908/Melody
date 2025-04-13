using CalamityMod.NPCs.NormalNPCs;
using Terraria.ModLoader;

namespace CalamityAnomalies.NPCs.AnomalyBosses.KingSlime;

public class AnomalyJewelEmerald : NPCOverride
{
    public override int OverrideNPCType => ModContent.NPCType<KingSlimeJewelEmerald>();

    public override DisableCalamityMethods DisableCalamityMethods => DisableCalamityMethods.None;

    public override bool DisableAI => false;
}

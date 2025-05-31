using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Transoceanic.Net;

namespace CalamityAnomalies.GlobalInstances.GlobalNPCs;

public partial class CAGlobalNPC : GlobalNPC
{
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.SendAI(AnomalyAI, AIChanged, binaryWriter);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        TONetUtils.ReceiveAI(AnomalyAI, binaryReader);
    }

}

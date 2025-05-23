using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Transoceanic.Core.Net;

namespace Transoceanic.GlobalInstances.GlobalNPCs;

public partial class TOGlobalNPC : GlobalNPC
{
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.SendAI(OceanAI, AIChanged, binaryWriter);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        TONetUtils.ReceiveAI(OceanAI, binaryReader);
    }
}

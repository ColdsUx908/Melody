using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityAnomalies.GlobalInstances;

public partial class CAGlobalNPC : GlobalNPC
{
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        Dictionary<byte, float> values = [];
        for (int i = 0; i < MaxAISlots; i++)
        {
            if (ShouldSyncAnomalyAI[i])
            {
                values[(byte)i] = AnomalyAI[i];
                ShouldSyncAnomalyAI[i] = false;
            }
        }
        binaryWriter.Write((byte)values.Count);
        foreach ((byte index, float value) in values)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        for (int i = 0; i < binaryReader.ReadByte(); i++)
            AnomalyAI[binaryReader.ReadByte()] = binaryReader.ReadSingle();
    }

}

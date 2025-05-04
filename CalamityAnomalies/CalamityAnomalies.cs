using System.IO;
using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.GlobalInstances.GlobalNPCs;
using CalamityAnomalies.Net;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies;

public class CalamityAnomalies : Mod
{
    internal static CalamityAnomalies Instance { get; private set; }

    public override void Load()
    {
        Instance = this;
    }

    public override void PostSetupContent()
    {
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        switch (reader.ReadByte())
        {
            case CANetPacketID.SyncAllAnomalyAI:
                SyncAllAnomalyAI_Func();
                break;
            case CANetPacketID.SyncAnomalyAIWithIndexes:
                SyncAnomalyAIWithIndexes_Func();
                break;
        }

        void SyncAllAnomalyAI_Func()
        {
            CAGlobalNPC anomalyNPC = Main.npc[reader.ReadByte()].Anomaly();
            for (int i = 0; i < anomalyNPC.AnomalyAI.Length; i++)
                anomalyNPC.AnomalyAI[i] = reader.ReadSingle();
        }

        void SyncAnomalyAIWithIndexes_Func()
        {
            int totalIndexes = reader.ReadByte();
            CAGlobalNPC anomalyNPC = Main.npc[reader.ReadByte()].Anomaly();
            for (int i = 0; i < totalIndexes; i++)
                anomalyNPC.AnomalyAI[reader.ReadByte()] = reader.ReadSingle();
        }
    }

    public override void Unload()
    {
        Instance = null;
    }
}

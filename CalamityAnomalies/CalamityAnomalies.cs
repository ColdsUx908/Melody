using System.IO;
using CalamityAnomalies.Net;
using CalamityAnomalies.Systems;
using CalamityMod.Systems;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies;

public class CalamityAnomalies : Mod
{
    internal static CalamityAnomalies Instance;

    public override void Load()
    {
        Instance = this;
        DifficultyModeSystem.Difficulties.Add(new AnomalyMode());
        DifficultyModeSystem.CalculateDifficultyData();
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
            ref float[] anomalyAI = ref Main.npc[reader.ReadByte()].Anomaly().anomalyAI;
            for (int i = 0; i < anomalyAI.Length; i++)
                anomalyAI[i] = reader.ReadSingle();
        }

        void SyncAnomalyAIWithIndexes_Func()
        {
            int totalIndexes = reader.ReadByte();
            ref float[] anomalyAI = ref Main.npc[reader.ReadByte()].Anomaly().anomalyAI;
            for (int i = 0; i < totalIndexes; i++)
                anomalyAI[reader.ReadByte()] = reader.ReadSingle();
        }
    }

    public override void Unload()
    {
        Instance = null;
    }
}

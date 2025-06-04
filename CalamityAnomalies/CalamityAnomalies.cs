global using Calamity = CalamityMod.CalamityMod;
using System;
using System.IO;
using System.Reflection;
using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.Net;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.IL;

namespace CalamityAnomalies;

public class CalamityAnomalies : Mod
{
    internal static CalamityAnomalies Instance { get; private set; }

    public override void Load()
    {
        Instance = this;
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        switch (reader.ReadByte())
        {
            case CANetPacketID.SyncAllAnomalyAI:
                SyncAllAnomalyAI_Func(reader);
                break;
            case CANetPacketID.SyncAnomalyAIWithIndexes:
                SyncAnomalyAIWithIndexes_Func(reader);
                break;
        }
    }

    private static void SyncAllAnomalyAI_Func(BinaryReader reader)
    {
        CAGlobalNPC anomalyNPC = Main.npc[reader.ReadByte()].Anomaly();
        for (int i = 0; i < anomalyNPC.AnomalyAI.Length; i++)
            anomalyNPC.AnomalyAI[i] = reader.ReadSingle();
    }

    private static void SyncAnomalyAIWithIndexes_Func(BinaryReader reader)
    {
        int totalIndexes = reader.ReadByte();
        CAGlobalNPC anomalyNPC = Main.npc[reader.ReadByte()].Anomaly();
        for (int i = 0; i < totalIndexes; i++)
            anomalyNPC.AnomalyAI[reader.ReadByte()] = reader.ReadSingle();
    }

    public override void Unload()
    {
        Instance = null;
    }
}

public class CAMain : ITOLoader
{
    public static Assembly Assembly { get; } = CalamityAnomalies.Instance.Code;

    public const string ModLocalizationPrefix = "Mods.CalamityAnomalies.";

    public static Type Type_CalamityMod { get; } = typeof(Calamity);

    public static Calamity CalamityModInstance { get; internal set; }

    public static Color AnomalyUltramundaneColor { get; } = new(0xE8, 0x97, 0xFF);

    void ITOLoader.PostSetupContent()
    {
        CalamityModInstance = (Calamity)Type_CalamityMod.GetField("Instance", TOReflectionUtils.UniversalBindingFlags).GetValue(null);
        TOMain.SyncEnabled = true;
    }
    void ITOLoader.OnModUnload()
    {
        CalamityModInstance = null;
    }
}
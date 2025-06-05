global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using CalamityAnomalies.Utilities;
global using CalamityAnomalies.GlobalInstances;
global using CalamityMod;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using ReLogic.Graphics;
global using Terraria;
global using Terraria.Audio;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using Terraria.ID;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using Terraria.ModLoader.IO;
global using Terraria.Utilities;
global using Transoceanic;
global using Transoceanic.Extensions;
global using Transoceanic.ExtraGameData;
global using Transoceanic.ExtraMathData;
global using Transoceanic.GameData;
global using Transoceanic.GameData.Utilities;
global using Transoceanic.IL;
global using Transoceanic.Localization;
global using Transoceanic.MathHelp;
global using Transoceanic.Net;
global using Transoceanic.Visual;
global using ZLinq;
global using Calamity = CalamityMod.CalamityMod;
using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.Net;

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

    public const string TexturePrefix = "CalamityAnomalies/Textures/";

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
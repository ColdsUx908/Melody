using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Transoceanic.Systems;

public class CompletedEventSystem : ModSystem
{
    public static bool CompletedPumpkinMoon { get; set; } = false;
    public static bool CompletedFrostMoon { get; set; } = false;

    public override void PostUpdateWorld()
    {
        if (Main.pumpkinMoon && NPC.waveNumber == 15)
            CompletedPumpkinMoon = true;
        if (Main.snowMoon && NPC.waveNumber == 20)
            CompletedFrostMoon = true;

    }

    public override void SaveWorldData(TagCompound tag)
    {
        List<string> completed = [];
        if (CompletedPumpkinMoon)
            completed.Add("PumpkinMoon");
        if (CompletedFrostMoon)
            completed.Add("FrostMoon");

        tag["CompletedEvent"] = completed;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        IList<string> completed = tag.GetList<string>("CompletedEvent");

        CompletedPumpkinMoon = completed.Contains("PumpkinMoon");
        CompletedFrostMoon = completed.Contains("FrostMoon");
    }
}

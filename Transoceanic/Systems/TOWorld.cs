using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Transoceanic.Core;

namespace Transoceanic.Systems;

public class TOWorld : ModSystem
{
    public static bool TrueMasterMode { get; set; } = false;
    public static bool LegendaryMode { get; set; } = false;

    public override void PreUpdateEntities()
    {
        TrueMasterMode = ((GameModeData)typeof(Main).GetField("_currentGameModeInfo", TOMain.UniversalBindingFlags).GetValue(null)).IsMasterMode;
        LegendaryMode = Main.getGoodWorld && TrueMasterMode;
    }

    public override void PostUpdateNPCs()
    {
        TOEntityIterator<NPC> bosses = TOMain.Bosses;
        TOMain.BossList = bosses.ToList();
        TOMain.BossActive = bosses.Any();
    }
}

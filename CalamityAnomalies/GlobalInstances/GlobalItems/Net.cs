using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace CalamityAnomalies.GlobalInstances.GlobalItems;

public partial class CAGlobalItem : GlobalItem
{
    public override void NetSend(Item item, BinaryWriter writer)
    {
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
    }
}

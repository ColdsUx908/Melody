using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Transoceanic.Net;

namespace CalamityAnomalies.GlobalInstances.GlobalProjectiles;

public partial class CAGlobalProjectile : GlobalProjectile
{
    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.SendAI(AnomalyAI, AIChanged, binaryWriter);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        TONetUtils.ReceiveAI(AnomalyAI, binaryReader);
    }
}

using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityAnomalies;

/// <summary>
/// 提供世界变量。
/// <br>仅保存异象属性，不保存异象超凡属性。</br>
/// </summary>
public class CAWorld
{
    public static bool Anomaly { get; set; } = false;
    public static bool AnomalyUltramundane { get; set; } = false;
}

public class CAWorldSavingSystem : ModSystem
{
    public override void OnWorldLoad()
    {
        CAWorld.AnomalyUltramundane = false;
    }

    public override void OnWorldUnload()
    {
        CAWorld.AnomalyUltramundane = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["Anomaly"] = CAWorld.Anomaly;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        CAWorld.Anomaly = tag.GetBool("Anomaly");
    }
}

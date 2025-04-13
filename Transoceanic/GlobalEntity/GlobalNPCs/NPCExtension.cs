using Terraria.ModLoader;

namespace Transoceanic.GlobalEntity.GlobalNPCs;

public partial class TOGlobalNPC : GlobalNPC
{

    /// <summary>
    /// 额外伤害减免。在所有伤害减免生效后独立生效。不建议使用。
    /// </summary>
    public float ExtraDR { get; set; } = 0;
    /// <summary>
    /// 动态伤害减免。
    /// </summary>
    public float DynamicDR { get; internal set; } = 0;
}

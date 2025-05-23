using System;
using Terraria;
using Terraria.ModLoader;

namespace Transoceanic.GlobalInstances.GlobalNPCs;

public partial class TOGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    /// <summary>
    /// NPC的标识符。
    /// <br/>若NPC在进入世界后生成，则标识符为正数。
    /// </summary>
    public ulong Identifier { get; internal set; } = 0;
    /// <summary>
    /// 标识符分配器。
    /// <br/>进入世界时重置为0。
    /// </summary>
    internal static ulong _identifierAllocator;

    public ulong SpawnTime { get; internal set; } = 0;

    private const int MaxAISlots = 24;

    /// <summary>
    /// 额外的AI槽位，共24个。
    /// </summary>
    public float[] OceanAI { get; } = new float[MaxAISlots];

    public bool[] AIChanged { get; } = new bool[MaxAISlots];

    public void SetOceanAI(float value, Index index)
    {
        OceanAI[index] = value;
        AIChanged[index] = true;
    }

    public int Master
    {
        get => (int)OceanAI[^1];
        set => SetOceanAI(Math.Clamp(value, 0, Main.maxNPCs), ^1);
    }

    public bool TryGetMaster(out NPC master)
    {
        NPC temp = Main.npc[Master];
        if (temp.active)
        {
            master = temp;
            return true;
        }
        master = null;
        return false;
    }

    public bool TryGetMaster(int masterType, out NPC master)
    {
        NPC temp = Main.npc[Master];
        if (temp.active && temp.type == masterType)
        {
            master = temp;
            return true;
        }
        master = null;
        return false;
    }

    public bool TryGetMaster<T>(out NPC master) where T : ModNPC => TryGetMaster(ModContent.NPCType<T>(), out master);

    /// <summary>
    /// 额外伤害减免。在所有伤害减免生效后独立生效。不建议使用。
    /// </summary>
    public float ExtraDR { get; set; } = 0;
    /// <summary>
    /// 动态伤害减免。
    /// </summary>
    public float DynamicDR { get; internal set; } = 0;

    public float LifeRatio { get; internal set; } = 0f;

    public float LifeRatioReverse { get; internal set; } = 0f;
}

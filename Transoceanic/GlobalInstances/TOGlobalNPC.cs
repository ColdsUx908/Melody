namespace Transoceanic.GlobalInstances;

public class TOGlobalNPC : GlobalNPC, ITOLoader
{
    public override bool InstancePerEntity => true;

    #region Data
    /// <summary>
    /// 标识符分配器。
    /// </summary>
    private static long _identifierAllocator;

    private long? _identifier = null;

    /// <summary>
    /// NPC的标识符。
    /// <br/>不同步。
    /// </summary>
    public long Identifier => _identifier ??= ++_identifierAllocator;

    /// <summary>
    /// NPC生成时 <see cref="TOMain.GameTimer"/> 的值。
    /// <br/>不同步。
    /// </summary>
    public int SpawnTime { get; internal set; } = -1;

    private const int AISlot = 33;
    private const int AISlot2 = 17;

    private Union32[] OceanAI32 { get; } = new Union32[AISlot];
    private Union64[] OceanAI64 { get; } = new Union64[AISlot2];

    private ref Bits32 AIChanged32 => ref OceanAI32[^1].bits;
    private ref Bits64 AIChanged64 => ref OceanAI64[^1].bits;

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        TOGlobalNPC clone = (TOGlobalNPC)base.Clone(from, to);

        clone.SpawnTime = SpawnTime;
        Array.Copy(OceanAI32, clone.OceanAI32, AISlot);
        Array.Copy(OceanAI64, clone.OceanAI64, AISlot2);

        return clone;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (!TOMain.SyncEnabled)
            return;

        Dictionary<int, float> aiToSend = [];
        for (int i = 0; i < AISlot - 1; i++)
        {
            if (AIChanged32[i])
                aiToSend[i] = OceanAI32[i].f;
        }
        binaryWriter.Write(aiToSend.Count);
        foreach ((int index, float value) in aiToSend)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged32 = default;

        Dictionary<int, double> aiToSend2 = [];
        for (int i = 0; i < AISlot2 - 1; i++)
        {
            if (AIChanged64[i])
                aiToSend2[i] = OceanAI64[i].d;
        }
        binaryWriter.Write(aiToSend2.Count);
        foreach ((int index, double value) in aiToSend2)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged64 = default;
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TOMain.SyncEnabled)
            return;

        int recievedAICount = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount; i++)
        {
            int index = binaryReader.ReadInt32();
            float value = binaryReader.ReadSingle();
            OceanAI32[index].f = value;
        }

        int recievedAICount2 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount2; i++)
        {
            int index = binaryReader.ReadInt32();
            double value = binaryReader.ReadDouble();
            OceanAI64[index].d = value;
        }
    }

    void ITOLoader.Load()
    {
        _identifierAllocator = 0;
    }

    void ITOLoader.Unload()
    {
        _identifierAllocator = 0;
    }
    #endregion Data

    #region 额外数据
    public bool AlwaysRotating
    {
        get => OceanAI32[0].bits[0];
        set
        {
            if (OceanAI32[0].bits[0] != value)
            {
                OceanAI32[0].bits[0] = value;
                AIChanged32[0] = true;
            }
        }
    }

    public int ActiveTime
    {
        get => OceanAI32[1].i;
        set
        {
            if (OceanAI32[1].i != value)
            {
                OceanAI32[1].i = value;
                AIChanged32[1] = true;
            }
        }
    }

    public float RotationOffset
    {
        get => OceanAI32[2].f;
        set
        {
            if (OceanAI32[2].f != value)
            {
                OceanAI32[2].f = value;
                AIChanged32[2] = true;
            }
        }
    }

    public float LifeRatio
    {
        get => OceanAI32[3].f;
        set
        {
            if (OceanAI32[3].f != value)
            {
                OceanAI32[3].f = value;
                AIChanged32[3] = true;
            }
        }
    }

    public float LifeRatioReverse => 1f - LifeRatio;

    public int Master
    {
        get => OceanAI32[4].i;
        set
        {
            if (OceanAI32[4].i != value)
            {
                OceanAI32[4].i = value;
                AIChanged32[4] = true;
            }
        }
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

    public bool TryGetMaster<T>(out NPC master, out T modNPC) where T : ModNPC
    {
        if (TryGetMaster(ModContent.NPCType<T>(), out master))
        {
            modNPC = master.GetModNPC<T>();
            return true;
        }
        modNPC = null;
        return false;
    }

    public int Timer1
    {
        get => OceanAI32[27].i;
        set
        {
            if (OceanAI32[27].i != value)
            {
                OceanAI32[27].i = value;
                AIChanged32[27] = true;
            }
        }
    }

    public int Timer2
    {
        get => OceanAI32[28].i;
        set
        {
            if (OceanAI32[28].i != value)
            {
                OceanAI32[28].i = value;
                AIChanged32[28] = true;
            }
        }
    }

    public int Timer3
    {
        get => OceanAI32[29].i;
        set
        {
            if (OceanAI32[29].i != value)
            {
                OceanAI32[29].i = value;
                AIChanged32[29] = true;
            }
        }
    }

    public float Timer4
    {
        get => OceanAI32[30].f;
        set
        {
            if (OceanAI32[30].f != value)
            {
                OceanAI32[30].f = value;
                AIChanged32[30] = true;
            }
        }
    }

    public float Timer5
    {
        get => OceanAI32[31].f;
        set
        {
            if (OceanAI32[21].f != value)
            {
                OceanAI32[31].f = value;
                AIChanged32[31] = true;
            }
        }
    }
    #endregion 额外数据
}

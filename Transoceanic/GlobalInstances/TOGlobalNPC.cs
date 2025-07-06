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
    /// <br/>若NPC在进入世界后生成，则标识符为正数。
    /// <br/>不同步。
    /// </summary>
    public long Identifier => _identifier ??= ++_identifierAllocator;

    /// <summary>
    /// NPC生成时 <see cref="TOMain.GameTimer"/> 的值。
    /// <br/>不同步。
    /// </summary>
    public int SpawnTime { get; private set; } = -1;

    private const int AISlot = 33;
    private const int AISlot2 = 17;

    private Union32[] OceanAI { get; } = new Union32[AISlot];
    private Union64[] OceanAI2 { get; } = new Union64[AISlot2];

    private ref Bits32 AIChanged => ref OceanAI[^1].bits;
    private ref Bits64 AIChanged2 => ref OceanAI2[^1].bits;

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        TOGlobalNPC clone = (TOGlobalNPC)base.Clone(from, to);

        clone.SpawnTime = SpawnTime;
        Array.Copy(OceanAI, clone.OceanAI, AISlot);
        Array.Copy(OceanAI2, clone.OceanAI2, AISlot2);

        return clone;
    }
    #endregion Data

    #region 额外数据
    public bool AlwaysRotating
    {
        get => OceanAI[0].bits[0];
        set
        {
            if (OceanAI[0].bits[0] != value)
            {
                OceanAI[0].bits[0] = value;
                AIChanged[0] = true;
            }
        }
    }

    public int ActiveTime
    {
        get => OceanAI[1].i;
        set
        {
            if (OceanAI[1].i != value)
            {
                OceanAI[1].i = value;
                AIChanged[1] = true;
            }
        }
    }

    public float RotationOffset
    {
        get => OceanAI[2].f;
        set
        {
            if (OceanAI[2].f != value)
            {
                OceanAI[2].f = value;
                AIChanged[2] = true;
            }
        }
    }

    public float LifeRatio
    {
        get => OceanAI[3].f;
        set
        {
            if (OceanAI[3].f != value)
            {
                OceanAI[3].f = value;
                AIChanged[3] = true;
            }
        }
    }

    public float LifeRatioReverse => 1f - LifeRatio;

    public int Master
    {
        get => OceanAI[4].i;
        set
        {
            if (OceanAI[4].i != value)
            {
                OceanAI[4].i = value;
                AIChanged[4] = true;
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
        get => OceanAI[27].i;
        set
        {
            if (OceanAI[27].i != value)
            {
                OceanAI[27].i = value;
                AIChanged[27] = true;
            }
        }
    }

    public int Timer2
    {
        get => OceanAI[28].i;
        set
        {
            if (OceanAI[28].i != value)
            {
                OceanAI[28].i = value;
                AIChanged[28] = true;
            }
        }
    }

    public int Timer3
    {
        get => OceanAI[29].i;
        set
        {
            if (OceanAI[29].i != value)
            {
                OceanAI[29].i = value;
                AIChanged[29] = true;
            }
        }
    }

    public float Timer4
    {
        get => OceanAI[30].f;
        set
        {
            if (OceanAI[30].f != value)
            {
                OceanAI[30].f = value;
                AIChanged[30] = true;
            }
        }
    }

    public float Timer5
    {
        get => OceanAI[31].f;
        set
        {
            if (OceanAI[21].f != value)
            {
                OceanAI[31].f = value;
                AIChanged[31] = true;
            }
        }
    }
    #endregion 额外数据

    #region Defaults
    public override void SetDefaults(NPC npc)
    {
        Master = Main.maxNPCs;
    }
    #endregion Defaults

    #region Active
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        SpawnTime = TOMain.GameTimer.TotalTicks;
    }
    #endregion Active

    #region AI
    public override bool PreAI(NPC npc)
    {
        ActiveTime++;
        LifeRatio = (float)npc.life / npc.lifeMax;

        return true;
    }

    public override void AI(NPC npc)
    {
        if (AlwaysRotating)
            npc.VelocityToRotation(RotationOffset);
    }

    public override void PostAI(NPC npc)
    {
    }
    #endregion AI

    #region Net
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (!TOMain.SyncEnabled)
            return;

        Dictionary<int, float> aiToSend = [];
        for (int i = 0; i < AISlot - 1; i++)
        {
            if (AIChanged[i])
                aiToSend[i] = OceanAI[i].f;
        }
        binaryWriter.Write(aiToSend.Count);
        foreach ((int index, float value) in aiToSend)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged = default;

        Dictionary<int, double> aiToSend2 = [];
        for (int i = 0; i < AISlot2 - 1; i++)
        {
            if (AIChanged2[i])
                aiToSend2[i] = OceanAI2[i].d;
        }
        binaryWriter.Write(aiToSend2.Count);
        foreach ((int index, double value) in aiToSend2)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged2 = default;
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TOMain.SyncEnabled)
            return;

        int recievedAICount = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount; i++)
            OceanAI[binaryReader.ReadInt32()].f = binaryReader.ReadSingle();

        int recievedAICount2 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount2; i++)
            OceanAI2[binaryReader.ReadInt32()].d = binaryReader.ReadDouble();
    }
    #endregion Net

    #region Load
    void ITOLoader.Load()
    {
        _identifierAllocator = 0;
    }

    void ITOLoader.Unload()
    {
        _identifierAllocator = 0;
    }
    #endregion Load
}

namespace Transoceanic.GlobalInstances;

public class TOGlobalNPC : GlobalNPC, ITOLoader
{
    public override bool InstancePerEntity => true;

    #region Data
    /// <summary>
    /// 标识符分配器。
    /// <br/>进入世界时重置为0。
    /// </summary>
    private static ulong _identifierAllocator;

    /// <summary>
    /// NPC的标识符。
    /// <br/>若NPC在进入世界后生成，则标识符为正数。
    /// <br/>不同步。
    /// </summary>
    public ulong Identifier { get; private set; } = 0;

    public void AllocateIdentifier() => Identifier = ++_identifierAllocator;

    /// <summary>
    /// NPC生成时 <see cref="TOMain.GameTimer"/> 的值。
    /// <br/>不同步。
    /// </summary>
    public int SpawnTime { get; private set; } = -1;

    private const int MaxAISlots = 128;

    /// <summary>
    /// 额外的AI槽位，共128个。
    /// </summary>
    private float[] OceanAI { get; } = new float[MaxAISlots];

    private bool[] AIChanged { get; } = new bool[MaxAISlots];

    public override GlobalNPC Clone(NPC from, NPC to)
    {
        TOGlobalNPC clone = (TOGlobalNPC)base.Clone(from, to);

        clone.Identifier = Identifier;
        clone.SpawnTime = SpawnTime;
        Array.Copy(OceanAI, clone.OceanAI, MaxAISlots);
        Array.Copy(AIChanged, clone.AIChanged, MaxAISlots);

        return clone;
    }

    private void SetOceanAI(float value, int index)
    {
        OceanAI[index] = value;
        AIChanged[index] = true;
    }

    private void SetOceanAI(float value, Index index)
    {
        OceanAI[index] = value;
        AIChanged[index] = true;
    }

    private bool GetOceanAIBit(int index, byte bitPosition) => BitOperation.GetBit((int)OceanAI[index], bitPosition);

    private bool GetOceanAIBit(Index index, byte bitPosition) => BitOperation.GetBit((int)OceanAI[index], bitPosition);

    private void SetOceanAIBit(bool value, int index, byte bitPosition) => SetOceanAI(BitOperation.SetBit((int)OceanAI[index], bitPosition, value), index);

    private void SetOceanAIBit(bool value, Index index, byte bitPosition) => SetOceanAI(BitOperation.SetBit((int)OceanAI[index], bitPosition, value), index);
    #endregion Data

    public bool AlwaysRotating
    {
        get => GetOceanAIBit(0, 0);
        set => SetOceanAIBit(value, 0, 0);
    }

    public int ActiveTime
    {
        get => (int)OceanAI[1];
        set => SetOceanAI(value, 1);
    }

    public float RotationOffset
    {
        get => OceanAI[1];
        set => SetOceanAI(value, 1);
    }

    public float LifeRatio
    {
        get => OceanAI[2];
        set => SetOceanAI(value, 2);
    }

    public float LifeRatioReverse => 1f - LifeRatio;

    public int Master
    {
        get => (int)OceanAI[3];
        set => SetOceanAI(Math.Clamp(value, 0, Main.maxNPCs), 3);
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

    public int Timer1
    {
        get => (int)OceanAI[^5];
        set => SetOceanAI(value, ^5);
    }

    public int Timer2
    {
        get => (int)OceanAI[^4];
        set => SetOceanAI(value, ^4);
    }

    public int Timer3
    {
        get => (int)OceanAI[^3];
        set => SetOceanAI(value, ^3);
    }

    public float Timer4
    {
        get => OceanAI[^2];
        set => SetOceanAI(value, ^2);
    }

    public float Timer5
    {
        get => OceanAI[^1];
        set => SetOceanAI(value, ^1);
    }

    #region Defaults
    public override void SetDefaults(NPC npc)
    {
        Master = Main.maxNPCs;
    }
    #endregion Defaults

    #region Active
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        AllocateIdentifier(); //城镇NPC这类NPC不会拥有在这里被设置标识符的机会
        SpawnTime = TOMain.GameTimer;
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

        TONetUtils.SendAI(OceanAI, AIChanged, binaryWriter);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TOMain.SyncEnabled)
            return;

        TONetUtils.ReceiveAI(OceanAI, binaryReader);
    }
    #endregion Net

    #region Load
    void ITOLoader.Load()
    {
        _identifierAllocator = 0ul;
    }

    void ITOLoader.UnLoad()
    {
        _identifierAllocator = 0ul;
    }
    #endregion Load
}

namespace Transoceanic.GlobalInstances;

public class TOGlobalNPC : GlobalNPC, ITOLoader
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
    private static ulong _identifierAllocator;

    public ulong SpawnTime { get; internal set; } = 0;

    public ulong ActiveTime => TOMain.GameTimer - SpawnTime;

    private const int MaxAISlots = 64;

    /// <summary>
    /// 额外的AI槽位，共64个。
    /// </summary>
    public float[] OceanAI { get; } = new float[MaxAISlots];

    public bool[] AIChanged { get; } = new bool[MaxAISlots];

    public void SetOceanAI(float value, int index)
    {
        OceanAI[index] = value;
        AIChanged[index] = true;
    }

    public void SetOceanAI(float value, Index index)
    {
        OceanAI[index] = value;
        AIChanged[index] = true;
    }

    public bool GetOceanAIBit(int index, byte bitPosition) => TOMathHelper.GetBit((int)OceanAI[index], bitPosition);

    public bool GetOceanAIBit(Index index, byte bitPosition) => TOMathHelper.GetBit((int)OceanAI[index], bitPosition);

    public void SetOceanAIBit(bool value, int index, byte bitPosition) => SetOceanAI(TOMathHelper.SetBit((int)OceanAI[index], bitPosition, value), index);

    public void SetOceanAIBit(bool value, Index index, byte bitPosition) => SetOceanAI(TOMathHelper.SetBit((int)OceanAI[index], bitPosition, value), index);

    public float LifeRatio
    {
        get => OceanAI[^4];
        set => SetOceanAI(value, ^3);
    }

    public int Master
    {
        get => (int)OceanAI[^3];
        set => SetOceanAI(Math.Clamp(value, 0, Main.maxNPCs), ^2);
    }

    public float RotationOffset
    {
        get => OceanAI[^2];
        set => SetOceanAI(value, ^2);
    }

    public bool AlwaysRotating
    {
        get => GetOceanAIBit(^1, 0);
        set => SetOceanAIBit(value, ^1, 0);
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

    public float LifeRatioReverse => 1f - LifeRatio;

    #region Defaults
    public override void SetDefaults(NPC npc)
    {
        Master = Main.maxNPCs;
    }
    #endregion

    #region Active
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        Identifier = ++_identifierAllocator; //城镇NPC这类NPC不会拥有在这里被设置标识符的机会
        SpawnTime = TOMain.GameTimer;
    }
    #endregion

    #region AI
    public override bool PreAI(NPC npc)
    {
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
    #endregion

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
    #endregion

    #region Load
    void ITOLoader.Load()
    {
        _identifierAllocator = 0ul;
    }

    void ITOLoader.UnLoad()
    {
        _identifierAllocator = 0ul;
    }
    #endregion
}

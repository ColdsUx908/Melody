namespace Transoceanic.GlobalInstances;

public class TOGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    #region Data
    private const int AISlot = 33;
    private const int AISlot2 = 17;

    private DataUnion32[] OceanAI { get; } = new DataUnion32[AISlot];
    private DataUnion64[] OceanAI2 { get; } = new DataUnion64[AISlot2];

    private ref Bits32 AIChanged => ref OceanAI[^1].bits;
    private ref Bits64 AIChanged2 => ref OceanAI2[^1].bits;

    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        TOGlobalProjectile clone = (TOGlobalProjectile)base.Clone(from, to);

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

    public float RotationOffset
    {
        get => OceanAI[1].f;
        set
        {
            if (OceanAI[1].f != value)
            {
                OceanAI[1].f = value;
                AIChanged[1] = true;
            }
        }
    }
    #endregion 额外数据

    #region Defaults
    public override void SetDefaults(Projectile projectile)
    {
    }
    #endregion Defaults

    #region AI
    public override bool PreAI(Projectile projectile)
    {
        return true;
    }

    public override void AI(Projectile projectile)
    {
        if (AlwaysRotating)
            projectile.VelocityToRotation(RotationOffset);
    }

    public override void PostAI(Projectile projectile)
    {
    }
    #endregion AI

    #region Net
    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
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

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
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
}

namespace Transoceanic.GlobalInstances;

public class TOGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    private const int MaxAISlots = 32;

    /// <summary>
    /// 额外的AI槽位，共32个。
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

    #region Defaults
    public override void SetDefaults(Projectile projectile)
    {
    }
    #endregion

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
    #endregion

    #region Net
    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        if (!TOMain.SyncEnabled)
            return;

        TONetUtils.SendAI(OceanAI, AIChanged, binaryWriter);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        if (!TOMain.SyncEnabled)
            return;

        TONetUtils.ReceiveAI(OceanAI, binaryReader);
    }
    #endregion
}

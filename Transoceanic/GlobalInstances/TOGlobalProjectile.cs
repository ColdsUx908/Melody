namespace Transoceanic.GlobalInstances;

public class TOGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    private const int MaxAISlots = 64;

    #region Data
    /// <summary>
    /// 额外的AI槽位，共64个。
    /// </summary>
    private float[] OceanAI { get; } = new float[MaxAISlots];

    private bool[] AIChanged { get; } = new bool[MaxAISlots];

    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        TOGlobalProjectile clone = (TOGlobalProjectile)base.Clone(from, to);

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

    public float RotationOffset
    {
        get => OceanAI[1];
        set => SetOceanAI(value, 1);
    }

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
    #endregion AI

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
    #endregion Net
}

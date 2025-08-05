
namespace Transoceanic.GlobalInstances;

public sealed class TOPlayer : ModPlayer, IResourceLoader
{
    public CommandCallInfo CommandCallInfo { get; internal set; } = null;

    public int GameTime { get; internal set; } = 0;

    public bool IsHurt = false;

    /// <summary>
    /// 透支生命值。
    /// </summary>
    public double OverdrawnLife = 0;

    /// <summary>
    /// 透支生命值的最大值，可超过玩家生命值，默认为零。
    /// </summary>
    public double OverdrawnLifeLimit = 0;

    /// <summary>
    /// 透支生命值回复指数。
    /// </summary>
    public double OverdrawnLifeRegenExponent = 2;

    /// <summary>
    /// 每帧回复透支生命值的最大值。
    /// </summary>
    public double OverdrawnLifeRegenLimit = 0.5;

    /// <summary>
    /// 透支生命值回复乘数。
    /// </summary>
    public double OverdrawnLifeRegenMult = 1;

    /// <summary>
    /// 回复透支生命值所需的最小未受击时间。
    /// </summary>
    public int OverdrawnLifeRegenThreshold = 0;

    public int TimeWithoutHurt = 0;

    /// <summary>
    /// 提升玩家翅膀飞行时间的乘区。
    /// <br/>每个索引独立计算。
    /// </summary>
    public AddableFloat[] WingTimeMaxMultipliers = new AddableFloat[5];

    public override ModPlayer Clone(Player newEntity)
    {
        TOPlayer clone = (TOPlayer)base.Clone(newEntity);

        clone.CommandCallInfo = CommandCallInfo;
        clone.GameTime = GameTime;
        clone.IsHurt = IsHurt;
        clone.OverdrawnLife = OverdrawnLife;
        clone.OverdrawnLifeLimit = OverdrawnLifeLimit;
        clone.OverdrawnLifeRegenExponent = OverdrawnLifeRegenExponent;
        clone.OverdrawnLifeRegenLimit = OverdrawnLifeRegenLimit;
        clone.OverdrawnLifeRegenMult = OverdrawnLifeRegenMult;
        clone.OverdrawnLifeRegenThreshold = OverdrawnLifeRegenThreshold;
        clone.TimeWithoutHurt = TimeWithoutHurt;
        Array.Copy(WingTimeMaxMultipliers, clone.WingTimeMaxMultipliers, WingTimeMaxMultipliers.Length);

        return clone;
    }

    public override void ResetEffects()
    {
        for (int i = 0; i < WingTimeMaxMultipliers.Length; i++)
            WingTimeMaxMultipliers[i] = AddableFloat.Zero;
    }
}
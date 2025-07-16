
namespace Transoceanic.GlobalInstances;

public sealed class TOPlayer : ModPlayer, IResourceLoader
{
    public CommandCallInfo CommandCallInfo { get; internal set; } = null;

    public int GameTime { get; set; } = 0;

    public bool IsHurt { get; set; } = false;

    /// <summary>
    /// 透支生命值。
    /// </summary>
    public double OverdrawnLife { get; set; } = 0;

    /// <summary>
    /// 透支生命值的最大值，可超过玩家生命值，默认为零。
    /// </summary>
    public double OverdrawnLifeLimit { get; set; } = 0;

    /// <summary>
    /// 透支生命值回复指数。
    /// </summary>
    public double OverdrawnLifeRegenExponent { get; set; } = 2;

    /// <summary>
    /// 每帧回复透支生命值的最大值。
    /// </summary>
    public double OverdrawnLifeRegenLimit { get; set; } = 0.5;

    /// <summary>
    /// 透支生命值回复乘数。
    /// </summary>
    public double OverdrawnLifeRegenMult { get; set; } = 1;

    /// <summary>
    /// 回复透支生命值所需的最小未受击时间。
    /// </summary>
    public int OverdrawnLifeRegenThreshold { get; set; } = 0;

    public int TimeWithoutHurt { get; set; } = 0;

    /// <summary>
    /// 提升玩家翅膀飞行时间的乘区。
    /// <br/>每个索引独立计算。
    /// </summary>
    public AddableFloat[] WingTimeMaxMultipliers { get; } = new AddableFloat[5];

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
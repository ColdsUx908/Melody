using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Transoceanic.Commands;

namespace Transoceanic.GlobalInstances.Players;

public partial class TOPlayer : ModPlayer
{
    #region 特殊事件
    public bool Celesgod { get; set; } = false;
    public bool Annigod { get; set; } = false;
    #endregion

    #region 透支生命值
    private double overdrawnLifeRegenExponent = 2;

    /// <summary>
    /// 透支生命值。
    /// </summary>
    public double OverdrawnLife { get; set; } = 0;
    /// <summary>
    /// 透支生命值的最大值，可超过玩家生命值，默认为零。
    /// </summary>
    public double OverdrawnLifeLimit { get; set; } = 0;
    /// <summary>
    /// 回复透支生命值所需的最小未受击时间。
    /// </summary>
    public int OverdrawnLifeRegenThreshold { get; set; } = 0;
    /// <summary>
    /// 每帧回复透支生命值的最大值。
    /// </summary>
    public double OverdrawnLifeRegenLimit { get; set; } = 0.5;
    /// <summary>
    /// 透支生命值回复乘数。
    /// </summary>
    public double OverdrawnLifeRegenMult { get; set; } = 1;
    /// <summary>
    /// 透支生命值回复指数。
    /// </summary>
    public double OverdrawnLifeRegenExponent { get => overdrawnLifeRegenExponent; set => overdrawnLifeRegenExponent = value; }
    #endregion

    #region 通用

    public int GameTime { get; set; } = 0;

    public bool IsHurt { get; set; } = false;

    public int TimeWithoutHurt { get; set; } = 0;

    public CommandCallInfo CommandCallInfo { get; internal set; } = null;
    #endregion
}

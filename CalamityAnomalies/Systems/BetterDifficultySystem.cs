using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Systems;
using Terraria.ModLoader;

namespace CalamityAnomalies.Systems;

public abstract class BetterDifficultyMode : DifficultyMode
{
    /// <summary>
    /// 更新代码。
    /// <br/>仅在该难度模式被激活时调用。
    /// </summary>
    public virtual void Update()
    {
    }
}

public class BetterDifficultySystem : ModSystem
{
}

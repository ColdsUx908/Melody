using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Transoceanic.Systems;

public abstract class LegendaryItem : ModItem
{
    public bool HasPower { get; set; } = false;
    public virtual void SetPhase(Player player) { }
    /// <summary>
    /// 设置神器之威。
    /// </summary>
    public virtual void SetPower(Player player) => HasPower = false;
}

public class LegendaryItemSystem : ModSystem
{
    
}

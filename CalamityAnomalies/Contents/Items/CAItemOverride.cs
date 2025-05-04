using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.GlobalInstances.GlobalItems;
using CalamityMod;
using CalamityMod.Items;
using Terraria;
using Transoceanic.GlobalInstances;
using Transoceanic.GlobalInstances.GlobalItems;

namespace CalamityAnomalies.Contents.Items;

public abstract class CAItemOverride
{
    public Item Item { get; set; } = null;

    public TOGlobalItem OceanItem
    {
        get => field ?? Item?.Ocean();
        set;
    }

    public CAGlobalItem AnomalyItem
    {
        get => field ?? Item?.Anomaly();
        set;
    }

    public CalamityGlobalItem CalamityItem
    {
        get => field ?? Item?.Calamity();
        set;
    }

    public abstract int OverrideItemType { get; }


}

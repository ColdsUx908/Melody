using CalamityMod.NPCs.Crabulon;
using Transoceanic.Data.Publicizer;

namespace CalamityAnomalies.Publicizer.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record Crabulon_Publicizer(Crabulon Source) : PublicizerBase<Crabulon>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // stomping (instance field)
    public static readonly FieldInfo i_f_stomping = GetInstanceField("stomping");
    public bool stomping
    {
        get => (bool)i_f_stomping.GetValue(Source);
        set => i_f_stomping.SetValue(Source, value);
    }
}

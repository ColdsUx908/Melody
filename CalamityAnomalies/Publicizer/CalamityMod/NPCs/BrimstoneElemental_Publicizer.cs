using CalamityMod.NPCs.BrimstoneElemental;
using Transoceanic.Publicizer;

namespace CalamityAnomalies.Publicizer.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record Brimling_Publicizer(Brimling Source) : PublicizerBase<Brimling>(Source)
{
    // boostDR (instance field)
    public static readonly FieldInfo i_f_boostDR = GetInstanceField("boostDR");
    public bool boostDR
    {
        get => (bool)i_f_boostDR.GetValue(Source);
        set => i_f_boostDR.SetValue(Source, value);
    }
}

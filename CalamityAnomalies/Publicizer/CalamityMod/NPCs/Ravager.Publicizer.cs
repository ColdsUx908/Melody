using CalamityMod.NPCs.Ravager;
using Transoceanic.Data.Publicizer;

namespace CalamityAnomalies.Publicizer.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record RavagerBody_Publicizer(RavagerBody Source) : PublicizerBase<RavagerBody>(Source)
{
    // velocityY (instance field)
    public static readonly FieldInfo i_f_velocityY = GetInstanceField("velocityY");
    public float velocityY
    {
        get => (float)i_f_velocityY.GetValue(Source);
        set => i_f_velocityY.SetValue(Source, value);
    }
}

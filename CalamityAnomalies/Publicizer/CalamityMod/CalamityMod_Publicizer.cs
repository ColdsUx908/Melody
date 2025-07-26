using System.Diagnostics;
using Transoceanic.Publicizer;

namespace CalamityAnomalies.Publicizer.CalamityMod;

#pragma warning disable IDE1006

public record CalamityMod_Publicizer(CalamityMod_ Source) : PublicizerBase<CalamityMod_>(Source)
{
    // Instance (static field)
    public static readonly FieldInfo s_f_Instance = GetStaticField("Instance");

    public static CalamityMod_ Instance
    {
        get => (CalamityMod_)s_f_Instance.GetValue(null);
        set => s_f_Instance.SetValue(null, value);
    }

    // SpeedrunTimer (static field)
    public static readonly FieldInfo s_f_SpeedrunTimer = GetStaticField("SpeedrunTimer");

    public static Stopwatch SpeedrunTimer
    {
        get => (Stopwatch)s_f_SpeedrunTimer.GetValue(null);
        set => s_f_SpeedrunTimer.SetValue(null, value);
    }
}

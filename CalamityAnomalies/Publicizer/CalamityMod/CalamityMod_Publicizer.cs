namespace CalamityAnomalies.Publicizer.CalamityMod;

#pragma warning disable IDE1006
public record CalamityMod_Publicizer
{
    public static readonly Type c_type = typeof(CalamityMod_);

    public static readonly FieldInfo s_f_Instance = c_type.GetField("Instance", TOReflectionUtils.StaticBindingFlags);

    public static CalamityMod_ Instance
    {
        get => (CalamityMod_)s_f_Instance.GetValue(null);
        set => s_f_Instance.SetValue(null, value);
    }
}

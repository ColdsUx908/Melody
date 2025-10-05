using Transoceanic.Data.Publicizer;

namespace CalamityAnomalies.Publicizer.CalamityMod;

public record TrueMeleeDamageClass_Publicizer : Publicizer<TrueMeleeDamageClass>
{
    // Instance (static field)
    public static readonly FieldInfo s_f_Instance = GetStaticField("Instance");
    public static TrueMeleeDamageClass Instance
    {
        get => (TrueMeleeDamageClass)s_f_Instance.GetValue(null);
        set => s_f_Instance.SetValue(null, value);
    }
}

public record TrueMeleeNoSpeedDamageClass_Publicizer : Publicizer<TrueMeleeNoSpeedDamageClass>
{
    // Instance (static field)
    public static readonly FieldInfo s_f_Instance = GetStaticField("Instance");
    public static TrueMeleeNoSpeedDamageClass Instance
    {
        get => (TrueMeleeNoSpeedDamageClass)s_f_Instance.GetValue(null);
        set => s_f_Instance.SetValue(null, value);
    }
}
using Transoceanic.Data.Publicizer;

namespace CalamityAnomalies.Publicizer.CalamityMod;

#pragma warning disable IDE1006
public record NPCStats_EnemyStats_Publicizer : IPublicizer
{
    public static Type C_type => CAUtils.GetCalamityTypeByFullName("CalamityMod.NPCStats+EnemyStats");

    // ExpertDamageMultiplier (static field)
    public static readonly FieldInfo s_f_ExpertDamageMultiplier = PublicizerHelper.GetStaticField(C_type, "ExpertDamageMultiplier");
    public static SortedDictionary<int, double> ExpertDamageMultiplier
    {
        get => (SortedDictionary<int, double>)s_f_ExpertDamageMultiplier.GetValue(null);
        set => s_f_ExpertDamageMultiplier.SetValue(null, value);
    }

    // ContactDamageValues (static field)
    public static readonly FieldInfo s_f_ContactDamageValues = PublicizerHelper.GetStaticField(C_type, "ContactDamageValues");
    public static SortedDictionary<int, int[]> ContactDamageValues
    {
        get => (SortedDictionary<int, int[]>)s_f_ContactDamageValues.GetValue(null);
        set => s_f_ContactDamageValues.SetValue(null, value);
    }

    // ProjectileDamageValues (static field)
    public static readonly FieldInfo s_f_ProjectileDamageValues = PublicizerHelper.GetStaticField(C_type, "ProjectileDamageValues");
    public static SortedDictionary<Tuple<int, int>, int[]> ProjectileDamageValues
    {
        get => (SortedDictionary<Tuple<int, int>, int[]>)s_f_ProjectileDamageValues.GetValue(null);
        set => s_f_ProjectileDamageValues.SetValue(null, value);
    }
}

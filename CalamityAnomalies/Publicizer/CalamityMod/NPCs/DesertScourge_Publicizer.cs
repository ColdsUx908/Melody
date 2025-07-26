using CalamityMod.NPCs.DesertScourge;
using Transoceanic.Publicizer;

namespace CalamityAnomalies.Publicizer.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record DesertScourgeHead_Publicizer(DesertScourgeHead Source) : PublicizerBase<DesertScourgeHead>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // tailSpawned (instance field)
    public static readonly FieldInfo i_f_tailSpawned = GetInstanceField("tailSpawned");
    public bool tailSpawned
    {
        get => (bool)i_f_tailSpawned.GetValue(Source);
        set => i_f_tailSpawned.SetValue(Source, value);
    }

    // OpenMouthFrame (const)
    public const int OpenMouthStopFrame = 4;
}

public record DesertNuisanceHead_Publicizer(DesertNuisanceHead Source) : PublicizerBase<DesertNuisanceHead>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // tailSpawned (instance field)
    public static readonly FieldInfo i_f_tailSpawned = GetInstanceField("tailSpawned");
    public bool tailSpawned
    {
        get => (bool)i_f_tailSpawned.GetValue(Source);
        set => i_f_tailSpawned.SetValue(Source, value);
    }

    // OpenMouthFrame (const)
    public const int OpenMouthStopFrame = 4;
}

public record DesertNuisanceHeadYoung_Publicizer(DesertNuisanceHeadYoung Source) : PublicizerBase<DesertNuisanceHeadYoung>(Source)
{
    // biomeEnrageTimer (instance field)
    public static readonly FieldInfo i_f_biomeEnrageTimer = GetInstanceField("biomeEnrageTimer");
    public int biomeEnrageTimer
    {
        get => (int)i_f_biomeEnrageTimer.GetValue(Source);
        set => i_f_biomeEnrageTimer.SetValue(Source, value);
    }

    // tailSpawned (instance field)
    public static readonly FieldInfo i_f_tailSpawned = GetInstanceField("tailSpawned");
    public bool tailSpawned
    {
        get => (bool)i_f_tailSpawned.GetValue(Source);
        set => i_f_tailSpawned.SetValue(Source, value);
    }

    // OpenMouthFrame (const)
    public const int OpenMouthStopFrame = 4;
}
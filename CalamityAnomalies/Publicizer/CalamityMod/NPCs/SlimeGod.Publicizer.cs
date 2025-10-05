using CalamityMod.NPCs.SlimeGod;
using Transoceanic.Data.Publicizer;

namespace CalamityAnomalies.Publicizer.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record SlimeGodCore_Publicizer(SlimeGodCore Source) : PublicizerBase<SlimeGodCore>(Source)
{
    // slimesSpawned (instance field)
    public static readonly FieldInfo i_f_slimesSpawned = GetInstanceField("slimesSpawned");
    public bool slimesSpawned
    {
        get => (bool)i_f_slimesSpawned.GetValue(Source);
        set => i_f_slimesSpawned.SetValue(Source, value);
    }

    // buffedSlime (instance field)
    public static readonly FieldInfo i_f_buffedSlime = GetInstanceField("buffedSlime");
    public int buffedSlime
    {
        get => (int)i_f_buffedSlime.GetValue(Source);
        set => i_f_buffedSlime.SetValue(Source, value);
    }
}

public record CrimulanPaladin_Publicizer(CrimulanPaladin Source) : PublicizerBase<CrimulanPaladin>(Source)
{
    // bossLife (instance field)
    public static readonly FieldInfo i_f_bossLife = GetInstanceField("bossLife");
    public float bossLife
    {
        get => (float)i_f_bossLife.GetValue(Source);
        set => i_f_bossLife.SetValue(Source, value);
    }
}

public record EbonianPaladin_Publicizer(EbonianPaladin Source) : PublicizerBase<EbonianPaladin>(Source)
{
    // bossLife (instance field)
    public static readonly FieldInfo i_f_bossLife = GetInstanceField("bossLife");
    public float bossLife
    {
        get => (float)i_f_bossLife.GetValue(Source);
        set => i_f_bossLife.SetValue(Source, value);
    }
}

public record SplitCrimulanPaladin_Publicizer(SplitCrimulanPaladin Source) : PublicizerBase<SplitCrimulanPaladin>(Source)
{
    // bossLife (instance field)
    public static readonly FieldInfo i_f_bossLife = GetInstanceField("bossLife");
    public float bossLife
    {
        get => (float)i_f_bossLife.GetValue(Source);
        set => i_f_bossLife.SetValue(Source, value);
    }
}

public record SplitEbonianPaladin_Publicizer(SplitEbonianPaladin Source) : PublicizerBase<SplitEbonianPaladin>(Source)
{
    // bossLife (instance field)
    public static readonly FieldInfo i_f_bossLife = GetInstanceField("bossLife");
    public float bossLife
    {
        get => (float)i_f_bossLife.GetValue(Source);
        set => i_f_bossLife.SetValue(Source, value);
    }
}

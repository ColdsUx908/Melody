namespace CalamityAnomalies.GlobalInstances;

public abstract class CalamityPlayerDetour : ModPlayerDetour<CalamityPlayer>
{
    // MiscEffects
    public delegate void Orig_MiscEffects(CalamityPlayer self);
    public virtual void Detour_MiscEffects(Orig_MiscEffects orig, CalamityPlayer self) { }

    // OtherBuffEffects
    public delegate void Orig_OtherBuffEffects(CalamityPlayer self);
    public virtual void Detour_OtherBuffEffects(Orig_OtherBuffEffects orig, CalamityPlayer self) { }

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_MiscEffects);
        TryApplyDetour(Detour_OtherBuffEffects);
    }
}
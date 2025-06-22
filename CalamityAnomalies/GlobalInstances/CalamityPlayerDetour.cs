namespace CalamityAnomalies.GlobalInstances;

public abstract class CalamityPlayerDetour : ModPlayerDetour<CalamityPlayer>
{
    public delegate void Orig_OtherBuffEffects(CalamityPlayer self);

    public virtual void Detour_OtherBuffEffects(Orig_OtherBuffEffects orig, CalamityPlayer self) { }

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_OtherBuffEffects, flags: BindingFlags.NonPublic | BindingFlags.Instance);
    }
}
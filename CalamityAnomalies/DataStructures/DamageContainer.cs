using Transoceanic;

namespace CalamityAnomalies.DataStructures;

public readonly record struct ProjectileDamageContainer(
    int NormalDamage, int ExpertDamage, int MasterDamage, int LegendaryDamage,
    int AnomalyDamage, int AnomalyUltramundaneDamage)
{
    public const float ExpertContactVanillaMultiplier = 2f;
    public const float MasterContactVanillaMultiplier = 3f;
    public const float NormalProjectileVanillaMultiplier = 2f;
    public const float ExpertProjectileVanillaMultiplier = 4f;
    public const float MasterProjectileVanillaMultiplier = 6f;

    public int Damage
    {
        get
        {
            float damageAdjustment = Main.masterMode ? MasterProjectileVanillaMultiplier : Main.expertMode ? ExpertProjectileVanillaMultiplier : NormalProjectileVanillaMultiplier;

            if (CASharedData.AnomalyUltramundane)
                return (int)MathF.Round(AnomalyUltramundaneDamage / damageAdjustment);
            if (CASharedData.Anomaly)
                return (int)MathF.Round(AnomalyDamage / damageAdjustment);
            if (TOSharedData.LegendaryMode)
                return (int)MathF.Round(LegendaryDamage / damageAdjustment);
            if (Main.masterMode)
                return (int)MathF.Round(MasterDamage / damageAdjustment);
            if (Main.expertMode)
                return (int)MathF.Round(ExpertDamage / damageAdjustment);

            return (int)MathF.Round(NormalDamage / damageAdjustment);
        }
    }
}
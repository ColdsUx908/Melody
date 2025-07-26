using CalamityMod.NPCs.NormalNPCs;
using Transoceanic.Publicizer;

namespace CalamityAnomalies.Publicizer.CalamityMod.NPCs;

#pragma warning disable IDE1006

public record KingSlimeJewelRuby_Publicizer : Publicizer<KingSlimeJewelRuby>
{
    // BoltShootGateValue (const)
    public const int BoltShootGateValue = 60;

    // BoltShootGateValue_Death (const)
    public const int BoltShootGateValue_Death = 75;

    // BoltShootGateValue_BossRush (const)
    public const int BoltShootGateValue_BossRush = 45;

    // LightTelegraphDuration (const)
    public const float LightTelegraphDuration = 45f;
}

public record SkeletronPrime2_Publicizer(SkeletronPrime2 Source) : PublicizerBase<SkeletronPrime2>(Source)
{
    // CreateParticles (instance method)
    public static readonly MethodInfo i_m_CreateParticles = GetInstanceMethod("CreateParticles");
    public delegate void Orig_CreateParticles(SkeletronPrime2 self, NPC npc, Vector2 position, int amountMultiplier = 1);
    public static readonly Orig_CreateParticles i_d_CreateParticles = i_m_CreateParticles.CreateDelegate<Orig_CreateParticles>();
    public void CreateParticles(NPC npc, Vector2 position, int amountMultiplier = 1) => i_d_CreateParticles(Source, npc, position, amountMultiplier);
}

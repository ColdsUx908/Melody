using System.Reflection;
using Terraria;
using Transoceanic.Core;
using Transoceanic.Core.ExtraGameData;

namespace CalamityAnomalies.Override;

public sealed class CAOverrideHelper : ITOLoader
{
    internal static EntityOverrideDictionary<NPC, CANPCOverride> NPCOverrides { get; } = [];

    internal static EntityOverrideDictionary<Projectile, CAProjectileOverride> ProjectileOverrides { get; } = [];

    internal static EntityOverrideDictionary<Item, CAItemOverride> ItemOverrides { get; } = [];

    void ITOLoader.PostSetupContent()
    {
        Assembly assembly = CAMain.Assembly;
        NPCOverrides.FillOverrides(assembly);
        ProjectileOverrides.FillOverrides(assembly);
        ItemOverrides.FillOverrides(assembly);
    }

    void ITOLoader.OnModUnload()
    {
        NPCOverrides.Clear();
        ProjectileOverrides.Clear();
        ItemOverrides.Clear();
    }
}

public static class CAOverrideExtensions
{
    public static bool HasNPCOverride(this NPC npc, out CANPCOverride npcOverride) => CAOverrideHelper.NPCOverrides.TryGetValue(npc, out npcOverride);

    public static bool HasProjectileOverride(this Projectile projectile, out CAProjectileOverride projectileOverride) => CAOverrideHelper.ProjectileOverrides.TryGetValue(projectile, out projectileOverride);

    public static bool HasItemOverride(this Item item, out CAItemOverride itemOverride) => CAOverrideHelper.ItemOverrides.TryGetValue(item, out itemOverride);
}

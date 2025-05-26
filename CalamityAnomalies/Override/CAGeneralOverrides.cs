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
    public static bool TryGetOverride(this NPC npc, out CANPCOverride npcOverride) => CAOverrideHelper.NPCOverrides.TryGetOverride(npc, out npcOverride);

    public static bool TryGetOverride(this Projectile projectile, out CAProjectileOverride projectileOverride) => CAOverrideHelper.ProjectileOverrides.TryGetOverride(projectile, out projectileOverride);

    public static bool TryGetOverride(this Item item, out CAItemOverride itemOverride) => CAOverrideHelper.ItemOverrides.TryGetOverride(item, out itemOverride);
}

using Transoceanic.RuntimeEditing;

namespace Transoceanic.Data;

public abstract class ModDOT : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public abstract bool HasBuff(Player player);

    public abstract bool HasBuff(NPC npc);

    public virtual float GetDamage(Player player) => 0f;

    public virtual float GetDamage(NPC npc) => 0f;

    public virtual int GetDamageValue(NPC npc) => 0;
}

public sealed class ModDOTHandler : IResourceLoader
{
    internal static List<ModDOT> ModDOTSet { get; } = [];

    internal static Dictionary<int, (Predicate<Player> hasBuffPlayer, Predicate<NPC> hasBuffNPC, Func<Player, float> damagePlayer, Func<NPC, float>, Func<NPC, int> damageValue)> ExternalDOTSet { get; } = [];

    public static void RegisterDOT(int type, Predicate<Player> hasBuffPlayer, Predicate<NPC> hasBuffNPC, Func<Player, float> damagePlayer = null, Func<NPC, float> damageNPC = null, Func<NPC, int> damageValue = null, bool cover = true)
    {
        hasBuffPlayer ??= _ => false;
        hasBuffNPC ??= _ => false;
        damagePlayer ??= _ => 0f;
        damageNPC ??= _ => 0f;
        damageValue ??= _ => 0;
        if (cover || !ExternalDOTSet.ContainsKey(type))
            ExternalDOTSet[type] = (hasBuffPlayer, hasBuffNPC, damagePlayer, damageNPC, damageValue);
    }

    void IResourceLoader.PostSetupContent() => ModDOTSet.AddRange(TOReflectionUtils.GetTypeInstancesDerivedFrom<ModDOT>());

    void IResourceLoader.OnModUnload()
    {
        ModDOTSet.Clear();
        ExternalDOTSet.Clear();
    }
}

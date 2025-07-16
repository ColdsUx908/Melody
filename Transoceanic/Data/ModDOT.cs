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

public sealed class ModDOTHelper : IResourceLoader
{
    internal static List<ModDOT> ModDOTSet { get; } = [];

    void IResourceLoader.PostSetupContent() => ModDOTSet.AddRange(TOReflectionUtils.GetTypeInstancesDerivedFrom<ModDOT>());

    void IResourceLoader.OnModUnload() => ModDOTSet.Clear();
}

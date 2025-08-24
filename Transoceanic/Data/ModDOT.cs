﻿using Transoceanic.RuntimeEditing;

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
    internal static readonly List<ModDOT> ModDOTSet = [];
    internal static readonly Dictionary<int, (Predicate<Player> hasBuffPlayer, Predicate<NPC> hasBuffNPC, Func<Player, float> damagePlayer, Func<NPC, float>, Func<NPC, int> damageValue)> ExternalDOTSet = [];

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

    public static void RegisterDOT<T>(Predicate<Player> hasBuffPlayer, Predicate<NPC> hasBuffNPC, Func<Player, float> damagePlayer = null, Func<NPC, float> damageNPC = null, Func<NPC, int> damageValue = null, bool cover = true) where T : ModBuff =>
        RegisterDOT(ModContent.BuffType<T>(), hasBuffPlayer, hasBuffNPC, damagePlayer, damageNPC, damageValue, cover);

    void IResourceLoader.PostSetupContent() => ModDOTSet.AddRange(TOReflectionUtils.GetTypeInstancesDerivedFrom<ModDOT>());

    void IResourceLoader.OnModUnload()
    {
        ModDOTSet.Clear();
        ExternalDOTSet.Clear();
    }
}

public sealed class ModDOT_Player : TOPlayerBehavior
{
    public override void UpdateBadLifeRegen()
    {
        float totalDOT = 0f;
        foreach (ModDOT dot in ModDOTHandler.ModDOTSet)
        {
            if (dot.HasBuff(Player))
                totalDOT += dot.GetDamage(Player);
        }
        foreach ((Predicate<Player> hasBuffPlayer, _, Func<Player, float> damagePlayer, _, _) in ModDOTHandler.ExternalDOTSet.Values)
        {
            if (hasBuffPlayer(Player))
                totalDOT += damagePlayer(Player);
        }

        Player.lifeRegen -= (int)totalDOT;
    }
}

public sealed class ModDOT_GlobalNPC : TOGlobalNPCBehavior
{
    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        float totalDOT = 0f;
        int finalDamageValue = 0;
        foreach (ModDOT dot in ModDOTHandler.ModDOTSet)
        {
            if (dot.HasBuff(npc))
                ApplyDOT(dot.GetDamage(npc), dot.GetDamageValue(npc));
        }
        foreach ((_, Predicate<NPC> hasBuffNPC, _, Func<NPC, float> damageNPC, Func<NPC, int> damageValue) in ModDOTHandler.ExternalDOTSet.Values)
        {
            if (hasBuffNPC(npc))
                ApplyDOT(damageNPC(npc), damageValue(npc));
        }
        npc.ApplyDOT((int)totalDOT, finalDamageValue, ref damage);

        void ApplyDOT(float dot, int damageValue)
        {
            totalDOT += dot * 2f;
            finalDamageValue = Math.Max(finalDamageValue, damageValue);
        }
    }
}

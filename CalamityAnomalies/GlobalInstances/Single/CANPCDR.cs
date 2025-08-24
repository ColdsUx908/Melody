using CalamityMod.Events;
using CalamityMod.NPCs.Providence;

namespace CalamityAnomalies.GlobalInstances.Single;

public sealed class CANPCDR : CAGlobalNPCBehavior2, IResourceLoader
{
    public delegate void Orig_ApplyDR(CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers);

    /// <summary>
    /// 禁用灾厄的DR机制。
    /// </summary>
    public static void Detour_ApplyDR(Orig_ApplyDR orig, CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers) { }

    public delegate float Orig_DRMath(CalamityGlobalNPC self, NPC npc, float DR);
    public static Orig_DRMath OrigMethod_CustomDRMath { get; private set; }
    public static Orig_DRMath OrigMethod_DefaultDRMath { get; private set; }

    public static float GetBaseDR(NPC npc)
    {
        CalamityGlobalNPC calamityNPC = npc.Calamity();
        return calamityNPC.unbreakableDR ? calamityNPC.DR : (calamityNPC.customDR ? OrigMethod_CustomDRMath : OrigMethod_DefaultDRMath).Invoke(calamityNPC, npc, calamityNPC.DR);
    }

    public static float GetTimedDR(NPC npc, float baseDR)
    {
        float timedDR = 0f;
        CalamityGlobalNPC calamityNPC = npc.Calamity();
        int killTime = calamityNPC.KillTime;
        int aiTimer = calamityNPC.AITimer;

        bool isNightProvidence = npc.ModNPC is Providence && !Main.IsItDay();
        bool isDayEmpress = npc.type == NPCID.HallowBoss && NPC.ShouldEmpressBeEnraged();

        if (killTime > 0 && aiTimer < killTime && !BossRushEvent.BossRushActive && (isNightProvidence || isDayEmpress))
        {
            const float tdrFactor = 10f;
            float extraDRLimit = (1f - baseDR) * tdrFactor / 2f;
            float lifeCompletion = npc.Ocean().LifeRatioReverse;
            float timeCompletion = (float)aiTimer / killTime;
            float timedDRStrength = lifeCompletion - timeCompletion;
            if (timedDRStrength > 0f)
                timedDR = extraDRLimit * timedDRStrength / (1 + timedDRStrength);
        }

        return timedDR;
    }

    public override decimal Priority => 100m;

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);
        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();
        foreach (CAGlobalItemBehavior behavior in GlobalItemBehaviorHandler.BehaviorSet.GetBehaviors<CAGlobalItemBehavior>(nameof(CAGlobalItemBehavior.ModifyHitNPC_DR)))
            behavior.ModifyHitNPC_DR(item, npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (item.TryGetBehavior(out CASingleItemBehavior itemBehavior, nameof(CASingleItemBehavior.ModifyHitNPC_DR)))
            itemBehavior.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (item.ModItem is ICAModItem caModItem)
            caModItem.ModifyHitNPC_DR(npc, player, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR));
        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR, 0f, 1f);
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        float baseDR = GetBaseDR(npc);
        StatModifier baseDRModifier = new();
        StatModifier standardDRModifier = new();
        StatModifier timedDRModifier = new();
        foreach (CAGlobalProjectileBehavior behavior in GlobalProjectileBehaviorHandler.BehaviorSet.GetBehaviors<CAGlobalProjectileBehavior>(nameof(CAGlobalProjectileBehavior.ModifyHitNPC_DR)))
            behavior.ModifyHitNPC_DR(projectile, npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.ModifyHitNPC_DR)))
            projectileBehavior.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        if (projectile.ModProjectile is ICAModProjectile caModProjectile)
            caModProjectile.ModifyHitNPC_DR(npc, ref modifiers, baseDR, ref baseDRModifier, ref standardDRModifier, ref timedDRModifier);
        baseDR = baseDRModifier.ApplyTo(baseDR);
        float standardDR = standardDRModifier.ApplyTo(baseDR);
        float timedDR = timedDRModifier.ApplyTo(GetTimedDR(npc, baseDR));
        modifiers.FinalDamage *= Math.Clamp(1f - standardDR - timedDR, 0f, 1f);
    }

    void IResourceLoader.PostSetupContent()
    {
        Type type = typeof(CalamityGlobalNPC);
        if (CAServerConfig.Instance.Contents)
            TODetourUtils.Modify(type, "ApplyDR", Detour_ApplyDR);
        OrigMethod_CustomDRMath = type.GetMethod("CustomDRMath", TOReflectionUtils.UniversalBindingFlags).CreateDelegate<Orig_DRMath>();
        OrigMethod_DefaultDRMath = type.GetMethod("DefaultDRMath", TOReflectionUtils.UniversalBindingFlags).CreateDelegate<Orig_DRMath>();
    }

    void IResourceLoader.OnModUnload()
    {
        OrigMethod_CustomDRMath = null;
        OrigMethod_DefaultDRMath = null;
    }
}

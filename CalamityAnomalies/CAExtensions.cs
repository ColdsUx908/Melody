﻿using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.StormWeaver;

namespace CalamityAnomalies;

public static class CAExtensions
{
    extension(Item item)
    {
        public CAGlobalItem Anomaly() => item.GetGlobalItem<CAGlobalItem>();

        public bool TryGetBehavior(out CAItemBehavior itemBehavior, [CallerMemberName] string methodName = null!) => CABehaviorHelper.ItemBehaviors.TryGetBehavior(item, methodName, out itemBehavior);
    }

    extension(NPC npc)
    {
        public CAGlobalNPC Anomaly() => npc.GetGlobalNPC<CAGlobalNPC>();

        public bool TryGetBehavior(out CANPCBehavior npcBehavior, [CallerMemberName] string methodName = null!) => CABehaviorHelper.NPCBehaviors.TryGetBehavior(npc, methodName, out npcBehavior);

        public bool DesertScourge => npc.ModNPC is DesertScourgeHead or DesertScourgeBody or DesertScourgeTail;

        public bool DesertNuisance => npc.ModNPC is DesertNuisanceHead or DesertNuisanceBody or DesertNuisanceTail;

        public bool DesertNuisanceYoung => npc.ModNPC is DesertNuisanceHeadYoung or DesertNuisanceBodyYoung or DesertNuisanceTailYoung;

        public bool AquaticScourge => npc.ModNPC is AquaticScourgeHead or AquaticScourgeBody or AquaticScourgeTail;

        public bool LeviathanBoss => npc.ModNPC is Leviathan or Anahita;

        public bool Ravager => npc.ModNPC is RavagerBody or RavagerClawLeft or RavagerClawRight or RavagerLegLeft or RavagerLegRight or RavagerHead or RavagerHead2;

        public bool ProfanedGuardianBoss => npc.ModNPC is ProfanedGuardianCommander or ProfanedGuardianDefender or ProfanedGuardianHealer;

        public bool ProfanedGuardianSpawned => npc.ModNPC is ProvSpawnOffense or ProvSpawnDefense or ProvSpawnHealer;

        public bool StormWeaver => npc.ModNPC is StormWeaverHead or StormWeaverBody or StormWeaverTail;

        public bool DoG => npc.ModNPC is DevourerofGodsHead or DevourerofGodsBody or DevourerofGodsTail;

        public bool CosmicGuardian => npc.ModNPC is CosmicGuardianHead or CosmicGuardianBody or CosmicGuardianTail;

        public bool Thanatos => npc.active && npc.ModNPC is ThanatosHead or ThanatosBody1 or ThanatosBody2 or ThanatosTail;

        public bool ThanatosHead => npc.ModNPC is ThanatosHead;

        public bool ExoTwins => npc.ModNPC is Artemis or Apollo;

        public bool Ares => npc.ModNPC is AresLaserCannon or AresTeslaCannon or AresGaussNuke or AresPlasmaFlamethrower;

        public bool ExoMechs => npc.Thanatos || npc.ExoTwins || npc.Ares;

        public void ApplyCalamityBossHealthBoost() => npc.lifeMax += (int)(npc.lifeMax * CalamityConfig.Instance.BossHealthBoost * 0.01);

        public int GetProjectileDamage<T>() where T : ModProjectile => npc.GetProjectileDamage(ModContent.ProjectileType<T>());
    }

    extension(Player player)
    {
        public CAPlayer Anomaly() => player.GetModPlayer<CAPlayer>();

        public NewCalamityPlayer NewCalamity() => player.GetModPlayer<NewCalamityPlayer>();
    }

    extension(Projectile projectile)
    {
        public CAGlobalProjectile Anomaly() => projectile.GetGlobalProjectile<CAGlobalProjectile>();

        public bool TryGetBehavior(out CAProjectileBehavior projectileBehavior, [CallerMemberName] string methodName = null!) => CABehaviorHelper.ProjectileBehaviors.TryGetBehavior(projectile, methodName, out projectileBehavior);
    }
}

using CalamityMod.Events;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Projectiles.Boss;
using Transoceanic.ExtraGameData;

namespace CalamityAnomalies.Tweaks._5_3_LastBoss;

public sealed class SupremeCalamitasDetour : ModNPCDetour<SupremeCalamitas>
{
    public override bool Detour_CheckDead(Orig_CheckDead orig, SupremeCalamitas self)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = true;
            bool result = orig(self);
            BossRushEvent.BossRushActive = temp;
            return result;
        }

        return orig(self);
    }
}

public sealed class SCalBrimstoneGigablastDetour : ModProjectileDetour<SCalBrimstoneGigablast>
{
    public override void Detour_OnKill(Orig_OnKill orig, SCalBrimstoneGigablast self, int timeLeft)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = temp;
            orig(self, timeLeft);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self, timeLeft);
    }
}

public sealed class SCalBrimstoneFireblastDetour : ModProjectileDetour<SCalBrimstoneFireblast>
{
    public override void Detour_OnKill(Orig_OnKill orig, SCalBrimstoneFireblast self, int timeLeft)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = temp;
            orig(self, timeLeft);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self, timeLeft);
    }
}

using CalamityMod.Events;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Projectiles.Boss;
using Transoceanic.ExtraGameData;

namespace CalamityAnomalies.Tweaks._5_3_LastBoss;

public class SupremeCalamitasDetour : ModNPCDetour<SupremeCalamitas>
{
    public override bool Detour_CheckDead(Orig_CheckDead orig, SupremeCalamitas self)
    {
        if (CAWorld.BossRush && self.giveUpCounter <= 0)
            return true;

        return orig(self);
    }

    public override void Detour_OnKill(Orig_OnKill orig, SupremeCalamitas self)
    {
        if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = CAWorld.RealBossRushEventActive;
            orig(self);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self);
    }
}

public class SCalBrimstoneGigablastDetour : ModProjectileDetour<SCalBrimstoneGigablast>
{
    public override void Detour_OnKill(Orig_OnKill orig, SCalBrimstoneGigablast self, int timeLeft)
    {
        if (CAWorld.Anomaly)
        {

        }
        else if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = true;
            orig(self, timeLeft);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self, timeLeft);
    }
}

public class SCalBrimstoneFireblastDetour : ModProjectileDetour<SCalBrimstoneFireblast>
{
    public override void Detour_OnKill(Orig_OnKill orig, SCalBrimstoneFireblast self, int timeLeft)
    {
        if (CAWorld.Anomaly)
        {

        }
        else if (CAWorld.BossRush)
        {
            bool temp = BossRushEvent.BossRushActive;
            BossRushEvent.BossRushActive = true;
            orig(self, timeLeft);
            BossRushEvent.BossRushActive = temp;
            return;
        }

        orig(self, timeLeft);
    }
}

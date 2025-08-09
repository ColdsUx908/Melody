namespace CalamityAnomalies.Tweaks._5_2_PostYharon;

/* 超位崩解
 * 
 * NPC具有此减益时视为对所有属性减益具有弱性。
 */

public sealed class MiracleBlight_GlobalNPC : CAGlobalNPCDetour<CalamityGlobalNPC>
{
    public override void Detour_UpdateLifeRegen(Orig_UpdateLifeRegen orig, CalamityGlobalNPC self, NPC npc, ref int damage)
    {
        if (self.miracleBlight > 0)
        {
            bool? vulnerableToHeat = self.VulnerableToHeat;
            bool? vulnerableToCold = self.VulnerableToCold;
            bool? vulnerableToSickness = self.VulnerableToSickness;
            bool? vulnerableToElectricity = self.VulnerableToElectricity;
            bool? vulnerableToWater = self.VulnerableToWater;
            self.VulnerableToHeat = true;
            self.VulnerableToCold = true;
            self.VulnerableToSickness = true;
            self.VulnerableToElectricity = true;
            self.VulnerableToWater = true;
            orig(self, npc, ref damage);
            self.VulnerableToHeat = vulnerableToHeat;
            self.VulnerableToCold = vulnerableToCold;
            self.VulnerableToSickness = vulnerableToSickness;
            self.VulnerableToElectricity = vulnerableToElectricity;
            self.VulnerableToWater = vulnerableToWater;
        }
    }
}

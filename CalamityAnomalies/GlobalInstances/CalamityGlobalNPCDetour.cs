namespace CalamityAnomalies.GlobalInstances;

public sealed class CalamityGlobalNPCDetour : GlobalNPCDetour<CalamityGlobalNPC>
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalNPC self, NPC npc)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreAI)))
        {
            if (!npcBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC.PreAI))
                return true;
        }

        return orig(self, npc);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalNPC self, NPC npc, Color drawColor)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.GetAlpha)))
        {
            if (!npcBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC.GetAlpha))
                return null;
        }

        if (npc.Anomaly().NeverTrippy)
        {
            CalamityPlayer calamityPlayer = Main.LocalPlayer.Calamity();
            bool temp = calamityPlayer.trippy;
            calamityPlayer.trippy = false;
            Color? result = orig(self, npc, drawColor);
            calamityPlayer.trippy = temp;
            return result;
        }

        return orig(self, npc, drawColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreDraw)))
        {
            if (npcBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC.PreDraw))
                return true;
        }

        return orig(self, npc, spriteBatch, screenPos, drawColor);
    }

    public delegate void Orig_ApplyDR(CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers);

    public static void Detour_ApplyDR(Orig_ApplyDR orig, CalamityGlobalNPC self, NPC npc, ref NPC.HitModifiers modifiers) { }

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_ApplyDR);
    }
}

namespace CalamityAnomalies.Buffs;

public sealed class DimensionalTorn : CalamityModDOT
{
    public override bool HasBuff(Player player) => player.Anomaly().Debuff_DimensionalTorn;

    public override bool HasBuff(NPC npc) => npc.Anomaly().Debuff_DimensionalTorn > 0;

    public override float GetDamageCalamity(Player player) => Interpolation(player) * 10f;

    public override float GetDamage(NPC npc) => Interpolation(npc) * 1000f;

    public override int GetDamageValue(NPC npc) => (int)(Interpolation(npc) * 400f);

    public override void Update(Player player, ref int buffIndex) => player.Anomaly().Debuff_DimensionalTorn = true;

    public override void Update(NPC npc, ref int buffIndex)
    {
        CAGlobalNPC anomalyNPC = npc.Anomaly();
        int buffTime = npc.buffTime[buffIndex];
        if (anomalyNPC.Debuff_DimensionalTorn < buffTime)
            anomalyNPC.Debuff_DimensionalTorn = buffTime;
        npc.DelBuff(buffIndex--);
    }

    /// <summary>
    /// 获取维度撕裂二次曲线插值。
    /// <br/>在实体速度为25（127mph）时达到最大值1。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Interpolation(Entity entity)
    {
        float velocityLength = Math.Clamp(entity.velocity.Length(), 0f, 25f);
        return velocityLength * (velocityLength * -0.0016f + 0.08f); //y = -0.0016x^2 + 0.08x
    }
}

public sealed class DimensionalTorn_Player : CAPlayerBehavior
{
    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (AnomalyPlayer.Debuff_DimensionalTorn)
            modifiers.FinalDamage *= DimensionalTorn.Interpolation(Player) * 0.15f + 1f;
    }
}

public sealed class DimensionalTorn_GlobalNPC : CAGlobalNPCBehavior
{
    public override void PostAI(NPC npc) => npc.Anomaly().Debuff_DimensionalTorn--;

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (npc.Anomaly().Debuff_DimensionalTorn > 0)
            modifiers.FinalDamage *= DimensionalTorn.Interpolation(npc) * 0.15f + 1f;
    }
}
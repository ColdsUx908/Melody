using CalamityMod.NPCs.NormalNPCs;

namespace CalamityAnomalies.Anomaly.KingSlime;

public class KingSlimeJewelEmerald_Anomaly : AnomalyNPCBehavior<KingSlimeJewelEmerald>
{
    #region 数据
    public enum Behavior
    {
        Despawn = -1,

        FollowTarget = 0,
        Charge = 1,
    }

    public const float DespawnDistance = 5000f;
    public int ChargeCooldownTime => HasEnteredPhase2 ? 210 : 150;
    public int ChargePreparationTime => HasEnteredPhase2 ? 75 : 60;
    public static int ChargeTime => 60;
    public float ChargeSpeed => HasEnteredPhase2 ? 24f : 28f;

    public Behavior CurrentAttack
    {
        get => (Behavior)(int)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    public int CurrentAttackPhase
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public bool HasEnteredPhase2
    {
        get => NPC.ai[2] == 1f;
        set => NPC.ai[2] = value.ToInt();
    }

    public bool CanAttack
    {
        get => NPC.ai[3] != 1f;
        set => NPC.ai[3] = (!value).ToInt();
    }
    #endregion 数据

    public override void SetDefaults()
    {
        NPC.lifeMax = (int)(NPC.lifeMax * 0.5f);
        NPC.width = 28;
        NPC.height = 28;
    }

    public override bool PreAI()
    {
        //如果找不到所属史莱姆王，直接脱战
        if (!OceanNPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            CurrentAttack = Behavior.Despawn;
            JewelHandler.Despawn(NPC);
            return false;
        }

        Lighting.AddLight(NPC.Center, 0f, 1f, 0f);

        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return false;
        }

        switch (CurrentAttack)
        {
            case Behavior.Despawn:
                JewelHandler.Despawn(NPC);
                return false;
            case Behavior.FollowTarget:
                FollowTarget();
                break;
            case Behavior.Charge:
                Charge();
                break;
        }

        NPC.netUpdate = true;

        return false;

        void FollowTarget()
        {
            JewelHandler.Movement(NPC, Target.Center, 15f, 12f, 0.2f, 350f, -350f, -200f, -400f);
            if (CanAttack)
                Timer1++;
            if (Timer1 >= ChargeCooldownTime)
            {
                Timer1 = 0;
                CurrentAttack = Behavior.Charge;
                NPC.netUpdate = true;
            }
        }

        void Charge()
        {
            NPC.knockBackResist = 0f;

            switch (CurrentAttackPhase)
            {
                case 0:
                    Timer1++;
                    if (Timer1 < ChargePreparationTime) //停止，旋转
                    {
                        NPC.damage = 0;
                        NPC.velocity *= 0.94f;
                        NPC.rotation += (0.1f + (float)Timer1 / ChargePreparationTime * 0.4f) * NPC.direction;
                    }
                    else //冲刺
                    {
                        Timer1 = 0;
                        for (int i = 0; i < 10; i++)
                            JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(4f, 7f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
                        SoundEngine.PlaySound(SoundID.Item38, NPC.Center);
                        NPC.damage = NPC.defDamage;
                        NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, ChargeSpeed), MathHelper.PiOver2);
                        NPC.netSpam = 0;
                        CurrentAttackPhase = 1;
                        NPC.netUpdate = true;
                    }
                    break;
                case 1: //冲刺中
                    Timer1++;
                    if (Timer1 >= ChargeTime)
                    {
                        Timer1 = 0;
                        for (int i = 0; i < 15; i++)
                            JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(2f, 3f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.4f, 0.7f));
                        NPC.damage = 0;
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                        CurrentAttackPhase = 0;
                        CurrentAttack = Behavior.FollowTarget;
                        NPC.velocity = Vector2.Zero;
                        NPC.netUpdate = true;
                    }
                    else
                        NPC.damage = NPC.defDamage;
                    break;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float timeLeftGateValue = 30f;
        float gateValue = ChargePreparationTime - timeLeftGateValue;
        float ratio = CurrentAttack == Behavior.Charge && CurrentAttackPhase == 0 && Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        if (CAClientConfig.Instance.AuxiliaryVisualEffects && ratio > 0f)
            JewelHandler.DrawAttackEffect(spriteBatch, screenPos, NPC, ratio, 120f, 0.35f);
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC, ratio);
        return false;
    }

    public override bool CheckDead()
    {
        if (CAWorld.AnomalyUltramundane)
        {
            NPC.life = 1;
            NPC.active = true;
            if (!HasEnteredPhase2)
                JewelHandler.EnterPhase2(NPC);
            return false;
        }
        return true;
    }
}
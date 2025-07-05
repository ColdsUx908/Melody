using CalamityMod.NPCs.NormalNPCs;

namespace CalamityAnomalies.NPCs.KingSlime;

public class AnomalyJewelEmerald : AnomalyNPCBehavior<KingSlimeJewelEmerald>
{
    #region 枚举、数值、属性
    public enum AttackType
    {
        Despawn = -1,

        FollowTarget = 0,
        Charge = 1,
    }

    public static class Data
    {
        public const float DespawnDistance = 5000f;

        public const int ChargePhaseGateValue = 90;

        public const int ChargeGateValue = 40;
    }

    public AttackType CurrentAttack
    {
        get => (AttackType)AnomalyNPC.AnomalyAI[0].i;
        set
        {
            int temp = (int)value;
            if (AnomalyNPC.AnomalyAI[0].i != temp)
            {
                AnomalyNPC.AnomalyAI[0].i = temp;
                AnomalyNPC.AIChanged[0] = true;
            }
        }
    }

    public int CurrentAttackPhase
    {
        get => AnomalyNPC.AnomalyAI[1].i;
        set
        {
            if (AnomalyNPC.AnomalyAI[1].i != value)
            {
                AnomalyNPC.AnomalyAI[1].i = value;
                AnomalyNPC.AIChanged[0] = true;
            }
        }
    }
    #endregion 枚举、数值、属性

    public override bool PreAI()
    {
        //如果找不到所属史莱姆王，直接脱战
        if (!OceanNPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            CurrentAttack = AttackType.Despawn;
            Despawn();
            return false;
        }

        Lighting.AddLight(NPC.Center, 0f, 0.8f, 0f);

        if (!NPC.TargetClosestIfInvalid(true, Data.DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return false;
        }

        switch (CurrentAttack)
        {
            case AttackType.Despawn:
                Despawn();
                return false;
            case AttackType.FollowTarget:
                FollowTarget();
                break;
            case AttackType.Charge:
                Charge();
                break;
        }

        NPC.netUpdate = true;

        return false;
    }

    private void MakeDust(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Dust.NewDustAction(NPC.Center, NPC.width, NPC.height, DustID.GemEmerald, d =>
            {
                d.velocity = NPC.SafeDirectionTo(Target.Center + Target.velocity * 20f, -Vector2.UnitY) * Main.rand.NextFloat(-4f, -1f) * Main.rand.NextFloat(1f, 2f);
                d.noGravity = true;
                if (Main.rand.NextBool())
                {
                    d.scale = 0.5f;
                    d.fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            });
        }
    }

    private void Despawn()
    {
        NPC.life = 0;
        NPC.HitEffect();
        NPC.active = false;
        NPC.netUpdate = true;
    }

    private void FollowTarget()
    {
        NPC.damage = 0;

        NPC.knockBackResist = 0.7f;

        NPC.rotation = NPC.velocity.X / 15f;

        float maxVelocityX = 8f;
        float maxVelocityY = 5f;
        float acceleration = 0.2f;

        switch (NPC.Center.X - Target.Center.X)
        {
            case > 200f:
                if (NPC.velocity.X > 0f)
                    NPC.velocity.X *= 0.98f;
                NPC.velocity.X = Math.Min(NPC.velocity.X - acceleration, maxVelocityX);
                break;
            case < -200f:
                if (NPC.velocity.X < 0f)
                    NPC.velocity.X *= 0.98f;
                NPC.velocity.X = Math.Max(NPC.velocity.X + acceleration, -maxVelocityX);
                break;
        }

        switch (NPC.Center.Y - Target.Center.Y)
        {
            case > -200f:
                if (NPC.velocity.Y > 0f)
                    NPC.velocity.Y *= 0.98f;
                NPC.velocity.Y = Math.Min(NPC.velocity.Y - acceleration, maxVelocityY);
                break;
            case < -300f:
                if (NPC.velocity.Y < 0f)
                    NPC.velocity.Y *= 0.98f;
                NPC.velocity.Y = Math.Max(NPC.velocity.Y + acceleration, -maxVelocityY);
                break;
        }

        Timer2++;
        if (Timer2 >= Data.ChargePhaseGateValue)
        {
            Timer2 = 0;
            CurrentAttack = AttackType.Charge;
            NPC.netUpdate = true;
        }
    }

    private void Charge()
    {
        NPC.knockBackResist = 0f;

        switch (CurrentAttackPhase)
        {
            case 0:
                Timer1++;
                if (Timer1 < Data.ChargeGateValue) //停止，旋转
                {
                    NPC.damage = 0;
                    NPC.velocity *= 0.94f;
                    NPC.rotation += (0.1f + Timer1 / Data.ChargeGateValue * 0.4f) * NPC.direction;
                }
                else //冲刺
                {
                    MakeDust(10);
                    SoundEngine.PlaySound(SoundID.Item38, NPC.Center);
                    NPC.damage = NPC.defDamage;
                    float chargeSpeed = 24f;
                    NPC.SetVelocityandRotation(NPC.SafeDirectionTo(Target.Center + Target.velocity * 20f, -Vector2.UnitY) * chargeSpeed, MathHelper.PiOver2);
                    CurrentAttackPhase = 2;
                    Timer1 = 0;
                    NPC.netSpam = 0;
                    CurrentAttackPhase = 1;
                    Timer1 = 0;
                    NPC.netUpdate = true;
                }
                break;
            case 1: //冲刺中
                Timer1++;
                if (Timer1 < Data.ChargeGateValue)
                    NPC.damage = NPC.defDamage;
                else
                {
                    NPC.damage = 0;
                    MakeDust(10);
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                    CurrentAttackPhase = 0;
                    Timer1 = 0;
                    CurrentAttack = AttackType.FollowTarget;
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                }
                break;
        }
    }
}

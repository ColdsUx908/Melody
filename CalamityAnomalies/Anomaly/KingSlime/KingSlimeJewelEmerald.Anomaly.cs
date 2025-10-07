using CalamityAnomalies.Assets.Textures;
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

    public static class Data
    {
        public const float DespawnDistance = 5000f;
        public static int ChargeCooldownTime => 150;
        public static int ChargePreparationTime => 60;
        public static int ChargeTime => 60;
        public static float ChargeSpeed => 28f;
    }

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
    #endregion 数据

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

        if (!NPC.TargetClosestIfInvalid(true, Data.DespawnDistance))
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
            JewelHandler.Movement(NPC, Target.Center, 10f, 7.5f, 0.2f, 200f, -200f, -200f, -300f);
            Timer1++;
            if (Timer1 >= Data.ChargeCooldownTime)
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
                    if (Timer1 >= Data.ChargePreparationTime) //停止，旋转
                    {
                        Timer1 = 0;
                        for (int i = 0; i < 10; i++)
                            JewelHandler.SpawnParticle(NPC, Main.rand.NextFloat(4f, 7f), Main.rand.Next(30, 45), Main.rand.NextFloat(0.4f, 0.7f));
                        SoundEngine.PlaySound(SoundID.Item38, NPC.Center);
                        NPC.damage = NPC.defDamage;
                        NPC.SetVelocityandRotation(NPC.GetVelocityTowards(Target, Data.ChargeSpeed), MathHelper.PiOver2);
                        NPC.netSpam = 0;
                        CurrentAttackPhase = 1;
                        NPC.netUpdate = true;
                    }
                    else //冲刺
                    {
                        NPC.damage = 0;
                        NPC.velocity *= 0.94f;
                        NPC.rotation += (0.1f + (float)Timer1 / Data.ChargePreparationTime * 0.4f) * NPC.direction;
                    }
                    break;
                case 1: //冲刺中
                    Timer1++;
                    if (Timer1 >= Data.ChargeTime)
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
        float gateValue = Data.ChargePreparationTime - timeLeftGateValue;
        float ratio = CurrentAttack == Behavior.Charge && CurrentAttackPhase == 0 && Timer1 > gateValue ? (Timer1 - gateValue) / timeLeftGateValue : 0f;
        if (CAClientConfig.Instance.AuxiliaryVisualEffects && ratio > 0f)
            JewelHandler.DrawAttackEffect(spriteBatch, screenPos, NPC, ratio, 120f, 0.35f);
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC, ratio);
        return false;
    }
}
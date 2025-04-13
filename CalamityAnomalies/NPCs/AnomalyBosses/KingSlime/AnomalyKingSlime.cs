using CalamityAnomalies.Systems;
using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic;

namespace CalamityAnomalies.NPCs.AnomalyBosses.KingSlime;

public partial class AnomalyKingSlime : NPCOverride
{
    #region 枚举、常量、属性
    public enum AttackType
    {
        Despawn = -1,

        NormalJump_Phase1 = 1,
        HighJump_Phase1 = 2,
        RapidJump_Phase1 = 3,
        Teleport_Phase1 = 4,

        PhaseChange_1And2 = 5,
    }

    public static class Constant
    {
        public const float despawnDistance = 5000f;

        public static readonly float[] maxScale = [6f, 7.5f];
        public static readonly float[] minScale = [0.5f, 0.5f];
        public static readonly float[] spawnSlimeThreshold = [0.03f, 0.025f];
        public static readonly float[] spawnSlimePow = [0.3f, 0.5f];
    }

    /// <summary>
    /// 当前阶段。0，1，2.
    /// </summary>
    public int AI_CurrentPhase
    {
        get => (int)AnomalyNPC.anomalyAI[0];
        set => SetAnomalyAI(value, 0);
    }

    public AttackType AI_CurrentAttack
    {
        get => (AttackType)(int)AnomalyNPC.anomalyAI[1];
        set => SetAnomalyAI((int)value, 1);
    }

    public int AI_CurrentAttackPhase
    {
        get => (int)AnomalyNPC.anomalyAI[2];
        set => SetAnomalyAI(value, 2);
    }

    /// <summary>
    /// 小型计时器，用于攻击运行和AI转换。
    /// </summary>
    public int AI_CurrentAttackTimer
    {
        get => (int)AnomalyNPC.anomalyAI[3];
        set => SetAnomalyAI(value, 3);
    }

    public int AI_JewelSpawn
    {
        get => (int)AnomalyNPC.anomalyAI[4];
        set => SetAnomalyAI(value, 4);
    }

    public int AI_LastSpawnSlimeLife
    {
        get => (int)AnomalyNPC.anomalyAI[5];
        set => SetAnomalyAI(value, 5);
    }

    public int AI_TeleportTimer
    {
        get => (int)AnomalyNPC.anomalyAI[6];
        set => SetAnomalyAI(value, 6);
    }

    /// <summary>
    /// 王冠绿宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC AI_JewelEmerald
    {
        get => Main.npc[(int)AnomalyNPC.anomalyAI[7]];
        set => AnomalyNPC.anomalyAI[7] = value.whoAmI;
    }

    /// <summary>
    /// 王冠红宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC AI_JewelRuby
    {
        get => Main.npc[(int)AnomalyNPC.anomalyAI[8]];
        set => AnomalyNPC.anomalyAI[8] = value.whoAmI;
    }

    /// <summary>
    /// 王冠蓝宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>

    public NPC AI_JewelSapphire
    {
        get => Main.npc[(int)AnomalyNPC.anomalyAI[9]];
        set => AnomalyNPC.anomalyAI[9] = value.whoAmI;
    }

    public int AI_ChangedVelocityDirectionWhenJump
    {
        get => (int)AnomalyNPC.anomalyAI[10];
        set => AnomalyNPC.anomalyAI[10] = value;
    }
    #endregion

    public override int OverrideNPCType => NPCID.KingSlime;

    public override DisableCalamityMethods DisableCalamityMethods => DisableCalamityMethods.PreAI;

    public override bool DisableAI => true;

    public override void SetDefaults()
    {
        if (CAWorld.AnomalyUltramundane)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 1.5f);
            NPC.Calamity().DR += 0.1f;
        }

        AI_JewelEmerald = TOMain.DummyNPC;
        AI_JewelRuby = TOMain.DummyNPC;
        AI_JewelSapphire = TOMain.DummyNPC;
    }

    public override void OnSpawn(IEntitySource source)
    {
    }

    /* 弃用的最小化灾厄AI方法
    public void MinimizeCalamityAI()
    {
        NPC.ai[0] = -9999f; //处理计时器
        NPC.ai[1] = 7f; //禁用攻击选取（正常范围为0~6）
        NPC.ai[2] = 0f; //禁用传送触发
        NPC.ai[3] = -1f; //禁用史莱姆召唤

        NPC.Calamity().newAI[0] = 3f; //禁用水晶召唤

        NPC.localAI[1] = NPC.Center.X;
        NPC.localAI[2] = NPC.Center.Y;
        NPC.localAI[3] = 1f; //禁用初始攻击延迟

        NPC.netUpdate = true;
        NPC.SyncExtraAI();

        //还原FallSpeedBonus
        float lifeRatio = (float)NPC.life / NPC.lifeMax;
        bool bossRush_C = BossRushEvent.BossRushActive;
        bool masterMode_C = Main.masterMode || bossRush_C;
        bool death_C = CalamityWorld.death || bossRush_C;
        bool crystalAlive_C = lifeRatio >= 0.5f || NPC.AnyNPCs(ModContent.NPCType<KingSlimeJewelRuby>());
        if (NPC.velocity.Y > 0f)
        {
            float fallSpeedBonus = (bossRush_C ? 0.1f : death_C ? 0.05f : 0f) + (!crystalAlive_C ? 0.1f : 0f) + (masterMode_C ? 0.1f : 0f);
            NPC.velocity.Y -= fallSpeedBonus;
        }
    }*/

    public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
    {
        return base.PreDraw(spriteBatch, lightColor);
    }
}

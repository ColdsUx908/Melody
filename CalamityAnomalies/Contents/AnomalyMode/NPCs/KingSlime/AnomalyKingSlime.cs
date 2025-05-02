using CalamityAnomalies.Contents.AnomalyMode.NPCs;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Transoceanic;

namespace CalamityAnomalies.GlobalInstances.AnomalyBosses.KingSlime;

public partial class AnomalyKingSlime : AnomalyNPCOverride
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

    private enum AttackFinishStatus
    {

    }

    public static class Constant
    {
        public const float despawnDistance = 5000f;

        public static readonly float[] maxScale = [6f, 7.5f];
        public static readonly float[] minScale = [0.5f, 0.5f];
        public static readonly float[] spawnSlimeThreshold = [0.03f, 0.025f];
        public static readonly float[] spawnSlimePow = [0.3f, 0.5f];
        public static readonly float[] teleportRate = [0.97f, 0.95f];
    }

    /// <summary>
    /// 当前阶段。0，1，2.
    /// </summary>
    public int AI_CurrentPhase
    {
        get => (int)AnomalyNPC.AnomalyAI[0];
        set => SetAnomalyAI(value, 0);
    }

    public AttackType AI_CurrentAttack
    {
        get => (AttackType)(int)AnomalyNPC.AnomalyAI[1];
        set => SetAnomalyAI((int)value, 1);
    }

    public int AI_CurrentAttackPhase
    {
        get => (int)AnomalyNPC.AnomalyAI[2];
        set => SetAnomalyAI(value, 2);
    }

    /// <summary>
    /// 小型计时器，用于攻击运行和AI转换。
    /// </summary>
    public int AI_CurrentAttackTimer
    {
        get => (int)AnomalyNPC.AnomalyAI[3];
        set => SetAnomalyAI(value, 3);
    }

    public int AI_JewelSpawn
    {
        get => (int)AnomalyNPC.AnomalyAI[4];
        set => SetAnomalyAI(value, 4);
    }

    public int AI_LastSpawnSlimeLife
    {
        get => (int)AnomalyNPC.AnomalyAI[5];
        set => SetAnomalyAI(value, 5);
    }

    public int AI_TeleportTimer
    {
        get => (int)AnomalyNPC.AnomalyAI[6];
        set => SetAnomalyAI(value, 6);
    }

    /// <summary>
    /// 王冠绿宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC AI_JewelEmerald
    {
        get => Main.npc[(int)AnomalyNPC.AnomalyAI[7]];
        set => SetAnomalyAI(value.whoAmI, 7);
    }

    /// <summary>
    /// 王冠红宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC AI_JewelRuby
    {
        get => Main.npc[(int)AnomalyNPC.AnomalyAI[8]];
        set => SetAnomalyAI(value.whoAmI, 8);
    }

    /// <summary>
    /// 王冠蓝宝石实例。
    /// <br/>在 <see cref="SetDefaults"/> 中初始化为 <see cref="TOMain.DummyNPC"/>。
    /// </summary>
    public NPC AI_JewelSapphire
    {
        get => Main.npc[(int)AnomalyNPC.AnomalyAI[9]];
        set => SetAnomalyAI(value.whoAmI, 9);
    }

    public int AI_ChangedVelocityDirectionWhenJump
    {
        get => (int)AnomalyNPC.AnomalyAI[10];
        set => SetAnomalyAI(value, 10);
    }

    public float AI_TeleportScaleMultiplier
    {
        get => AnomalyNPC.AnomalyAI[11];
        set => SetAnomalyAI(value, 11);
    }

    public Vector2 AI_TeleportDestination
    {
        get => new(AnomalyNPC.AnomalyAI[12], AnomalyNPC.AnomalyAI[13]);
        set
        {
            SetAnomalyAI(value.X, 12);
            SetAnomalyAI(value.Y, 13);
        }
    }

    #endregion

    public override int OverrideNPCType => NPCID.KingSlime;

    public override bool AllowOrigMethod(OrigMethodType type) => type switch
    {
        OrigMethodType.AI => false,
        _ => true,
    };

    public override bool AllowOrigCalMethod(OrigCalMethodType type) => type switch
    {
        OrigCalMethodType.PreAI => false,
        _ => true,
    };

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

    public override void PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
    {
    }
}

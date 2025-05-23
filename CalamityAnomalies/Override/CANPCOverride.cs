using System;
using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.GlobalInstances.GlobalNPCs;
using CalamityAnomalies.IL;
using CalamityMod;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Transoceanic;
using Transoceanic.Core.ExtraGameData;
using Transoceanic.Core.GameData;
using Transoceanic.GlobalInstances;
using Transoceanic.GlobalInstances.GlobalNPCs;

namespace CalamityAnomalies.Override;

public enum OrigMethodType_CalamityGlobalNPC
{
    PreAI,
    GetAlpha,
    PreDraw,
    DrawBossBar,
}

public abstract class CANPCOverride : EntityOverride<NPC>
{
    /*
    private sealed class NPCContainer
    {
        public NPC NPC { get; set; }

        public int Index { get; set; }

        public TOGlobalNPC OceanNPC { get; set; }

        public CAGlobalNPC AnomalyNPC { get; set; }

        public CalamityGlobalNPC CalamityNPC { get; set; }

        public NPCContainer(NPC npc)
        {
            ArgumentNullException.ThrowIfNull(npc, nameof(npc));
            NPC = npc;
            Index = npc.whoAmI;
            OceanNPC = npc.Ocean();
            AnomalyNPC = npc.Anomaly();
            CalamityNPC = npc.Calamity();
        }
    }
    */

    #region 实成员
    public static int UltraIndex => CAWorld.AnomalyUltramundane.ToInt();

    /// <summary>
    /// Override对象当前应用的NPC。
    /// </summary>
    protected NPC NPC
    {
        get => field ?? TOMain.DummyNPC;
        set;
    } = null;

    /// <summary>
    /// Override对象当前应用的NPC的 <see cref="TOGlobalNPC"/> 实例。
    /// <br/>在值为 <see langword="null"/> 时会尝试定位 <see cref="NPC"/> 对应的实例。
    /// </summary>
    protected TOGlobalNPC OceanNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? field = NPC.Ocean() : NPC.Ocean());
        set;
    } = null;

    /// <summary>
    /// Override对象当前应用的NPC的 <see cref="CAGlobalNPC"/> 实例。
    /// <br/>在值为 <see langword="null"/> 时会尝试定位 <see cref="NPC"/> 对应的实例。
    /// </summary>
    protected CAGlobalNPC AnomalyNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? field = NPC.Anomaly() : NPC.Anomaly());
        set;
    } = null;

    /// <summary>
    /// Override对象当前应用的NPC的 <see cref="CalamityGlobalNPC"/> 实例。
    /// <br/>在值为 <see langword="null"/> 时会尝试定位 <see cref="NPC"/> 对应的实例。
    /// </summary>
    protected CalamityGlobalNPC CalamityNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? field = NPC.Calamity() : NPC.Calamity());
        set;
    } = null;

    public Player Target { get; private set; } = null;

    public bool TargetClosestIfInvalid(bool faceTarget = true, float distanceThreshold = 4000f)
    {
        bool result = NPC.TargetClosestIfInvalid(faceTarget, distanceThreshold);
        Target = result ? Main.player[NPC.target] : null;
        return result;
    }

    public override void Connect(NPC npc)
    {
        NPC = npc;
        OceanNPC = npc.Ocean();
        AnomalyNPC = npc.Anomaly();
        CalamityNPC = npc.Calamity();
    }

    public override void Disconnect()
    {
        NPC = null;
        OceanNPC = null;
        AnomalyNPC = null;
        CalamityNPC = null;
    }

    public int AI_Timer1
    {
        get => (int)AnomalyNPC.AnomalyAI[^3];
        set => AnomalyNPC.SetAnomalyAI(value, ^3);
    }

    public int AI_Timer2
    {
        get => (int)AnomalyNPC.AnomalyAI[^2];
        set => AnomalyNPC.SetAnomalyAI(value, ^2);
    }

    public int AI_Timer3
    {
        get => (int)AnomalyNPC.AnomalyAI[^1];
        set => AnomalyNPC.SetAnomalyAI(value, ^1);
    }
    #endregion

    #region 虚成员
    /// <summary>
    /// NPC是否应用Boss无敌帧。
    /// <br/>默认返回 <see langword="null"/>，即沿用默认设置。
    /// </summary>
    public virtual bool? UseBossImmunityCooldownID => null;

    /// <summary>
    /// 是否允许灾厄的相关方法执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC type) => true;

    #region Defaults
    /// <summary>
    /// 设置负数type的NPC的额外属性。
    /// </summary>
    public virtual void SetDefaultsFromNetId() { }
    #endregion

    #region AI
    /// <summary>
    /// AI。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止 <see cref="NPC.AI"/> 方法运行。</returns>
    public virtual bool PreAI() => true;

    /// <summary>
    /// AI。
    /// </summary>
    public virtual void AI() { }

    /// <summary>
    /// AI。
    /// </summary>
    public virtual void PostAI() { }
    #endregion

    #region Draw
    /// <summary>
    /// 设置NPC当前帧。
    /// </summary>
    /// <param name="frameHeight"></param>
    public virtual void FindFrame(int frameHeight) { }

    /// <summary>
    /// 获取绘制颜色。
    /// </summary>
    /// <param name="drawColor">原始绘制颜色。</param>
    /// <returns>返回 <see langword="null"/> 以使用默认颜色。</returns>
    public virtual Color? GetAlpha(Color drawColor) => null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="screenPos"></param>
    /// <param name="lightColor"></param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制NPC方法运行。</returns>
    public virtual bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor) => true;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="screenPos"></param>
    /// <param name="drawColor"></param>
    public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index">The index for NPCID.Sets.BossHeadTextures</param>
    public virtual void BossHeadSlot(ref int index) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rotation"></param>
    public virtual void BossHeadRotation(ref float rotation) { }

    /// <summary>
    /// Allows you to flip an NPC's boss head icon on the map.
    /// </summary>
    /// <param name="spriteEffects"></param>
    public virtual void BossHeadSpriteEffects(ref SpriteEffects spriteEffects) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newBar"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public virtual void PreDrawCalBossBar(On_BossHealthBarManager.BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newBar"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public virtual void PostDrawCalBossBar(On_BossHealthBarManager.BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) { }
    #endregion

    #region Active
    /// <summary>
    /// 生成时执行的代码。
    /// </summary>
    /// <param name="source"></param>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <summary>
    /// Whether or not to run the code for checking whether an NPC will remain active. Return false to stop the NPC from being despawned and to stop the NPC from counting towards the limit for how many NPCs can exist near a player. Returns true by default.
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckActive() => true;

    /// <summary>
    /// Whether or not an NPC should be killed when it reaches 0 health. You may program extra effects in this hook (for example, how Golem's head lifts up for the second phase of its fight). Return false to stop the NPC from being killed. Returns true by default.
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckDead() => true;

    /// <summary>
    /// Allows you to call OnKill on your own when the NPC dies, rather then letting vanilla call it on its own. Returns false by default.
    /// </summary>
    /// <returns>Return true to stop vanilla from calling OnKill on its own. Do this if you call OnKill yourself.</returns>
    public virtual bool SpecialOnKill() => false;

    /// <summary>
    /// Allows you to determine whether or not this NPC will do anything upon death (besides dying). This method can also be used to dynamically prevent specific item loot using <see cref="NPCLoader.blockLoot"/>, but editing the drop rules themselves is usually the better approach.
    /// <para/> Returning false will skip dropping loot, the <see cref="NPCLoader.OnKill(NPC)"/> methods, and logic setting boss flags (<see cref="NPC.DoDeathEvents"/>).
    /// <para/> Returns true by default. 
    /// </summary>
    /// <returns></returns>
    public virtual bool PreKill() => true;

    /// <summary>
    /// Allows you to make things happen when this NPC dies (for example, dropping items manually and setting ModSystem fields).
    /// <br/>This hook runs on the server/single player. For client-side effects, such as dust, gore, and sounds, see HitEffect.
    /// </summary>
    public virtual void OnKill() { }
    #endregion
    #endregion
}

public abstract class AnomalyNPCOverride : CANPCOverride
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

using System;
using System.Reflection;
using CalamityAnomalies.Net;
using CalamityMod;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core;
using Transoceanic.GlobalEntity.GlobalNPCs;
using TransoceanicCalamity.Core;
using TransoceanicCalamity.GlobalEntity.GlobalNPCs;

namespace CalamityAnomalies.NPCs;

[Flags]
public enum DisableCalamityMethods
{
    None = 0,
    PreAI = 1 << 0,
    AI = 1 << 1,
    PostAI = 1 << 2,
    PreDraw = 1 << 3,
    FindFrame = 1 << 4,
}

[Flags]
public enum HasCustomNPCMethods
{
    None = 0,
    SetDefaults = 1 << 0,
    AnomalyAI = 1 << 1,
    AI = 1 << 2,
    PostAI = 1 << 3,
    PreDraw = 1 << 4,
    FindFrame = 1 << 5,
}

public record NPCOverrideContainer(NPCOverride BehaviorOverride, HasCustomNPCMethods HasCustomMethods);

/// <summary>
/// 异象NPCAI覆写抽象类。
/// </summary>
public abstract class NPCOverride : ITOLoader
{
    #region 实成员
    /// <summary>
    /// Override对象当前应用的NPC。
    /// </summary>
    public NPC NPC
    {
        get => field ?? Main.npc[Main.maxNPCs];
        set;
    } = null;

    /// <summary>
    /// Override对象当前应用的NPC的 <see cref="TOGlobalNPC"/> 实例。
    /// <br/>在值为 <see langword="null"/> 时会尝试定位 <see cref="NPC"/> 对应的实例。
    /// </summary>
    public TOGlobalNPC OceanNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? field = NPC.Ocean() : Main.npc[Main.maxNPCs].Ocean());
        set;
    } = null;

    /// <summary>
    /// Override对象当前应用的NPC的 <see cref="TOCGlobalNPC"/> 实例。
    /// <br/>在值为 <see langword="null"/> 时会尝试定位 <see cref="NPC"/> 对应的实例。
    /// </summary>
    public TOCGlobalNPC OceanCalNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? field = NPC.OceanCal() : Main.npc[Main.maxNPCs].OceanCal());
        set;
    } = null;

    /// <summary>
    /// Override对象当前应用的NPC的 <see cref="CAGlobalNPC"/> 实例。
    /// <br/>在值为 <see langword="null"/> 时会尝试定位 <see cref="NPC"/> 对应的实例。
    /// </summary>
    public CAGlobalNPC AnomalyNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? field = NPC.Anomaly() : Main.npc[Main.maxNPCs].Anomaly());
        set;
    } = null;

    /// <summary>
    /// Override对象当前应用的NPC的 <see cref="CalamityGlobalNPC"/> 实例。
    /// <br/>在值为 <see langword="null"/> 时会尝试定位 <see cref="NPC"/> 对应的实例。
    /// </summary>
    public CalamityGlobalNPC CalamityNPC
    {
        get => field ?? (NPC.whoAmI < Main.maxNPCs ? field = NPC.Calamity() : Main.npc[Main.maxNPCs].Calamity());
        set;
    } = null;

    public int OverrideCount() => TOMain.ActiveNPCs.Count(k => k.type == OverrideNPCType);

    /// <summary>
    /// 判定世界上是否有且只有一个指定Boss存活。
    /// </summary>
    /// <returns></returns>
    public bool ExceptionalOverride() => OverrideCount() == 1;

    /// <summary>
    /// 更改 <see cref="AnomalyNPC"/> 的 <see cref="CAGlobalNPC.anomalyAI"/> 数据值。
    /// <br>含自动Net同步。</br>
    /// </summary>
    /// <param name="value">设置的值。</param>
    /// <param name="index">待设置的索引值。</param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void SetAnomalyAI(float value, int index)
    {
        AnomalyNPC.anomalyAI[index] = value;
        NPC.SyncAnomalyAI(index);
    }

    /// <summary>
    /// 尝试将Override对象连接到指定NPC。同步更新 <see cref="NPC"/>、<see cref="AnomalyNPC"/> 和 <see cref="CalamityNPC"/>。
    /// </summary>
    /// <param name="npc"></param>
    public void TryConnectWithNPC(NPC npc)
    {
        NPC = npc;
        OceanNPC = npc.Ocean();
        OceanCalNPC = npc.OceanCal();
        AnomalyNPC = npc.Anomaly();
        CalamityNPC = npc.Calamity();
    }

    /// <summary>
    /// 清除Override对象的NPC实例。同步更新 <see cref="NPC"/>、<see cref="AnomalyNPC"/> 和 <see cref="CalamityNPC"/>。
    /// <br>在 <see langword="return"/> 语句中使用，以同时终止方法。</br>
    /// </summary>
    public void ClearNPCInstances()
    {
        NPC = null;
        OceanNPC = null;
        OceanCalNPC = null;
        AnomalyNPC = null;
        CalamityNPC = null;
    }

    /// <summary>
    /// 判定指定NPCID是否已注册。
    /// </summary>
    /// <param name="npcID"></param>
    /// <returns></returns>
    public static bool NPCRegistered(int npcID, out NPCOverrideContainer container)
    {
        container = NPCOverrideSet[npcID];
        return container is not null;
    }

    /// <summary>
    /// 判定指定ModNPC类型是否已注册。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool NPCRegistered<T>(out NPCOverrideContainer container) where T : ModNPC => NPCRegistered(ModContent.NPCType<T>(), out container);
    #endregion

    #region 虚成员
    /// <summary>
    /// 覆写的目标NPCID。
    /// </summary>
    public abstract int OverrideNPCType { get; }

    /// <summary>
    /// 禁用灾厄方法类型。
    /// </summary>
    public abstract DisableCalamityMethods DisableCalamityMethods { get; }

    /// <summary>
    /// 是否禁用AI。
    /// <br/>影响 <see cref="CAGlobalNPC.PreAI(NPC)"/> 的返回值。
    /// </summary>
    public abstract bool DisableAI { get; }

    /// <summary>
    /// Whether the NPC should use the boss immunity cooldown slot. Defaults to true.
    /// </summary>
    public virtual bool UseBossImmunityCooldownID => true;

    /// <summary>
    /// Use this to set custom defaults for the npc. This runs after every other mod's.
    /// </summary>
    /// <param name="npc">The NPC</param>
    public virtual void SetDefaults() { }

    /// <summary>
    /// 生成时执行的代码。
    /// </summary>
    /// <param name="source"></param>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <summary>
    /// 异象模式AI。
    /// </summary>
    public virtual void AnomalyAI() { }

    /// <summary>
    /// Use this to set the NPCs current frame.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="frameHeight"></param>
    public virtual void FindFrame(int frameHeight) { }

    /// <summary>
    /// Use this to perform custom drawing for the NPC. Return false to stop the game drawing the NPC as well. Returns true by default.
    /// </summary>
    /// <param name="spriteBatch">The spritebatch to draw with.</param>
    /// <param name="lightColor">The light color at the NPC's center</param>
    /// <returns></returns>
    public virtual bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => true;

    /// <summary>
    /// Whether or not to run the code for checking whether an NPC will remain active. Return false to stop the NPC from being despawned
    /// and to stop the NPC from counting towards the limit for how many NPCs can exist near a player. Returns true by default.
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckDead() => true;

    public virtual void BossHeadSlot(ref int index) { }
    #endregion

    #region 加载器
    internal static NPCOverrideContainer[] NPCOverrideSet { get; private set; }

    void ITOLoader.PostSetupContent()
    {
        NPCOverrideSet = new SetFactory(ContentSamples.NpcsByNetId.Count).CreateCustomSet<NPCOverrideContainer>(null);
        Type typeNPCOverride = typeof(NPCOverride);
        foreach ((Type type, NPCOverride npcOverride) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<NPCOverride>(CAMain.Assembly))
        {
            HasCustomNPCMethods hasCustomMethods = HasCustomNPCMethods.None;

            if (type.HasRealMethod("AnomalyAI", TOMain.UniversalBindingFlags, out _))
                hasCustomMethods |= HasCustomNPCMethods.AnomalyAI;

            if (type.HasRealMethod("FindFrame", TOMain.UniversalBindingFlags, out _))
                hasCustomMethods |= HasCustomNPCMethods.FindFrame;

            NPCOverrideContainer container = new(npcOverride, hasCustomMethods);

            NPCOverrideSet[npcOverride.OverrideNPCType] = container;
        }
    }

    void ITOLoader.OnModUnload()
    {
        NPCOverrideSet = null;
    }
    #endregion
}

using System;
using System.Collections.Generic;
using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.IL;
using CalamityAnomalies.Utilities;
using CalamityMod;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core;
using Transoceanic.Core.GameData;
using Transoceanic.Core.IL;
using Transoceanic.GlobalInstances.GlobalNPCs;

namespace CalamityAnomalies.Contents.AnomalyNPCs;

public enum OrigMethodType
{
    AI,
    FindFrame,
    Draw,
}

public enum OrigCalMethodType
{
    PreAI,
    PostAI,
    GetAlpha,
    PreDraw,
    DrawBossBar,
}

/// <summary>
/// 异象NPCAI覆写抽象类。
/// </summary>
public abstract class AnomalyNPCOverride
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
    /// 更改 <see cref="AnomalyNPC"/> 的 <see cref="CAGlobalNPC.AnomalyAI"/> 数据值。
    /// <br>含自动Net同步。</br>
    /// </summary>
    /// <param name="value">设置的值。</param>
    /// <param name="index">待设置的索引值。</param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void SetAnomalyAI(float value, int index)
    {
        AnomalyNPC.AnomalyAI[index] = value;
        AnomalyNPC.ShouldSyncAnomalyAI[index] = true;
        //NPC.SyncAnomalyAI(index);
    }

    /// <summary>
    /// 尝试将Override对象连接到指定NPC。同步更新 <see cref="NPC"/>、<see cref="AnomalyNPC"/> 和 <see cref="CalamityNPC"/>。
    /// </summary>
    /// <param name="npc"></param>
    public void TryConnectWithNPC(NPC npc)
    {
        NPC = npc;
        OceanNPC = npc.Ocean();
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
        AnomalyNPC = null;
        CalamityNPC = null;
    }
    #endregion

    #region 虚成员
    /// <summary>
    /// 覆写的目标NPCID。
    /// </summary>
    public abstract int OverrideNPCType { get; }

    /// <summary>
    /// 是否允许原NPC类的方法执行。
    /// </summary>
    public abstract bool AllowOrigMethod(OrigMethodType type);

    /// <summary>
    /// 是否允许灾厄的相关方法执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowOrigCalMethod(OrigCalMethodType type) => true;

    /// <summary>
    /// Whether the NPC should use the boss immunity cooldown slot. Defaults to true.
    /// </summary>
    public virtual bool UseBossImmunityCooldownID => true;

    /// <summary>
    /// Use this to set custom defaults for the npc. This runs after every other mod's.
    /// <br/><see cref="NPC"/> 等属性会在调用前自动更新，调用后自动清除。
    /// </summary>
    public virtual void SetDefaults() { }

    /// <summary>
    /// 生成时执行的代码。
    /// <br/><see cref="NPC"/> 等属性会在调用前自动更新，调用后自动清除。
    /// </summary>
    /// <param name="source"></param>
    public virtual void OnSpawn(IEntitySource source) { }

    /// <summary>
    /// 异象模式AI。
    /// <br/><see cref="NPC"/> 等属性会在调用前自动更新，调用后自动清除。
    /// </summary>
    public virtual void AnomalyAI() { }

    /// <summary>
    /// 设置NPC当前帧。
    /// <br/><see cref="NPC"/> 等属性会在调用前自动更新，调用后自动清除。
    /// </summary>
    /// <param name="frameHeight"></param>
    public virtual void FindFrame(int frameHeight) { }

    /// <summary>
    /// 获取绘制颜色。
    /// <br/>返回 <see langword="null"/> 以使用默认颜色。
    /// <br/><see cref="NPC"/> 等属性会在调用前自动更新，调用后自动清除。
    /// </summary>
    /// <param name="drawColor">原始绘制颜色。</param>
    /// <returns></returns>
    public virtual Color? GetAlpha(Color drawColor) => null;

    /// <summary>
    /// 
    /// <br/><see cref="NPC"/> 等属性会在调用前自动更新，调用后自动清除。
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="screenPos"></param>
    /// <param name="lightColor"></param>
    public virtual void PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor) { }

    /// <summary>
    /// 
    /// <br/><see cref="NPC"/> 等属性会在调用前自动更新，调用后自动清除。
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="screenPos"></param>
    /// <param name="drawColor"></param>
    public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    /// <summary>
    /// 
    /// <br/><see cref="NPC"/> 等属性会在调用前自动更新，调用后自动清除。
    /// </summary>
    /// <param name="newBar"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public virtual void PreDrawCalBossBar(CANPCHook.BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) { }

    /// <summary>
    /// 
    /// <br/><see cref="NPC"/> 等属性会在调用前自动更新，调用后自动清除。
    /// </summary>
    /// <param name="bar"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public virtual void PostDrawCalBossBar(CANPCHook.BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) { }

    /// <summary>
    /// Whether or not to run the code for checking whether an NPC will remain active. Return false to stop the NPC from being despawned
    /// and to stop the NPC from counting towards the limit for how many NPCs can exist near a player. Returns true by default.
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckDead() => true;

    public virtual void BossHeadSlot(ref int index) { }

    #endregion
}

public class AnomalyNPCOverrideHelper : ITOLoader
{
    internal static Dictionary<int, AnomalyNPCOverride> NPCOverrideSet { get; } = [];

    /// <summary>
    /// 判定指定NPCID是否已注册。
    /// </summary>
    /// <param name="npcID"></param>
    /// <returns></returns>
    public static bool Registered(int npcID, out AnomalyNPCOverride anomalyNPCOverride) => NPCOverrideSet.TryGetValue(npcID, out anomalyNPCOverride);

    /// <summary>
    /// 判定指定ModNPC类型是否已注册。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool Registered<T>(out AnomalyNPCOverride anomalyNPCOverride) where T : ModNPC => Registered(ModContent.NPCType<T>(), out anomalyNPCOverride);

    void ITOLoader.PostSetupContent()
    {
        foreach (AnomalyNPCOverride anomalyNPCOverride in TOReflectionUtils.GetTypeInstancesDerivedFrom<AnomalyNPCOverride>(CAMain.Assembly))
            NPCOverrideSet[anomalyNPCOverride.OverrideNPCType] = anomalyNPCOverride;
    }

    void ITOLoader.OnModUnload()
    {
        NPCOverrideSet.Clear();
    }
}

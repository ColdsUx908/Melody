using System.Reflection;
using System.Runtime.CompilerServices;
using CalamityAnomalies.Configs;
using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.GlobalInstances.GlobalItems;
using CalamityAnomalies.UI;
using CalamityMod;
using CalamityMod.Items;
using CalamityMod.NPCs;
using CalamityMod.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.ExtraGameData;
using Transoceanic.GameData.Utilities;

namespace CalamityAnomalies;

#region NPC
public enum OrigMethodType_CalamityGlobalNPC
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class CANPCOverride : NPCOverride
{
    public CAGlobalNPC AnomalyNPC { get; private set; } = null;

    public CalamityGlobalNPC CalamityNPC { get; private set; } = null;

    public override void Connect(NPC npc)
    {
        base.Connect(npc);
        AnomalyNPC = npc.Anomaly();
        CalamityNPC = npc.Calamity();
    }

    public override void Disconnect()
    {
        base.Disconnect();
        AnomalyNPC = null;
        CalamityNPC = null;
    }

    /// <summary>
    /// 是否允许灾厄的相关方法执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC type) => true;

    /// <summary>
    /// 在绘制灾厄的Boss血条之前调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制血条方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreDrawCalBossBar(BetterBossHealthBar.BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) => true;

    /// <summary>
    /// 在绘制灾厄的Boss血条之后调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    public virtual void PostDrawCalBossBar(BetterBossHealthBar.BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) { }

    /// <summary>
    /// 使用此方法可以修改灾厄Boss血条的高度。
    /// <br/>默认为 <c>70</c>。
    /// </summary>
    public virtual int CustomCalBossBarHeight(BetterBossHealthBar.BetterBossHPUI newBar) => 70;
}

public abstract class CANPCOverride<T> : CANPCOverride where T : ModNPC
{
    public T ModNPC { get; private set; } = null;

    public override int OverrideType => ModContent.NPCType<T>();

    public override void Connect(NPC npc)
    {
        base.Connect(npc);
        ModNPC = npc.GetModNPC<T>();
    }

    public override void Disconnect()
    {
        base.Disconnect();
        ModNPC = null;
    }
}

public abstract class AnomalyNPCOverride : CANPCOverride
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class AnomalyNPCOverride<T> : CANPCOverride<T> where T : ModNPC
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class CANPCTweak : CANPCOverride
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.TweaksEnabled;
}

public abstract class CANPCTweak<T> : CANPCOverride<T> where T : ModNPC
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.TweaksEnabled;
}
#endregion

#region Projectile
public enum OrigMethodType_CalamityGlobalProjectile
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class CAProjectileOverride : ProjectileOverride
{
    public CAGlobalProjectile AnomalyProjectile { get; private set; } = null;

    public CalamityGlobalProjectile CalamityProjectile { get; private set; } = null;

    public override void Connect(Projectile projectile)
    {
        base.Connect(projectile);
        AnomalyProjectile = projectile.Anomaly();
        CalamityProjectile = projectile.Calamity();
    }

    /// <summary>
    /// 是否允许灾厄的相关方法执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile type) => true;
}

public abstract class CAProjectileOverride<T> : CAProjectileOverride where T : ModProjectile
{
    public T ModProjectile { get; private set; } = null;

    public override int OverrideType => ModContent.ProjectileType<T>();

    public override void Connect(Projectile projectile)
    {
        base.Connect(projectile);
        ModProjectile = projectile.ModProjectile<T>();
    }

    public override void Disconnect()
    {
        base.Disconnect();
        ModProjectile = null;
    }
}

public abstract class AnomalyProjectileOverride : CAProjectileOverride
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class AnomalyProjecileOverride<T> : CAProjectileOverride<T> where T : ModProjectile
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class CAProjectileTweak : CAProjectileOverride
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.TweaksEnabled;
}

public abstract class CAProjectileTweak<T> : CAProjectileOverride<T> where T : ModProjectile
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.TweaksEnabled;
}
#endregion

#region Item
public abstract class CAItemOverride : ItemOverride
{
    public CAGlobalItem AnomalyItem { get; private set; } = null;

    public CalamityGlobalItem CalamityItem { get; private set; } = null;

    public override void Connect(Item item)
    {
        base.Connect(item);
        AnomalyItem = item.Anomaly();
        CalamityItem = item.Calamity();
    }

    public override void Disconnect()
    {
        base.Disconnect();
        AnomalyItem = null;
        CalamityItem = null;
    }
}

public abstract class CAItemOverride<T> : CAItemOverride where T : ModItem
{
    public T ModItem { get; private set; } = null;

    public override int OverrideType => ModContent.ItemType<T>();

    public override void Connect(Item item)
    {
        base.Connect(item);
        ModItem = item.GetModItem<T>();
    }

    public override void Disconnect()
    {
        base.Disconnect();
        ModItem = null;
    }
}

public abstract class CAItemTweak : CAItemOverride
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.TweaksEnabled;
}

public abstract class CAItemTweak<T> : CAItemOverride<T> where T : ModItem
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.TweaksEnabled;
}
#endregion

public sealed class CAOverrideHelper : ITOLoader
{
    internal static EntityOverrideDictionary<NPC, CANPCOverride> NPCOverrides { get; } = [];

    internal static EntityOverrideDictionary<Projectile, CAProjectileOverride> ProjectileOverrides { get; } = [];

    internal static EntityOverrideDictionary<Item, CAItemOverride> ItemOverrides { get; } = [];

    void ITOLoader.PostSetupContent()
    {
        Assembly assembly = CAMain.Assembly;
        NPCOverrides.FillOverrides(assembly);
        ProjectileOverrides.FillOverrides(assembly);
        ItemOverrides.FillOverrides(assembly);
    }

    void ITOLoader.OnModUnload()
    {
        NPCOverrides.Clear();
        ProjectileOverrides.Clear();
        ItemOverrides.Clear();
    }
}

public static class CAOverrideExtensions
{
    public static bool TryGetOverride(this NPC npc, out CANPCOverride npcOverride, [CallerMemberName] string methodName = null!) => CAOverrideHelper.NPCOverrides.TryGetOverride(npc, methodName, out npcOverride);

    public static bool TryGetOverride(this Projectile projectile, out CAProjectileOverride projectileOverride, [CallerMemberName] string methodName = null!) => CAOverrideHelper.ProjectileOverrides.TryGetOverride(projectile, methodName, out projectileOverride);

    public static bool TryGetOverride(this Item item, out CAItemOverride itemOverride, [CallerMemberName] string methodName = null!) => CAOverrideHelper.ItemOverrides.TryGetOverride(item, methodName, out itemOverride);
}
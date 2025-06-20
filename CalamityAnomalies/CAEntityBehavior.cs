using CalamityAnomalies.UI;

namespace CalamityAnomalies;

#region Player
public abstract class CAPlayerBehavior : PlayerBehavior
{
    public CAPlayer AnomalyPlayer { get; private set; } = null;

    public CalamityPlayer CalamityPlayer { get; private set; } = null;

    public NewCalamityPlayer NewCalamityPlayer { get; private set; } = null;

    public override void Connect(Player player)
    {
        base.Connect(player);
        AnomalyPlayer = player.Anomaly();
        CalamityPlayer = player.Calamity();
        NewCalamityPlayer = player.NewCalamity();
    }
}
#endregion

#region NPC
public enum OrigMethodType_CalamityGlobalNPC
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class CANPCBehavior : NPCBehavior
{
    public CAGlobalNPC AnomalyNPC { get; private set; } = null;

    public CalamityGlobalNPC CalamityNPC { get; private set; } = null;

    public override void Connect(NPC npc)
    {
        base.Connect(npc);
        AnomalyNPC = npc.Anomaly();
        CalamityNPC = npc.Calamity();
    }

    /// <summary>
    /// 是否允许灾厄的相关方法执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC type) => true;

    /// <summary>
    /// 在更新灾厄的Boss血条之前调用。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止默认的更新血条方法运行（除对 <see cref="BetterBossHealthBar.BetterBossHPUI.Valid"/> 属性的更新之外）。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreUpdateCalBossBar(BetterBossHealthBar.BetterBossHPUI newBar) => true;

    /// <summary>
    /// 在更新灾厄的Boss血条之后调用。
    /// </summary>
    public virtual void PostUpdateCalBossBar(BetterBossHealthBar.BetterBossHPUI newBar) { }

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
    public virtual void ModifyCalBossBarHeight(BetterBossHealthBar.BetterBossHPUI newBar, ref int height) { }
}

public abstract class CANPCBehavior<T> : CANPCBehavior where T : ModNPC
{
    public static Type Type { get; } = typeof(T);

    public T ModNPC { get; private set; } = null;

    public override int ApplyingType => ModContent.NPCType<T>();

    public override void Connect(NPC npc)
    {
        base.Connect(npc);
        ModNPC = npc.GetModNPC<T>();
    }
}

public abstract class AnomalyNPCBehavior : CANPCBehavior
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class AnomalyNPCBehavior<T> : CANPCBehavior<T> where T : ModNPC
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class CANPCTweak : CANPCBehavior
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAMain.Tweak;
}

public abstract class CANPCTweak<T> : CANPCBehavior<T> where T : ModNPC
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAMain.Tweak;
}
#endregion NPC

#region Projectile
public enum OrigMethodType_CalamityGlobalProjectile
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class CAProjectileBehavior : ProjectileBehavior
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

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC npc, ref NPC.HitModifiers modifiers, float baseDR,
        ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier)
    { }
}

public abstract class CAProjectileBehavior<T> : CAProjectileBehavior where T : ModProjectile
{
    public static Type Type { get; } = typeof(T);

    public T ModProjectile { get; private set; } = null;

    public override int ApplyingType => ModContent.ProjectileType<T>();

    public override void Connect(Projectile projectile)
    {
        base.Connect(projectile);
        ModProjectile = projectile.GetModProjectile<T>();
    }
}

public abstract class AnomalyProjectileBehavior : CAProjectileBehavior
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class AnomalyProjecileBehavior<T> : CAProjectileBehavior<T> where T : ModProjectile
{
    public override decimal Priority => 10m;

    public override bool ShouldProcess => CAWorld.Anomaly;
}

public abstract class CAProjectileTweak : CAProjectileBehavior
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAMain.Tweak;
}

public abstract class CAProjectileTweak<T> : CAProjectileBehavior<T> where T : ModProjectile
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAMain.Tweak;
}
#endregion Projectile

#region Item
public abstract class CAItemBehavior : ItemBehavior
{
    public CAGlobalItem AnomalyItem { get; private set; } = null;

    public CalamityGlobalItem CalamityItem { get; private set; } = null;

    public override void Connect(Item item)
    {
        base.Connect(item);
        AnomalyItem = item.Anomaly();
        CalamityItem = item.Calamity();
    }

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC npc, Player player, ref NPC.HitModifiers modifiers, float baseDR,
        ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier)
    { }
}

public abstract class CAItemBehavior<T> : CAItemBehavior where T : ModItem
{
    public static Type Type { get; } = typeof(T);

    public T ModItem { get; private set; } = null;

    public override int ApplyingType => ModContent.ItemType<T>();

    public override void Connect(Item item)
    {
        base.Connect(item);
        ModItem = item.GetModItem<T>();
    }
}

public abstract class CAItemTweak : CAItemBehavior
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAMain.Tweak;
}

public abstract class CAItemTweak<T> : CAItemBehavior<T> where T : ModItem
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAMain.Tweak;
}
#endregion Item

public class CABehaviorHelper : ITOLoader
{
    internal static NontypedEntityBehaviorSet<Player, CAPlayerBehavior> PlayerBehaviors { get; } = [];

    internal static TypedEntityBehaviorSet<NPC, CANPCBehavior> NPCBehaviors { get; } = [];

    internal static TypedEntityBehaviorSet<Projectile, CAProjectileBehavior> ProjectileBehaviors { get; } = [];

    internal static TypedEntityBehaviorSet<Item, CAItemBehavior> ItemBehaviors { get; } = [];

    void ITOLoader.PostSetupContent()
    {
        Assembly assembly = CAMain.Assembly;
        PlayerBehaviors.FillSet(assembly);
        NPCBehaviors.FillSet(assembly);
        ProjectileBehaviors.FillSet(assembly);
        ItemBehaviors.FillSet(assembly);
    }

    void ITOLoader.OnModUnload()
    {
        PlayerBehaviors.Clear();
        NPCBehaviors.Clear();
        ProjectileBehaviors.Clear();
        ItemBehaviors.Clear();
    }
}
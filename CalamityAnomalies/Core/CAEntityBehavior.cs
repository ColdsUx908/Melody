namespace CalamityAnomalies.Core;

#region General Behavior
public abstract class CAPlayerBehavior : PlayerBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    public CAPlayer AnomalyPlayer => _entity.Anomaly();

    public CalamityPlayer CalamityPlayer => _entity.Calamity();
}

public abstract class CAGlobalNPCBehavior : GlobalNPCBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    /// <summary>
    /// 在更新灾厄的Boss血条之前调用。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止默认的更新血条方法运行（除对 <see cref="BetterBossHPUI.Valid"/> 属性的更新之外）。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreUpdateCalBossBar(NPC npc, BetterBossHPUI newBar, bool hasSingle) => true;

    /// <summary>
    /// 在更新灾厄的Boss血条之后调用。
    /// </summary>
    public virtual void PostUpdateCalBossBar(NPC npc, BetterBossHPUI newBar, bool hasSingle) { }

    /// <summary>
    /// 在绘制灾厄的Boss血条之前调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制血条方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreDrawCalBossBar(NPC npc, BetterBossHPUI newBar, SpriteBatch spriteBatch, ref int x, ref int y, bool hasSingle) => true;

    /// <summary>
    /// 在绘制灾厄的Boss血条之后调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    public virtual void PostDrawCalBossBar(NPC npc, BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y, bool hasSingle) { }
}

public abstract class CAGlobalProjectileBehavior : GlobalProjectileBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class CAGlobalItemBehavior : GlobalItemBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(Item item, NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class CAPlayerBehavior2 : CAPlayerBehavior
{
    public override bool ShouldProcess => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalNPCBehavior2 : CAGlobalNPCBehavior
{
    public override bool ShouldProcess => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalProjectileBehavior2 : CAGlobalProjectileBehavior
{
    public override bool ShouldProcess => CAServerConfig.Instance.Contents;
}

public abstract class CAGlobalItemBehavior2 : CAGlobalItemBehavior
{
    public override bool ShouldProcess => CAServerConfig.Instance.Contents;
}
#endregion General Behavior

#region Single Behavior
#region NPC
public enum OrigMethodType_CalamityGlobalNPC
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class CASingleNPCBehavior : SingleNPCBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    public CAGlobalNPC AnomalyNPC => _entity.Anomaly();

    public CalamityGlobalNPC CalamityNPC => _entity.Calamity();

    /// <summary>
    /// 是否允许灾厄的相关方法执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC method) => true;

    /// <summary>
    /// 在更新灾厄的Boss血条之前调用。
    /// </summary>
    /// <returns>返回 <see langword="false"/> 以阻止默认的更新血条方法运行（除对 <see cref="BetterBossHPUI.Valid"/> 属性的更新之外）。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreUpdateCalBossBar(BetterBossHPUI newBar) => true;

    /// <summary>
    /// 在更新灾厄的Boss血条之后调用。
    /// </summary>
    public virtual void PostUpdateCalBossBar(BetterBossHPUI newBar) { }

    /// <summary>
    /// 在绘制灾厄的Boss血条之前调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    /// <returns>返回 <see langword="false"/> 以阻止默认的绘制血条方法运行。默认返回 <see langword="true"/>。</returns>
    public virtual bool PreDrawCalBossBar(BetterBossHPUI newBar, SpriteBatch spriteBatch, ref int x, ref int y) => true;

    /// <summary>
    /// 在绘制灾厄的Boss血条之后调用。
    /// </summary>
    /// <param name="x">绘制位置左上角的X坐标。</param>
    /// <param name="y">绘制位置左上角的Y坐标。</param>
    public virtual void PostDrawCalBossBar(BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y) { }
}

public abstract class CASingleNPCBehavior<T> : CASingleNPCBehavior where T : ModNPC
{
    public static readonly Type Type = typeof(T);

    public T ModNPC => _entity.GetModNPC<T>();

    public override int ApplyingType => ModContent.NPCType<T>();
}

public abstract class AnomalyNPCBehavior : CASingleNPCBehavior
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => CAWorld.Anomaly && (AnomalyNPC?.ShouldRunAnomalyAI ?? false);
}

public abstract class AnomalyNPCBehavior<T> : CASingleNPCBehavior<T> where T : ModNPC
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => CAWorld.Anomaly && (AnomalyNPC?.ShouldRunAnomalyAI ?? false);
}

public abstract class CANPCTweak : CASingleNPCBehavior
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.Contents;
}

public abstract class CANPCTweak<T> : CASingleNPCBehavior<T> where T : ModNPC
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.Contents;
}
#endregion NPC

#region Projectile
public enum OrigMethodType_CalamityGlobalProjectile
{
    PreAI,
    GetAlpha,
    PreDraw,
}

public abstract class CASingleProjectileBehavior : SingleProjectileBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    public CAGlobalProjectile AnomalyProjectile => _entity.Anomaly();

    public CalamityGlobalProjectile CalamityProjectile => _entity.Calamity();

    /// <summary>
    /// 是否允许灾厄的相关方法执行。
    /// <br/>默认返回 <see langword="true"/>，即全部允许。
    /// </summary>
    public virtual bool AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile type) => true;

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public abstract class CASingleProjectileBehavior<T> : CASingleProjectileBehavior where T : ModProjectile
{
    public static readonly Type Type = typeof(T);

    public T ModProjectile => _entity.GetModProjectile<T>();

    public override int ApplyingType => ModContent.ProjectileType<T>();
}

public abstract class AnomalyProjectileBehavior : CASingleProjectileBehavior
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => CAWorld.Anomaly && (AnomalyProjectile?.ShouldRunAnomalyAI ?? false);
}

public abstract class AnomalyProjecileBehavior<T> : CASingleProjectileBehavior<T> where T : ModProjectile
{
    public override decimal Priority => 100m;

    public override bool ShouldProcess => CAWorld.Anomaly && (AnomalyProjectile?.ShouldRunAnomalyAI ?? false);
}

public abstract class CAProjectileTweak : CASingleProjectileBehavior
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.Contents;
}

public abstract class CAProjectileTweak<T> : CASingleProjectileBehavior<T> where T : ModProjectile
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.Contents;
}
#endregion Projectile

#region Item
public abstract class CASingleItemBehavior : SingleItemBehavior
{
    public sealed override CAMain Mod => CAMain.Instance;

    public CAGlobalItem AnomalyItem => _entity.Anomaly();

    public CalamityGlobalItem CalamityItem => _entity.Calamity();

    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }

    public void ApplyCATweakColorToDamage() => OceanItem.TooltipDictionary.Modify(null, "Damage", l => l.OverrideColor = CAMain.GetGradientColor(0.25f));
}

public abstract class CASingleItemBehavior<T> : CASingleItemBehavior where T : ModItem
{
    public static readonly Type Type = typeof(T);

    public T ModItem => _entity.GetModItem<T>();

    public override int ApplyingType => ModContent.ItemType<T>();
}

public abstract class CAItemTweak : CASingleItemBehavior
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.Contents;
}

public abstract class CAItemTweak<T> : CASingleItemBehavior<T> where T : ModItem
{
    public override decimal Priority => 5m;

    public override bool ShouldProcess => CAServerConfig.Instance.Contents;
}
#endregion Item
#endregion Single Behavior

#region Single Behavior Handler
public sealed class CASingleNPCBehaviorHandler : SingleNPCBehaviorHandler<CASingleNPCBehavior>
{
    public override CAMain Mod => CAMain.Instance;

    public override bool ShouldProcess => CAServerConfig.Instance.Contents;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<NPC, CASingleNPCBehavior> BehaviorSet => CASingleBehaviorHelper.NPCBehaviors;
}

public sealed class CASingleProjectileBehaviorHandler : SingleProjectileBehaviorHandler<CASingleProjectileBehavior>
{
    public override CAMain Mod => CAMain.Instance;

    public override bool ShouldProcess => CAServerConfig.Instance.Contents;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<Projectile, CASingleProjectileBehavior> BehaviorSet => CASingleBehaviorHelper.ProjectileBehaviors;
}

public sealed class CASingleItemBehaviorHandler : SingleItemBehaviorHandler<CASingleItemBehavior>
{
    public override CAMain Mod => CAMain.Instance;

    public override bool ShouldProcess => CAServerConfig.Instance.Contents;

    public override decimal Priority => 50m;

    protected override SingleEntityBehaviorSet<Item, CASingleItemBehavior> BehaviorSet => CASingleBehaviorHelper.ItemBehaviors;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (TryGetBehavior(item, out CASingleItemBehavior itemBehavior))
        {
            CAItemTooltipModifier.Instance.Update(tooltips);
            itemBehavior.ModifyTooltips(tooltips);
            CAItemTooltipModifier.Instance.AddCATooltip(l =>
            {
                l.Text = Language.GetTextValue(CAMain.TweakLocalizationPrefix + "TweakIdentifier");
                l.OverrideColor = TOMain.CelestialColor;
            });
        }
    }
}

public sealed class CASingleBehaviorHelper : IResourceLoader
{
    internal static readonly SingleEntityBehaviorSet<NPC, CASingleNPCBehavior> NPCBehaviors = new();

    internal static readonly SingleEntityBehaviorSet<Projectile, CASingleProjectileBehavior> ProjectileBehaviors = new();

    internal static readonly SingleEntityBehaviorSet<Item, CASingleItemBehavior> ItemBehaviors = new();

    void IResourceLoader.PostSetupContent()
    {
        Assembly assembly = CAMain.Assembly;
        NPCBehaviors.FillSet(assembly);
        ProjectileBehaviors.FillSet(assembly);
        ItemBehaviors.FillSet(assembly);
    }

    void IResourceLoader.OnModUnload()
    {
        NPCBehaviors.Clear();
        ProjectileBehaviors.Clear();
        ItemBehaviors.Clear();
    }
}
#endregion Single Behavior Handler

#region Detour
public sealed class CalamityGlobalNPCBehaviorDetour : CACalamityGlobalNPCDetour
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalNPC self, NPC npc)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreAI)) &&
            !npcBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC.PreAI))
            return true;

        return orig(self, npc);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalNPC self, NPC npc, Color drawColor)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.GetAlpha)) &&
            !npcBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC.GetAlpha))
            return null;

        return orig(self, npc, drawColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalNPC self, NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreDraw))
            && npcBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalNPC.PreDraw))
            return true;

        return orig(self, npc, spriteBatch, screenPos, drawColor);
    }
}

public sealed class CalamityGlobalProjectileBehaviorDetour : CACalamityGlobalProjectileDetour
{
    public override bool Detour_PreAI(Orig_PreAI orig, CalamityGlobalProjectile self, Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.PreAI))
            && !projectileBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.PreAI))
            return true;

        return orig(self, projectile);
    }

    public override Color? Detour_GetAlpha(Orig_GetAlpha orig, CalamityGlobalProjectile self, Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.GetAlpha))
            && !projectileBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.GetAlpha))
            return null;

        return orig(self, projectile, lightColor);
    }

    public override bool Detour_PreDraw(Orig_PreDraw orig, CalamityGlobalProjectile self, Projectile projectile, ref Color lightColor)
    {
        if (projectile.TryGetBehavior(out CASingleProjectileBehavior projectileBehavior, nameof(CASingleProjectileBehavior.PreDraw))
            && !projectileBehavior.AllowOrigCalMethod(OrigMethodType_CalamityGlobalProjectile.PreDraw))
            return true;

        return orig(self, projectile, ref lightColor);
    }
}
#endregion Detour